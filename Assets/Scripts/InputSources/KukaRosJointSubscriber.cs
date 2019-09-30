// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Protocols;
using RosSharp.RosBridgeClient.Messages.Standard;

// NERVV
using NERVV;
using System.Net.Sockets;

public class KukaRosJointSubscriber : InputSource {
    #region Static
    public enum ProtocolSelection { WebSocketSharp, WebSocketNET };
    #endregion

    #region Classes

    /// <summary>Used to apply transformations on values coming from ROS</summary>
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

    [Serializable]
    public class KukaJoint : Message {
        public const string RosMessageName = "kuka_robot/joint_angles";
        public double a1;
        public double a2;
        public double a3;
        public double a4;
        public double a5;
        public double a6;

        public KukaJoint() { }
    }
    #endregion

    #region ROS Settings
    [Tooltip("ROS topic name"), Header("ROS Settings")]
    public string Topic = "";

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
    /// If machineToSet is null or AxisValueAdjustment axisID is null
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// If matching ProtocolSelection is not found
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

        if (PrintDebugMessages && rosConnect != null)
            Debug.LogWarning("Socket not null! Overwriting...");
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
                throw new NotSupportedException("Could not get find matching protocol for RosSocket!");
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

        // Unsubscribe and close
        if (rosSocket != null) {
            if (!string.IsNullOrEmpty(topicID))
                rosSocket.Unsubscribe(topicID);
            rosSocket.Close();
            rosSocket = null;
        }

        base.OnDisable();
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
                if (PrintDebugMessages)
                    Debug.LogError("SocketException, trying again in one second...\n" +
                        "----------------------------------------------\n" + e.Message);
            }
            if (rosSocket != null) break;
            yield return new WaitForSeconds(1);
        }

        // Wait until socket is active
        yield return new WaitUntil(() => rosSocket.protocol.IsAlive());

        // Subscribe to topic once socket is active
        topicID = rosSocket.Subscribe<KukaJoint>(
            Topic,
            ReceiveMessage,
            0   // the rate(in ms in between messages) at which to throttle the topics
        );
    }

    /// <summary>Called when RosSocket receieves messages</summary>
    /// <param name="message">Incoming joint angles</param>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when reflection does not find field with name
    /// </exception>
    /// <exception cref="InvalidCastException">
    /// Thrown when found field does not match double
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when axis is not found for axis ID in axesToBind
    /// </exception>
    protected void ReceiveMessage(KukaJoint message) {
        if (!InputEnabled) return;

        for (int i = 0; i < axesToBind.Length; i++) {
            // Get field
            var fieldInfo = typeof(KukaJoint).GetField("a" + (i + 1));
            if (fieldInfo == null)
                throw new KeyNotFoundException("Could not find field with name: " + fieldInfo);

            // Get value
            float? val = null;
            try {
                val = (float)(double)fieldInfo.GetValue(message);
            } catch (InvalidCastException e) {
                throw new InvalidCastException(
                    "Invalid cast exception, check value type!\n" +
                    "-----------------------------------------\n", e);
            }

            // Set Axis
            Debug.Assert(val.HasValue);
            Machine.Axis a = machineToSet.Axes.Find(x => x.ID == axesToBind[i].ID);
            if (a == null)
                throw new ArgumentNullException("Axis not found for axis ID: " + axesToBind[i].ID);
            a.ExternalValue = (val.Value + axesToBind[i].Offset) * axesToBind[i].ScaleFactor;
            if (PrintDebugMessages) Debug.Log("Kuka ROS Input: " + a.Name + " has " + val.Value);
        }
    }

    /// <summary>Callback when websocket is connected</summary>
    protected void OnConnected(object sender, EventArgs e) {
        if (PrintDebugMessages)
            Debug.Log("Kuka ROS Joint Subscriber connected to RosBridge: " + URL);
    }

    /// <summary>Callback when websocket is disconnected</summary>
    protected void OnDisconnected(object sender, EventArgs e) {
        if (PrintDebugMessages)
            Debug.Log("Kuka ROS Joint Subscriber disconnected from RosBridge: " + URL);
    }
    #endregion
}
