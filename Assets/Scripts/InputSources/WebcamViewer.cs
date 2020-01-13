// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;
using TMPro;
using Valve.VR;

// NERVV
using NERVV;

public class WebcamViewer : InputSource {
    #region Properties
    [SerializeField, Header("Properties")]
    protected int _deviceID = -1;
    public int DeviceID {
        get => _deviceID;
        protected set {
            if (value < 0 || value > cachedDevices.Length)
                throw new ArgumentOutOfRangeException("Invalid webcam device ID");
            _deviceID = value;
            if (Dropdown != null)
                Dropdown.SetValueWithoutNotify(_deviceID);
            OnDisable();
            currWebcamFeed = StartCoroutine(GetLocalWebcamFeed());
        }
    }

    /// <summary>Convenience function to get current device. Can be null!</summary>
    public WebCamDevice? CurrentDevice {
        get {
            if (DeviceID < 0 || cachedDevices == null || DeviceID > cachedDevices.Length)
                return null;
            return cachedDevices[DeviceID];
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
    [Header("Settings")]
    public bool printAvailableWebcams = false;
    public Vector3 GrabbingOffset;
    #endregion

    #region References
    [Header("References")]
    public Renderer PlaneRenderer = null;
    public WebcamViewerHandle HandleScript = null;
    public TMP_Dropdown Dropdown = null;
    #endregion

    #region Vars
    WebCamTexture w = null;
    WebCamDevice[] cachedDevices = null;
    Coroutine currWebcamFeed = null;
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
        if (PlaneRenderer == null) throw new ArgumentNullException("PlaneRenderer is null!");
        if (HandleScript == null) throw new ArgumentNullException("HandleScript is null!");
        if (Dropdown == null) throw new ArgumentNullException("Dropdown is null!");
        if (_deviceID < 0 || _deviceID >= WebCamTexture.devices.Length)
            throw new ArgumentOutOfRangeException("Webcam DeviceID out of range!");
        base.OnEnable();

        // Initial InputSource fields
        ExclusiveType = false;
        _deviceID = -1;
        HandleScript.OnGrab += MoveWebcamViewer;

        // Enumerates webcams and sets to first if available
        EnumerateWebcams();
    }

    /// <summary>Disables existing coroutine and</summary>
    protected override void OnDisable() {
        HandleScript.OnGrab -= MoveWebcamViewer;
        if (currWebcamFeed != null)
            StopCoroutine(currWebcamFeed);
        w?.Stop();
        w = null;
        base.OnDisable();
    }
    #endregion

    #region Public Methods
    /// <summary>UI wrapper to set the device ID</summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Throws when <paramref name="deviceID"/> is not a valid webcam device
    /// </exception>
    public void SetWebcamFeedID(int deviceID) => DeviceID = deviceID;
    #endregion

    #region Methods
    /// <summary>Unity Coroutine for streaming local webcam to plane</summary>
    protected IEnumerator GetLocalWebcamFeed() {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam) && CurrentDevice != null) {
            w = null;

            WebCamDevice d = CurrentDevice.Value;
            Resolution r = (d.availableResolutions?.Length ?? 0) > 0 ?
                r = d.availableResolutions[d.availableResolutions.Length - 1] :
                r = new Resolution() { width = 100, height = 100, refreshRate = 15 };
            PlaneRenderer.material.mainTexture = w = new WebCamTexture(
                deviceName: WebCamTexture.devices[DeviceID].name,
                requestedWidth: r.width,
                requestedHeight: r.height,
                requestedFPS: r.refreshRate);
            w?.Play();
            Debug.Assert(w != null && w.isPlaying);
            
        } else {
            if (PrintDebugMessages)
                Debug.LogWarning("Webcam authorization denied for: \"" +
                    (CurrentDevice.Value.name ?? "(null)") + "\"!");
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Enumerates webcams in cachedDevices and sets to first webcam feed if available
    /// </summary>
    protected void EnumerateWebcams() {
        // Debug.Log listing of webcams
        if (printAvailableWebcams) {
            string s = "Available Webcam Devices:";
            foreach (WebCamDevice d in WebCamTexture.devices)
                s += "\n" + d.name;
            Debug.Log(s);
        }

        if (Dropdown.options == null)
            Dropdown.options = new List<TMP_Dropdown.OptionData>();
        if (cachedDevices == null || cachedDevices.Length != WebCamTexture.devices.Length)
            cachedDevices = new WebCamDevice[WebCamTexture.devices.Length];
        Dropdown.options.Clear();
        for (int i = 0; i < WebCamTexture.devices.Length; i++) {
            cachedDevices[i] = WebCamTexture.devices[i];
            Dropdown.options.Add(new TMP_Dropdown.OptionData(cachedDevices[i].name));
        }

        // Set to first webcam if available
        if (WebCamTexture.devices.Length > 0)
            DeviceID = 0;
    }

    /// <summary>Invoked via callback when</summary>
    /// <param name="sender">Unused</param>
    /// <param name="args">Unused</param>
    protected void MoveWebcamViewer(object sender, EventArgs args) {
        transform.position = HandleScript.CurrentHand.position;
        transform.localPosition += GrabbingOffset;
    }
    #endregion
}
