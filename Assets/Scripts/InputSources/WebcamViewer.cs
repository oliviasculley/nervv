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
using RosSharp.RosBridgeClient.MessageTypes.Sensor;
using RosSharp.RosBridgeClient.MessageTypes.Std;

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
                        ReceiveMessage);

                    if (string.IsNullOrEmpty(subscribedTopic))
                        LogError("Returned subscribed topic is invalid!");

                    Log($"Subscribed to \"{Topics[_topicID.Value]}\", with topicID: \"{subscribedTopic}\"");
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
    
    public WebcamViewerHandle HandleScript = null;
    public TMP_Dropdown Dropdown = null;
    public RenderTexture WebcamRT = null;
    public Material WebcamMat = null;
    #endregion

    #region Vars
    protected Coroutine rosConnect = null;
    protected RosSocket rosSocket = null;
    protected Coroutine displayMessage = null;
    protected CompressedImage receivedMessage = null;
    protected Texture2D TempTex2D = null;

    /// <summary>Used to unsubscribe from topic on close</summary>
    protected string subscribedTopic = null;
    protected Transform savedParent;
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
        if (WebcamMat == null) throw new ArgumentNullException("WebcamMat is null!");
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
        if (displayMessage != null)
            StopCoroutine(displayMessage);
        displayMessage = null;

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

        Log("Connecting to ros...");

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
        //Log("rosSocket connected");
        //while (!rosSocket.protocol.IsAlive()) {
        //    Log("RosSocket is still not active....");
        //    yield return new WaitForSeconds(1);
        //}
        //yield return new WaitUntil(() => rosSocket.protocol.IsAlive());
        //Log("rosSocket active!");
    }

    /// <summary>Called when RosSocket receieves messages</summary>
    /// <param name="message">Incoming image to display</param>
    protected void ReceiveMessage(CompressedImage message) {
        Log("Message callback");
        if (!InputEnabled) return;

        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();

        if (displayMessage != null)
            StopCoroutine(displayMessage);
        receivedMessage = message;
        displayMessage = StartCoroutine(DisplayImage());

        stopWatch.Stop();

        TimeSpan ts = stopWatch.Elapsed;
        string elapsedTime = System.String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        Log($"Received message of format {receivedMessage.format} in {elapsedTime}");
    }

    /// <summary>Doesn't return properly</summary>
    public IEnumerator DisplayImage() {
        Log("Starting display...");
        if (TempTex2D == null)                      // Load image into Texture2D
            TempTex2D = new Texture2D(640, 480);    // Sample size, may change after LoadImage
        TempTex2D.LoadImage(receivedMessage.data);
        TempTex2D.Apply();

        // Copy from Texture2D to material
        WebcamMat.mainTexture = TempTex2D;
        WebcamMat.SetTexture("_MainTex", TempTex2D);

        // Copy image from Texture2D to render texture
        RenderTexture currActive = RenderTexture.active;
        RenderTexture.active = WebcamRT;

        // Ensure that render texture is same dimensions
        if (WebcamRT.height != TempTex2D.height || WebcamRT.width != TempTex2D.width) {
            WebcamRT.height = TempTex2D.height;
            WebcamRT.width = TempTex2D.width;
        }
        Graphics.Blit(TempTex2D, WebcamRT);
        RenderTexture.active = currActive;

        Log("Displayed message");
        yield return null;
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
    protected void OnConnected(object sender, EventArgs e) {
        Log($"Connected to RosBridge: {URL}");
        Log("Subscribing to first topic if available");
        if (Topics.Length > 0)
            TopicID = 0;
    }

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
