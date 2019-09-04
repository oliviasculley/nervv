// System
using System;
using System.Collections;

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

    public bool PrintLogMessages = false;
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
    /// <summary>Initializes input with InputManager</summary>
    protected override void Start() {
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

        base.Start();
    }

    /// <summary>Initialize websocket connection</summary>
    protected void OnEnable() {
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

    /// <summary>Disable websocket connection</summary>
    protected void OnDisable() {
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
    /// <param name="message"></param>
    protected void ReceiveMessage(KukaJoint message) {
        if (InputEnabled) {
            for (int i = 0; i < axesToBind.Length; i++) {
                var fieldInfo = typeof(KukaJoint).GetField("a" + (i + 1));
                Debug.Assert(fieldInfo != null);
                float? val = null;
                try {
                    val = (float)(double)fieldInfo.GetValue(message);
                } catch (InvalidCastException e) {
                    Debug.LogError("Invalid cast exception, check value type!\n" +
                        "-----------------------------------------\n" + e.Message);
                }
                Debug.Assert(val.HasValue);
                Machine.Axis a = machineToSet.Axes.Find(x => x.ID == axesToBind[i].ID);
                Debug.Assert(a != null);

                if (PrintLogMessages) Debug.Log("Kuka ROS Input: " + a.Name + " has " + val.Value);
                a.ExternalValue = (val.Value + axesToBind[i].Offset) * axesToBind[i].ScaleFactor;
            }
        }
    }

    /// <summary>Callback when websocket is connected</summary>
    protected void OnConnected(object sender, EventArgs e) {
        if (PrintLogMessages)
            Debug.Log("Kuka ROS Joint Subscriber connected to RosBridge: " + URL);
    }

    /// <summary>Callback when websocket is disconnected</summary>
    protected void OnDisconnected(object sender, EventArgs e) {
        if (PrintLogMessages)
            Debug.Log("Kuka ROS Joint Subscriber disconnected from RosBridge: " + URL);
    }
    #endregion
}
