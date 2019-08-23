// System
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

// Unity
using Newtonsoft.Json;
using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Protocols;
using RosSharp.RosBridgeClient.Services;
using RosSharp.RosBridgeClient.Messages.Standard;

// NERVV
using NERVV;

/// <summary>
/// Implements Doosan's MoveJoint service.
/// Details can be found at http://wiki.ros.org/doosan-robotics?action=AttachFile&do=get&target=Doosan_Robotics_ROS_Manual_ver0.92_190508A%28EN.%29.pdf
///</summary>
public class DoosanROSJointService : OutputSource {
    #region Static
    public enum ProtocolSelection { WebSocketSharp, WebSocketNET };
    #endregion

    #region Settings
    /// <summary>Name of service to call</summary>
    [Tooltip("Name of service to call"), Header("Settings")]
    public string ServiceName = "/dsrm0609/motion/move_joint";

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
    RosSocket rosSocket = null;
    string serviceID = null;
    float timeToTrigger = 0.0f;
    #endregion

    #region Unity methods
    /// <summary>Safety checks and initialization</summary>
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
        if (rosSocket != null)
            Debug.LogWarning("Socket not null! Overwriting...");
        rosSocket = null;

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
        rosSocket = new RosSocket(p, RosSocket.SerializerEnum.JSON);
    }

    /// <summary>Destroys socket connection if object is disabled</summary>
    void OnDisable() {
        // Stop socket and close
        if (rosSocket != null) {
            serviceID = null;
            rosSocket.Close();
        }
    }

    /// <summary>Check if need to publish message</summary>
    void Update() {
        if (OutputEnabled && UnityEngine.Time.time > timeToTrigger) {
            // Set new time to trigger
            timeToTrigger = UnityEngine.Time.time + pollInterval;
            SendJointsMessage();
        }
    }
    #endregion

    #region Methods
    /// <summary>Sends message to service to move joints to specified location</summary>
    void SendJointsMessage() {
        // Safety checks
        if (!OutputEnabled)
            return;
        if (!rosSocket.protocol.IsAlive()) {
            Debug.LogError("RosSocket is not active!");
            return;
        }
        if (!string.IsNullOrEmpty(serviceID)) {
            Debug.Log("ServiceID not null, not calling again");
            return;
        }

        // Create new JointState message
        MoveJointRequest message = new MoveJointRequest {
            pos = new float[machineToPublish.Axes.Count],
            vel = 75,
            acc = 0,
            time = 0,
            radius = 0,
            mode = 0,
            blendType = 0,
            syncType = 0
        };

        // Set joint angles
        for (int i = 0; i < machineToPublish.Axes.Count; i++)
            message.pos[i] = machineToPublish.Axes[i].ExternalValue;

        // Call move joint service
        Debug.Log(serviceID = rosSocket.CallService<MoveJointRequest, MoveJointResponse>(
            ServiceName,
            VerifySuccess,
            message
        ));
    }

    /// <summary>Callback with response from Doosan MoveJoint Service</summary>
    void VerifySuccess(MoveJointResponse r) {
        if (!r.success)
            Debug.LogWarning("Could not successfully move angles to new joint!");
        serviceID = null;
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
