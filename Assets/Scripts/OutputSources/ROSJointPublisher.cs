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
using System.Net.Sockets;

public class ROSJointPublisher : OutputSource {
    #region Static
    /// <summary>Used to select a protocol to initialize RosSocket</summary>
    public enum ProtocolSelection { WebSocketSharp, WebSocketNET, None };
    #endregion

    #region ROS Settings
    /// <summary>Name of topic to publish</summary>
    [Tooltip("Name of topic to publish"), Header("ROS Settings")]
    public string Topic = "/vr_joint_states";

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

    #region NERVV Settings
    /// <summary>Machine to set angles from topic</summary>
    [Tooltip("Machine to set angles from topic"), Header("NERVV Settings")]
    public Machine MachineToPublish;

    /// <summary>Interval in seconds to poll</summary>
    [Tooltip("Interval in seconds to poll")]
    public float PollInterval = 0.25f;
    #endregion

    #region Vars
    Coroutine RosConnect = null;
    RosSocket RosSocket = null;
    /// <summary>Used to unsubscribe from topic on close</summary>
    string TopicID = null;
    float TimeToTrigger = 0.0f;
    #endregion

    #region Unity methods
    /// <summary>Safety checks</summary>
    /// <exception cref="ArgumentNullException">
    /// If machineToPublish is null
    /// </exception>
    protected override void Start() {
        base.Start();

        // Safety checks
        if (MachineToPublish == null) {
            OutputEnabled = false;
            throw new ArgumentNullException("Machine null, disabling self...");
        }
    }

    /// <summary>Initializes socket connection when object is enabled</summary>
    /// <exception cref="NotSupportedException">
    /// If matching ProtocolSelection is not found
    /// </exception>
    void OnEnable() {
        if (PrintDebugMessages && RosConnect != null)
            Debug.LogWarning("Socket not null! Overwriting...");
        RosConnect = null;

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
                    "Could not get find matching protocol for RosSocket!");
        }
        Debug.Assert(p != null);

        // OnConnected and OnClosed event handlers
        p.OnConnected += OnConnected;
        p.OnClosed += OnDisconnected;

        // Start coroutine
        RosConnect = StartCoroutine(ConnectToRos(p));
    }

    /// <summary>Destroys socket connection if object is disabled</summary>
    protected override void OnDisable() {
        // Stop rosConnect coroutine if still running
        if (RosConnect != null)
            StopCoroutine(RosConnect);

        // Stop Publishing and close
        if (RosSocket != null) {
            try {
                if (!string.IsNullOrEmpty(TopicID)) {
                    RosSocket.Unadvertise(TopicID);
                    TopicID = null;
                }
                RosSocket.Close();
            } catch (SocketException e) {
                if (PrintDebugMessages)
                    Debug.LogError("Socket was closed while trying to unadvertise topic");
            } finally {
                RosSocket = null;
            }
        }

        base.OnDisable(); // Remove from OutputManager
    }

    /// <summary>Checks for time to trigger</summary>
    void Update() {
        if (OutputEnabled && UnityEngine.Time.time > TimeToTrigger) {
            // Set new time to trigger
            TimeToTrigger = UnityEngine.Time.time + PollInterval;
            SendJointsMessage(MachineToPublish, RosSocket, TopicID);
        }
    }
    #endregion

    #region Methods
    /// <summary>Connects to ROS given a IProtocol object with settings</summary>
    /// <param name="p">IProtocol object with appropriate settings</param>
    /// <returns>Unity Coroutine</returns>
    IEnumerator ConnectToRos(IProtocol p) {
        RosSocket = new RosSocket(p, RosSocket.SerializerEnum.JSON);

        // Wait until socket is active
        yield return new WaitUntil(() => RosSocket.protocol.IsAlive());

        // Advertise Doosan joint angles topic once socket is active
        TopicID = RosSocket.Advertise<JointState>(Topic);
    }

    /// <summary>Callback when socket is connected</summary>
    void OnConnected(object sender, EventArgs e) {
        if (PrintDebugMessages)
            Debug.Log("ROSJointPublisher connected to RosBridge: " + URL);
    }

    /// <summary>Callback when socket is disconnected</summary>
    void OnDisconnected(object sender, EventArgs e) {
        if (PrintDebugMessages)
            Debug.Log("ROSJointPublisher disconnected from RosBridge: " + URL);
    }
    #endregion

    #region Static Methods
    /// <summary>Called when RosSocket receieves messages</summary>
    /// <remarks>RosSocket and topicID must be set</remarks>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="machineToPublish"/>, <paramref name="rosSocket"/>, or
    /// <paramref name="topicID"/> is null.
    /// </exception>
    protected static void SendJointsMessage(
        Machine machineToPublish,
        RosSocket rosSocket,
        string topicID) {
        if (machineToPublish == null)
            throw new ArgumentNullException("Machine to publish is null!");
        if (rosSocket == null)
            throw new ArgumentNullException("RosSocket is null!");
        if (topicID == null)
            throw new ArgumentNullException("No topicID!");

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
    #endregion
}