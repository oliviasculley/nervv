using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Protocols;
using RosSharp.RosBridgeClient.Messages.Sensor;

public class RosSubscriber : InputSource {

    [Header("Properties")]
    [SerializeField] private bool _inputActive = true;   // Is this input currently enabled?

    [Header("Input Settings")]
    public new string name;
    [Tooltip("Topic to subscribe from")]
    public string Topic = "/joint_states";

    [Header("Settings")]
    [Tooltip("URL of RosBridgeClient websocket to subscribe from")]
    public string URL = "";
    public enum ProtocolSelection { WebSocketSharp, WebSocketNET };
    [Tooltip("Protocol to use to connect to RosBridgeClient")]
    public ProtocolSelection Protocol = ProtocolSelection.WebSocketNET;
    [Tooltip("Serialization mode of RosBridgeClient")]
    public RosSocket.SerializerEnum SerializationMode = RosSocket.SerializerEnum.JSON;

    // Private vars
    private Coroutine rosConnect;
    private RosSocket rosSocket;
    private ManualResetEvent isConnected = new ManualResetEvent(false);

    private void OnEnable() {
        // Init vars
        rosConnect = null;
        name = "RosSubscriber JSON: " + URL;

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
                break;
        }

        if (p != null) {
            // OnConnected and OnClosed event handlers
            p.OnConnected += OnConnected;
            p.OnClosed += OnDisconnected;

            // Start coroutine
            rosConnect = StartCoroutine(ConnectToRos(p));
        }
    }

    private void OnDisable() {
        // Stop rosConnect coroutine if still running
        if (rosConnect != null)
            StopCoroutine(rosConnect);
        rosConnect = null;

        rosSocket.Close();
    }

    public RosSubscriber() : base("RosSubscriber JSON", false) { }

    #region Private methods

    /// <summary>
    /// Connects to ROS given a IProtocol object with settings
    /// </summary>
    /// <param name="p">IProtocol object with appropriate settings</param>
    /// <returns>Unity Coroutine</returns>
    private IEnumerator ConnectToRos(IProtocol p) {
        // Wait until socket is active
        yield return new WaitUntil(() => 
            (rosSocket = new RosSocket(p, RosSocket.SerializerEnum.JSON)) != null
        );

        // Subscribe to topic once socket is active
        rosSocket.Subscribe<JointState>(
            Topic,
            ReceiveMessage,
            0                               // the rate(in ms in between messages) at which to throttle the topics
        );
    }

    /// <summary>
    /// Called when RosSocket receieves messages
    /// </summary>
    /// <param name="message"></param>
    private void ReceiveMessage(JointState message) {
        throw new NotImplementedException();
    }

    private void OnConnected(object sender, EventArgs e) {
        Debug.Log("Connected to RosBridge: " + URL);
    }

    private void OnDisconnected(object sender, EventArgs e) {
        Debug.Log("Disconnected from RosBridge: " + URL);
    }

    #endregion
}
