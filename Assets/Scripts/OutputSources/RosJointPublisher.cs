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

public class RosJointPublisher : OutputSource {

    [Header("Settings")]

    [Tooltip("Name of topic to publish")]
    public string Topic = "/vr_joint_states";
    [Tooltip("Machine to set angles from topic")]
    public Machine machineToPublish;
    [Tooltip("Interval in seconds to poll")]
    public float pollInterval = 0.25f;
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
    private string topicID = null;    // Used to unsubscribe from topic on close
    float timeToTrigger = 0.0f;

    protected override void Start() {
        base.Start();

        // Safety checks
        if (machineToPublish == null) {
            Debug.LogError("Machine null, disabling self...");
            OutputEnabled = false;
        }
    }

    /// <summary>
    /// Initializes socket connection when object is enabled
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
    /// Destroys socket connection if object is disabled
    /// </summary>
    private void OnDisable() {
        // Stop rosConnect coroutine if still running
        if (rosConnect != null)
            StopCoroutine(rosConnect);

        // Stop Publishing and close
        if (rosSocket != null) {
            if (!string.IsNullOrEmpty(topicID)) {
                rosSocket.Unadvertise(topicID);
                topicID = null;
            }
                
            rosSocket.Close();
        }
    }

    private void Update() {
        // Check if time to trigger
        if (OutputEnabled && UnityEngine.Time.time > timeToTrigger) {
            // Set new time to trigger
            timeToTrigger = UnityEngine.Time.time + pollInterval;
            SendJointsMessage();
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

        // Advertise Doosan joint angles topic once socket is active
        topicID = rosSocket.Advertise<JointState>(Topic);
    }

    /// <summary>
    /// Called when RosSocket receieves messages
    /// </summary>
    private void SendJointsMessage() {
        // Safety checks
        if (!OutputEnabled)
            return;
        if (rosSocket == null) {
            Debug.LogError("RosSocket is null!");
            return;
        }
        if (topicID == null) {
            Debug.LogError("No topicID!");
            return;
        }
        
        // Create new JointState message
        JointState message = new JointState {
            header = new Header(),
            position = new double[machineToPublish.Axes.Count],
            name = new string[machineToPublish.Axes.Count],
            velocity = new double[machineToPublish.Axes.Count],
            effort = new double[machineToPublish.Axes.Count]
        };

        // Set joint angles
        for (int i = 0; i < machineToPublish.Axes.Count; i++) {
            message.name[i] = machineToPublish.Axes[i].Name;
            message.position[i] = machineToPublish.Axes[i].ExternalValue;
            message.velocity[i] = 0;
            message.effort[i] = 0;
        }
        
        rosSocket.Publish(topicID, message);
    }

    private void OnConnected(object sender, EventArgs e) {
        Debug.Log("Connected to RosBridge: " + URL);
    }

    private void OnDisconnected(object sender, EventArgs e) {
        Debug.Log("Disconnected from RosBridge: " + URL);
    }

    #endregion
}
