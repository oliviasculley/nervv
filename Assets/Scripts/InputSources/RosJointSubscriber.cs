// System
using System;
using System.Collections;
using System.Net.Sockets;

// Unity Engine
using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Protocols;
using RosSharp.RosBridgeClient.MessageTypes.Sensor;
using RosSharp.RosBridgeClient.MessageTypes.Std;

// NERVV
using NERVV;

/// <summary>
/// Example of a ROS joint subscriber using Ros#'s default joint message
/// </summary>
public class RosJointSubscriber : InputSource {
    #region Classes
    /// <summary>
    /// Serializable class, converts implicit ordering to axisIDs.
    /// Also allows user to adjust for scale/units as well.
    /// </summary>
    [Serializable]
    public class AxisValueAdjustment {
        /// <summary>ID of Axis to map to</summary>
        [Tooltip("ID of Axis to map to")]
        public string ID;

        /// <summary>
        /// Offset used to correct between particular input's
        /// worldspace to chosen external worldspac
        /// </summary>
        [Tooltip("Offset used to correct between particular " +
            "input's worldspace to chosen external worldspace")]
        public float Offset = 0;

        /// <summary>
        /// Scale factor used to correct between particular input's
        /// worldspace to chosen external worldspace
        /// </summary>
        [Tooltip("Scale factor used to correct between particular " +
            "input's worldspace to chosen external worldspace")]
        public float ScaleFactor = 1;
    }
    #endregion

    #region Static
    public enum ProtocolSelection { WebSocketSharp, WebSocketNET };
    #endregion

    #region ROS Settings
    [Tooltip("Topic to subscribe from"), Header("ROS Settings")]
    public string Topic = "/joint_states";

    [Tooltip("URL of RosBridgeClient websocket to subscribe from")]
    public string URL = "";

    [Tooltip("Protocol to use to connect to RosBridgeClient")]
    public ProtocolSelection Protocol = ProtocolSelection.WebSocketNET;

    [Tooltip("Serialization mode of RosBridgeClient")]
    public RosSocket.SerializerEnum SerializationMode = RosSocket.SerializerEnum.JSON;
    #endregion

    #region NERVV Settings
    [Tooltip("Machine to set angles from /joint_states"), Header("NERVV Settings")]
    public Machine machineToSet;

    [Tooltip("Axes to bind")]
    public AxisValueAdjustment[] axesToBind;
    #endregion

    #region Vars
    Coroutine rosConnect = null;
    RosSocket rosSocket = null;
    /// <summary>Used to unsubscribe from topic on close</summary>
    string topicID = "";
    #endregion

    #region Unity Methods
    /// <summary>Initializes websocket connection</summary>
    /// <exception cref="ArgumentNullException">
    /// Thrown if machineToSet is null or axisID in axesToBind is null
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown if no matching protocol handler
    /// </exception>
    protected override void OnEnable() {
        // Safety checks
        if (machineToSet == null) {
            InputEnabled = false;
            throw new ArgumentNullException("Machine null, disabling self...");
        }
        foreach (AxisValueAdjustment a in axesToBind)
            if (string.IsNullOrEmpty(a.ID)) {
                InputEnabled = false;
                throw new ArgumentNullException("Axis ID is null, disabling self...");
            }

        base.OnEnable();

        if (rosConnect != null)
            LogWarning("Socket not null! Overwriting...");
        rosConnect = null;

        // Get protocol object
        IProtocol p = null;
        switch (Protocol) {
            case ProtocolSelection.WebSocketSharp:
                p = new WebSocketSharpProtocol(URL);
                break;

            case ProtocolSelection.WebSocketNET:
                p = new WebSocketNetProtocol(URL);
                break;

            default:
                throw new NotSupportedException(
                    $"Could not get find matching protocol for RosSocket: {p.GetType()}!");
        }
        Debug.Assert(p != null);

        // OnConnected and OnClosed event handlers
        p.OnConnected += OnConnected;
        p.OnClosed += OnDisconnected;

        // Start coroutine
        rosConnect = StartCoroutine(ConnectToRos(p));
    }

    /// <summary>Disable websocket connection</summary>
    protected override void OnDisable() {
        // Stop rosConnect coroutine if still running
        if (rosConnect != null)
            StopCoroutine(rosConnect);

        if (!string.IsNullOrEmpty(topicID))
            rosSocket.Unsubscribe(topicID);

        // Attempt to close socket
        for (int i = 0; i < 50 && rosSocket != null; i++) {
            rosSocket.Close();
            rosSocket = null;
        }

        base.OnDisable();   // Remove self from InputManager
    }
    #endregion

    #region Methods
    /// <summary>Connects to ROS given a IProtocol object with settings</summary>
    /// <param name="p">IProtocol object with appropriate settings</param>
    /// <returns>Unity Coroutine</returns>
    protected IEnumerator ConnectToRos(IProtocol p) {
        for (int i = 0; i < 50 && rosSocket == null; i++) {
            try {
                rosSocket = new RosSocket(p, SerializationMode);
            } catch (SocketException e) {
                LogError(
                    "SocketException, trying again in one second...\n" +
                    "----------------------------------------------\n" +
                    $"{e.Message}\n{e.StackTrace}");
            }
            if (rosSocket != null) break;
            yield return new WaitForSeconds(1);
        }

        // Wait until socket is active
        yield return new WaitUntil(() => rosSocket.protocol.IsAlive());

        // Subscribe to topic once socket is active
        topicID = rosSocket.Subscribe<JointState>(
            Topic,
            ReceiveMessage,
            0   // the rate(in ms in between messages) at which to throttle the topics
        );
    }

    /// <summary>Called when RosSocket receieves messages</summary>
    /// <param name="message">Incoming joint angles</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if Axis is not found for AxisID
    /// </exception>
    protected void ReceiveMessage(JointState message) {
        if (!InputEnabled) return;

        for (int i = 0; i < message.name.Length && i < axesToBind.Length; i++) {
            Machine.Axis a = machineToSet.Axes.Find(x => x.ID == axesToBind[i].ID);
            if (a == null)
                throw new ArgumentNullException($"Axis not found for axis ID: {axesToBind[i].ID}!");

            Log($"ROS Subscriber: {a.Name} has {message.position[i]}");
            a.ExternalValue = ((float)message.position[i] + axesToBind[i].Offset) *
                axesToBind[i].ScaleFactor;
        }
    }

    /// <summary>Callback when websocket is connected</summary>
    /// <param name="sender">Unused</param>
    /// <param name="e">Unused</param>
    protected void OnConnected(object sender, EventArgs e) =>
        Log($"Connected to RosBridge: {URL}");

    /// <summary>Callback when websocket is disconnected</summary>
    /// /// <param name="sender">Unused</param>
    /// <param name="e">Unused</param>
    protected void OnDisconnected(object sender, EventArgs e) =>
        Log($"Disconnected from RosBridge: {URL}");
    #endregion
}
