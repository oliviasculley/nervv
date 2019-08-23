// System
using System;
using System.Collections;

// Unity
using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Protocols;
using RosSharp.RosBridgeClient.Messages.Sensor;
using RosSharp.RosBridgeClient.Messages.Standard;

// NERVV
using NERVV;

public class ROSJointPublisher : OutputSource {
    #region Static
    public enum ProtocolSelection { WebSocketSharp, WebSocketNET, None };
    #endregion

    #region Settings
    /// <summary>Name of topic to publish</summary>
    [Tooltip("Name of topic to publish"), Header("Settings")]
    public string Topic = "/vr_joint_states";

    /// <summary>Machine to set angles from topic</summary>
    [Tooltip("Machine to set angles from topic")]
    public Machine machineToPublish;

    /// <summary>Interval in seconds to poll</summary>
    [Tooltip("Interval in seconds to poll")]
    public float pollInterval = 0.25f;

    /// <summary>URL of RosBridgeClient websocket to subscribe from</summary>
    [Tooltip("URL of RosBridgeClient websocket to subscribe from")]
    public string URL = "";

    /// <summary>Protocol to use to connect to RosBridgeClient</summary>
    [Tooltip("Protocol to use to connect to RosBridgeClient")]
    public ProtocolSelection Protocol = ProtocolSelection.WebSocketNET;

    /// <summary>Serialization mode of RosBridgeClient</summary>
    [Tooltip("Serialization mode of RosBridgeClient")]
    public RosSocket.SerializerEnum SerializationMode = RosSocket.SerializerEnum.JSON;
    #endregion

    #region Vars
    Coroutine rosConnect = null;
    RosSocket rosSocket = null;
    /// <summary>Used to unsubscribe from topic on close</summary>
    string topicID = null;
    float timeToTrigger = 0.0f;
    #endregion

    #region Unity methods
    /// <summary>Safety checks</summary>
    protected override void Start() {
        base.Start();

        // Safety checks
        if (machineToPublish == null) {
            Debug.LogError("Machine null, disabling self...");
            OutputEnabled = false;
        }
    }

    /// <summary>Initializes socket connection when object is enabled</summary>
    void OnEnable() {
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

        Debug.Assert(p != null);

        // OnConnected and OnClosed event handlers
        p.OnConnected += OnConnected;
        p.OnClosed += OnDisconnected;

        // Start coroutine
        rosConnect = StartCoroutine(ConnectToRos(p));
    }

    /// <summary>Destroys socket connection if object is disabled</summary>
    void OnDisable() {
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

    /// <summary>Checks for time to trigger</summary>
    void Update() {
        if (OutputEnabled && UnityEngine.Time.time > timeToTrigger) {
            // Set new time to trigger
            timeToTrigger = UnityEngine.Time.time + pollInterval;
            SendJointsMessage();
        }
    }
    #endregion

    #region Methods
    /// <summary>Connects to ROS given a IProtocol object with settings</summary>
    /// <param name="p">IProtocol object with appropriate settings</param>
    /// <returns>Unity Coroutine</returns>
    IEnumerator ConnectToRos(IProtocol p) {
        rosSocket = new RosSocket(p, RosSocket.SerializerEnum.JSON);

        // Wait until socket is active
        yield return new WaitUntil(() => rosSocket.protocol.IsAlive());

        // Advertise Doosan joint angles topic once socket is active
        topicID = rosSocket.Advertise<JointState>(Topic);
    }

    /// <summary>Called when RosSocket receieves messages</summary>
    void SendJointsMessage() {
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

    /// <summary>Callback when socket is connected</summary>
    void OnConnected(object sender, EventArgs e) {
        Debug.Log("Connected to RosBridge: " + URL);
    }

    /// <summary>Callback when socket is disconnected</summary>
    void OnDisconnected(object sender, EventArgs e) {
        Debug.Log("Disconnected from RosBridge: " + URL);
    }
    #endregion
}