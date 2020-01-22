// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;
using UnityEngine.UI;
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
            StopWebcamTexture();

            if (value == -1) {
                _deviceID = -1;
                return;
            }

            if (cachedDevices == null)
                throw new InvalidOperationException("Could not get any webcams!");

            if (value < 0 || value > cachedDevices.Length)
                throw new ArgumentOutOfRangeException("Invalid webcam device ID");
            _deviceID = value;
            if (Dropdown != null)
                Dropdown.SetValueWithoutNotify(_deviceID);

            // Restart webcam feed
            currWebcamFeed = StartCoroutine(GetCurrentDeviceFeed());
        }
    }

    /// <summary>Convenience function to get current device. Can be null!</summary>
    public WebCamDevice? CurrentDevice {
        get {
            // If DeviceID is less than -1 or greater than cachedDevices' length
            if (DeviceID < -1 ||
                (cachedDevices != null && DeviceID >= cachedDevices.Length))
                throw new ArgumentOutOfRangeException();

            if (DeviceID == -1 || cachedDevices == null)
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
    public RawImage ImageRenderer = null;
    public WebcamViewerHandle HandleScript = null;
    public TMP_Dropdown Dropdown = null;
    public Material WebcamMaterial = null;
    #endregion

    #region Vars
    WebCamTexture w = null;
    WebCamDevice[] _cachedDevices = null;
    WebCamDevice[] cachedDevices {
        get {
            if (_cachedDevices == null) {
                Dropdown.options = Dropdown.options ?? new List<TMP_Dropdown.OptionData>();
                Dropdown.options.Clear();
                Dropdown.options.Capacity = WebCamTexture.devices.Length;

                if (_cachedDevices == null || _cachedDevices.Length != WebCamTexture.devices.Length)
                    _cachedDevices = new WebCamDevice[WebCamTexture.devices.Length];
                
                for (int i = 0; i < WebCamTexture.devices.Length; i++) {
                    _cachedDevices[i] = WebCamTexture.devices[i];
                    Dropdown.options.Add(new TMP_Dropdown.OptionData(_cachedDevices[i].name));
                }
            }
            return _cachedDevices;
        }
    }
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
        if (ImageRenderer == null) throw new ArgumentNullException("PlaneRenderer is null!");
        if (HandleScript == null) throw new ArgumentNullException("HandleScript is null!");
        if (Dropdown == null) throw new ArgumentNullException("Dropdown is null!");
        if (WebcamMaterial == null) throw new ArgumentNullException("WebcamRT is null!");
        base.OnEnable();

        // Initial InputSource fields
        ExclusiveType = false;
        _deviceID = -1;

        // Enumerates webcams and sets to first if available
        GetFirstWebcamDevice();

        // Set callback
        HandleScript.OnGrab += MoveWebcamViewer;
    }

    /// <summary>Disables existing callback and coroutines</summary>
    protected override void OnDisable() {
        // Remove callback
        if (HandleScript != null)
            HandleScript.OnGrab -= MoveWebcamViewer;

        // Disable running webcam feeds or coroutines
        StopWebcamTexture();

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
    /// <summary>Unity Coroutine for activating current device's webcam texture</summary>
    protected IEnumerator GetCurrentDeviceFeed() {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam)) {
            LogWarning(
                "Webcam authorization denied for: \"" +
                (CurrentDevice.Value.name ?? "(null)") + "\"!");
            gameObject.SetActive(false);
        }
        if (CurrentDevice == null) {
            LogWarning("Current device is null!");
            gameObject.SetActive(false);
        }

        WebCamDevice d = CurrentDevice.Value;
        Resolution r = (d.availableResolutions?.Length ?? 0) > 0 ?
            r = d.availableResolutions[0] :
            r = new Resolution() { width = 1280, height = 720, refreshRate = 30 };
        w = new WebCamTexture(
            deviceName: WebCamTexture.devices[DeviceID].name,
            requestedWidth: r.width,
            requestedHeight: r.height,
            requestedFPS: r.refreshRate);

        Log("FPS: " + w.requestedFPS + ", H: " + w.requestedHeight + ", w: " + w.requestedWidth);

        (ImageRenderer.material = WebcamMaterial).mainTexture = w;
        ImageRenderer.texture = w;
        w.Play();

        if (!w.isPlaying)
            LogError("Webcam texture is not playing!");
    }

    /// <summary>
    /// Enumerates webcams in cachedDevices and sets to first webcam feed if available
    /// </summary>
    protected void GetFirstWebcamDevice() {
        // Debug.Log listing of webcams
        if (printAvailableWebcams) {
            string s = "Available Webcam Devices:";
            foreach (WebCamDevice d in WebCamTexture.devices)
                s += "\n" + d.name;
            Debug.Log(s);
        }

        // Enumerate webcams
        if (cachedDevices == null)
            LogError("cachedDevices is null!");

        // Set to first webcam if available
        if (cachedDevices.Length > 0)
            DeviceID = 0;
    }

    /// <summary>Invoked via callback when</summary>
    /// <param name="sender">Unused</param>
    /// <param name="args">Unused</param>
    protected void MoveWebcamViewer(object sender, EventArgs args) {
        transform.position = HandleScript.CurrentHand.position;
        transform.localPosition += GrabbingOffset;
    }

    /// <summary>Stops running coroutine and webcam texture</summary>
    protected void StopWebcamTexture() {
        if (currWebcamFeed != null)
            StopCoroutine(currWebcamFeed);
        w?.Stop();
        w = null;
    }
    #endregion
}
