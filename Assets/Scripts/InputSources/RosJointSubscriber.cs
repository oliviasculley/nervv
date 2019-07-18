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

public class RosJointSubscriber : InputSource {

    // Input Properties
    public new bool InputEnabled {
        get { return _inputEnabled; }
        set {
            // If disabling input
            if (!(_inputEnabled = value)) {
                OnDisable();
                return;
            }

            // Else, enabling input
            // Init vars
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
    }

    [Header("Input Settings")]
        [Tooltip("Topic to subscribe from")]
        public string Topic = "/joint_states";
        [Tooltip("Axes to bind")]
        public string[] axesName;

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
        private Coroutine rosConnect;
        private RosSocket rosSocket;
        private string topicID = "";    // Used to unsubscribe from topic on close

    private void OnEnable() {
        // Safety checks
        if (machineToSet == null) {
            Debug.LogError("Machine null, disabling self...");
            _inputEnabled = false;
        }
        foreach (string s in axesName) {
            if (string.IsNullOrEmpty(s)) {
                Debug.LogError("AxisName null, disabling self...");
                _inputEnabled = false;
            }
        }

        if (_inputEnabled)
            InputEnabled = true;
    }

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

    private void Start() {
        // Add self to InputManager
        Debug.Assert(InputManager.Instance != null, "[MTConnect] Could not get ref to InputManager!");
        if (!InputManager.Instance.AddInput(this))
            Debug.LogError("[MTConnect] Could not add self to InputManager!");
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
            for (int i = 0; i < message.name.Length && i < axesName.Length; i++)
                machineToSet.Axes.Find(x => x.Name == axesName[i]).ExternalValue = 
                    (float)message.position[i] * Mathf.Rad2Deg;
    }

    private void OnConnected(object sender, EventArgs e) {
        Debug.Log("Connected to RosBridge: " + URL);
    }

    private void OnDisconnected(object sender, EventArgs e) {
        Debug.Log("Disconnected from RosBridge: " + URL);
    }

    #endregion
}
