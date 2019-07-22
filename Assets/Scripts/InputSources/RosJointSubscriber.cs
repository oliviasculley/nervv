using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Protocols;
using RosSharp.RosBridgeClient.Messages.Sensor;
using Newtonsoft.Json;
using RosSharp.RosBridgeClient.Messages.Standard;

using MTConnectVR;

public class RosJointSubscriber : InputSource {

    [Header("Input Settings")]

    [Tooltip("Topic to subscribe from")]
    public string Topic = "/joint_states";
    [Tooltip("Axes to bind")]
    public AxisValueAdjustment[] axesToBind;

    [Header("Settings")]

    [Tooltip("Machine to set angles from /joint_states")]
    public Machine machineToSet;
    [Tooltip("URL of RosBridgeClient websocket to subscribe from")]
    public string URL = "";
    public enum ProtocolSelection { WebSocketSharp, WebSocketNET };
    [Tooltip("Protocol to use to connect to RosBridgeClient")]
    public ProtocolSelection Protocol = ProtocolSelection.WebSocketNET;
    [Tooltip("Serialization mode of RosBridgeClient")]
    public RosSocket.SerializerEnum SerializationMode = RosSocket.SerializerEnum.JSON;

    // Private vars

    private Coroutine rosConnect = null;
    private RosSocket rosSocket = null;
    private string topicID = "";    // Used to unsubscribe from topic on close

    /// <summary>
    /// Initializes input with InputManager, does error checking that won't change
    /// </summary>
    protected override void Start() {
        base.Start();
        // Safety checks
        if (machineToSet == null) {
            Debug.LogError("Machine null, disabling self...");
            InputEnabled = false;
        }
        foreach (AxisValueAdjustment a in axesToBind)
            if (string.IsNullOrEmpty(a.ID)) {
                Debug.LogError("Axis ID is null, disabling self...");
                InputEnabled = false;
            }
    }

    /// <summary>
    /// Initialize websocket connection
    /// </summary>
    private void OnEnable() {
        if (rosConnect != null)
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
                Debug.LogError("Could not get find matching protocol for RosSocket!");
                return;
        }

        Debug.Assert(p != null, "Could not initialize protocol!");

        // OnConnected and OnClosed event handlers
        p.OnConnected += OnConnected;
        p.OnClosed += OnDisconnected;

        // Start coroutine
        rosConnect = StartCoroutine(ConnectToRos(p));
    }

    /// <summary>
    /// Disable websocket connection
    /// </summary>
    private void OnDisable() {
        // Stop rosConnect coroutine if still running
        if (rosConnect != null)
            StopCoroutine(rosConnect);

        // Unsubscribe and close
        if (rosSocket != null) {
            if (!string.IsNullOrEmpty(topicID))
                rosSocket.Unsubscribe(topicID);
            rosSocket.Close();
        }
    }

    #region Private methods

    /// <summary>
    /// Connects to ROS given a IProtocol object with settings
    /// </summary>
    /// <param name="p">IProtocol object with appropriate settings</param>
    /// <returns>Unity Coroutine</returns>
    private IEnumerator ConnectToRos(IProtocol p) {
        rosSocket = new RosSocket(p, RosSocket.SerializerEnum.JSON);

        // Wait until socket is active
        yield return new WaitUntil(() => rosSocket.protocol.IsAlive());

        // Subscribe to topic once socket is active
        topicID = rosSocket.Subscribe<JointState>(
                Topic,
                ReceiveMessage,
                0   // the rate(in ms in between messages) at which to throttle the topics
            );
    }

    /// <summary>
    /// Called when RosSocket receieves messages
    /// </summary>
    /// <param name="message"></param>
    private void ReceiveMessage(JointState message) {
        if (InputEnabled)
            for (int i = 0; i < message.name.Length && i < axesToBind.Length; i++)
                machineToSet.Axes.Find(x => x.ID == axesToBind[i].ID).ExternalValue =
                    (((float)message.position[i] * Mathf.Rad2Deg) + axesToBind[i].Offset) * axesToBind[i].ScaleFactor;
    }

    private void OnConnected(object sender, EventArgs e) {
        Debug.Log("Connected to RosBridge: " + URL);
    }

    private void OnDisconnected(object sender, EventArgs e) {
        Debug.Log("Disconnected from RosBridge: " + URL);
    }

    #endregion

    #region AxisValueAdjustment Convenience Class

    [System.Serializable]
    public class AxisValueAdjustment {
        [Tooltip("ID of Axis to map to")]
        public string ID;

        [Tooltip("Offset used to correct between particular input's worldspace to chosen external worldspace")]
        public float Offset;

        [Tooltip("Scale factor used to correct between particular input's worldspace to chosen external worldspace")]
        public float ScaleFactor;
    }

    #endregion
}
