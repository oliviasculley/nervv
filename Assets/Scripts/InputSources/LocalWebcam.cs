// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;

// NERVV
using NERVV;

public class LocalWebcam : InputSource {
    #region Properties
    public override bool InputEnabled {
        get => _inputEnabled;
        set {
            _inputEnabled = value;
            foreach (Transform t in transform)
                t.gameObject.SetActive(_inputEnabled);
        }
    }

    [SerializeField, Header("Properties")]
    protected int _deviceID;
    public int DeviceID {
        get => _deviceID;
        protected set { SetWebcamFeedID(value); }
    }
    #endregion

    #region Settings
    [Header("Settings")]
    public bool printAvailableWebcams = false;
    #endregion

    #region References
    [Header("References")]
    public Renderer PlaneRenderer;
    #endregion

    #region Vars
    WebCamTexture w = null;
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
        if (PlaneRenderer == null)
            throw new ArgumentNullException("Plane Renderer is null!");
        if (_deviceID < 0 || _deviceID >= WebCamTexture.devices.Length)
            throw new ArgumentOutOfRangeException("Webcam DeviceID out of range!");
        base.OnEnable();


        // Initial InputSource fields
        ExclusiveType = false;

        if (printAvailableWebcams)
            PrintAvailableWebcams();
        StartCoroutine(GetLocalWebcamFeed());
    }

    protected override void OnDisable() {
        StopCoroutine(GetLocalWebcamFeed());
        w?.Stop();
        w = null;
        base.OnDisable();
    }

    /// <summary>Orient webcam plane towards camera</summary>
    protected void Update() {
        transform.LookAt(Camera.main.transform.position);
    }
    #endregion

    #region Methods
    /// <summary>Unity Coroutine for streaming local webcam to plane</summary>
    protected IEnumerator GetLocalWebcamFeed() {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam)) {
            w = null;

            WebCamDevice d = WebCamTexture.devices[DeviceID];

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
                    WebCamTexture.devices[_deviceID].name + "\"!");
            gameObject.SetActive(false);
        }
    }

    /// <summary>Prints available webcams to log</summary>
    public void PrintAvailableWebcams() {
        string s = "Available Webcam Devices:";
        foreach (WebCamDevice d in WebCamTexture.devices)
            s += "\n" + d.name;
        Debug.Log(s);
    }

    /// <summary>Sets Webcam feed amount</summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Throws when <paramref name="deviceID"/> is not a valid webcam device
    /// </exception>
    public void SetWebcamFeedID(int deviceID) {
        if (deviceID < 0 || deviceID > WebCamTexture.devices.Length)
            throw new ArgumentOutOfRangeException("Invalid webcam device ID");
        _deviceID = deviceID;
        OnDisable();
        StartCoroutine(GetLocalWebcamFeed());
    }
    #endregion
}
