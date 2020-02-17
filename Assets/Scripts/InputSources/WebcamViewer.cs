// System
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

// Unity
using UnityEngine;
using TMPro;
using Valve.VR;

// NERVV
using NERVV;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Protocols;
using RosSharp.RosBridgeClient.Messages.Sensor;
using RosSharp.RosBridgeClient.Messages.Standard;

public class WebcamViewer : InputSource {
    #region Static
    public enum ProtocolSelection { WebSocketSharp, WebSocketNET, None };   // None also used as length
    #endregion

    #region Properties
    [SerializeField, Header("Properties")]
    protected int? _topicID = null;
    public int? TopicID {
        get => _topicID;
        protected set {
            // Safety checks
            if (Topics == null) throw new ArgumentNullException("No topics!");
            if (value.HasValue) {
                if (value.Value >= Topics.Length)
                    throw new ArgumentOutOfRangeException("TopicID invalid!");
                if (string.IsNullOrEmpty(Topics[value.Value]))
                    throw new ArgumentException("Topic is null or empty!");
            }

            // Unsubscribe from any previous coroutines
            if (!string.IsNullOrEmpty(subscribedTopic))
                Unsubscribe();

            _topicID = value;
            if (Dropdown != null)
                Dropdown.SetValueWithoutNotify(_topicID ?? -1);
            Log($"Set TopicID to {_topicID?.ToString() ?? "null"}");

            if (_topicID == null) return;

            // Subscribe to topic
            if (rosSocket != null) {
                if (_topicID != null) {
                    subscribedTopic = rosSocket.Subscribe<CompressedImage>(
                        Topics[_topicID.Value],
                        ReceiveMessage
                    );

                    Log($"Subscribed to \"{Topics[_topicID.Value]}\"");
                } else {
                    LogError($"Current TopicID is null!");
                }
            } else {
                LogError("rosSocket is null!");
            }
        }
    }

    public override bool InputEnabled {
        get => _inputEnabled;
        set {
            _inputEnabled = value;
            foreach (Transform t in transform)
                t.gameObject.SetActive(_inputEnabled);
            
        }
    }
    #endregion

    #region Settings
    //[Header("Settings")]
    #endregion

    #region ROS Settings
    [Tooltip("Topics to subscribe from"), Header("ROS Settings")]
    public string[] Topics = new string[] { "/webcam/raw_image/compressed" };

    [Tooltip("URL of RosBridgeClient websocket to subscribe from")]
    public string URL = "";

    [Tooltip("Protocol to use to connect to RosBridgeClient")]
    public ProtocolSelection Protocol = ProtocolSelection.WebSocketNET;

    [Tooltip("Serialization mode of RosBridgeClient")]
    public RosSocket.SerializerEnum SerializationMode = RosSocket.SerializerEnum.JSON;
    #endregion

    #region References
    [Header("References")]
    public Texture2D WebcamTexture = null;
    public WebcamViewerHandle HandleScript = null;
    public TMP_Dropdown Dropdown = null;
    public RenderTexture WebcamRT = null;
    #endregion

    #region Vars
    Coroutine rosConnect = null;
    RosSocket rosSocket = null;

    /// <summary>Used to unsubscribe from topic on close</summary>
    string subscribedTopic = "";
    Transform savedParent;
    #endregion

    #region Unity Methods
    /// <summary>Safety checks and get local camera feed</summary>
    /// <exception cref="ArgumentNullException">
    /// If Plane renderer or PlaneRenderTexture is null
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If webcam DeviceID is out of range of WebCamTexture.devices
    /// </exception>
    protected override void OnEnable() {
        if (WebcamTexture == null) throw new ArgumentNullException("PlaneRenderer is null!");
        if (HandleScript == null) throw new ArgumentNullException("HandleScript is null!");
        if (Dropdown == null) throw new ArgumentNullException("Dropdown is null!");
        if (WebcamRT == null) throw new ArgumentNullException("Webcam RenderTexture is null!");
        base.OnEnable();

        // Initial InputSource fields
        ExclusiveType = false;
        _topicID = null;

        if (rosConnect != null)
            LogWarning("Socket not null! Overwriting...");
        rosConnect = StartCoroutine(ConnectToRos());

        // Set callback
        HandleScript.OnGrabDown += Parent;
        HandleScript.OnGrabUp += UnParent;
    }

    /// <summary>Disables existing callback and coroutines</summary>
    protected override void OnDisable() {
        // Remove callback
        if (HandleScript != null) {
            HandleScript.OnGrabDown -= Parent;
            HandleScript.OnGrabUp -= UnParent;
        }

        Unsubscribe();

        // Close socket
        if (rosConnect != null)
            StopCoroutine(rosConnect);
        rosConnect = null;

        if (rosSocket != null) {
            rosSocket.Close();
            rosSocket = null;
            TopicID = null;
        }

        base.OnDisable();
    }
    #endregion

    #region Public Methods
    /// <summary>UI wrapper to set the device ID</summary>
    /// <param name="deviceID">ID of Topic to set feed</param>
    public void SetWebcamFeedID(int deviceID) => TopicID = deviceID;
    #endregion

    #region Methods
    /// <summary>Connects to ROS given a IProtocol object with settings</summary>
    /// <param name="p">IProtocol object with appropriate settings</param>
    /// <returns>Unity Coroutine</returns>
    protected IEnumerator ConnectToRos() {
        // Get protocol object
        IProtocol currentProtocol = null;
        switch (Protocol) {
            case ProtocolSelection.WebSocketSharp:
                currentProtocol = new WebSocketSharpProtocol(URL);
                break;

            case ProtocolSelection.WebSocketNET:
                currentProtocol = new WebSocketNetProtocol(URL);
                break;

            default:
                throw new NotSupportedException("Could not get find matching protocol for RosSocket!");
        }
        Debug.Assert(currentProtocol != null);

        // OnConnected and OnClosed event handlers
        currentProtocol.OnConnected += OnConnected;
        currentProtocol.OnClosed += OnDisconnected;

        // Try for 50 seconds
        for (int i = 0; i < 50 && rosSocket == null; i++) {
            try {
                rosSocket = new RosSocket(currentProtocol, SerializationMode);
            } catch (SocketException e) {
                LogError(
                    "SocketException, trying again in one second...\n" +
                    "----------------------------------------------\n" +
                    $"{e.Message}\n{e.StackTrace}");
            }
            if (rosSocket != null) break;
            yield return new WaitForSeconds(1);
        }
        if (rosSocket == null)
            throw new OperationCanceledException("Could not connect rosSocket!");

        // Wait until socket is active
        yield return new WaitUntil(() => rosSocket.protocol.IsAlive());

        Log("rosSocket active, subscribing to first topic if available");
        if (Topics.Length > 0)
            TopicID = 0;
    }

    /// <summary>Called when RosSocket receieves messages</summary>
    /// <param name="message">Incoming image to display</param>
    protected void ReceiveMessage(CompressedImage message) {
        Log("Received message");
        if (!InputEnabled) return;

        // Load image into Texture2D
        if (WebcamTexture == null)
            WebcamTexture = new Texture2D(640, 480);    // Sample size, may change after LoadImage
        Debug.Assert(WebcamTexture.LoadImage(message.data));

        // Copy image from Texture2D to render texture
        RenderTexture currActive = RenderTexture.active;
        RenderTexture.active = WebcamRT;

        // Ensure that render texture is same dimensions
        if (WebcamRT.height != WebcamTexture.height || WebcamRT.width != WebcamTexture.width) {
            WebcamRT.height = WebcamTexture.height;
            WebcamRT.width = WebcamTexture.width;
        }
        Graphics.Blit(WebcamTexture, WebcamRT);
        RenderTexture.active = currActive;

        Log($"Displayed message of format {message.format}");
    }

    /// <summary>Stops current coroutine, unsubscribes from topic</summary>
    protected void Unsubscribe() {
        for (int i = 0; i < 50 && !string.IsNullOrEmpty(subscribedTopic); i++) {
            rosSocket.Unsubscribe(subscribedTopic);
            Log($"Unsubscribed from topic {subscribedTopic}");
            subscribedTopic = null;
            return;
        }

        LogError($"Could not unsubscribe from topic {subscribedTopic}!");
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

    protected void Parent(object sender, EventArgs args) {
        savedParent = transform.parent;
        transform.parent = HandleScript.CurrentHand.transform;
    }

    protected void UnParent(object sender, EventArgs args) {
        transform.parent = savedParent;
    }
    #endregion
}
