// System
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;

// NERVV
using NERVV;

public class LocalWebcam : InputSource {
    #region Properties
    public override bool InputEnabled {
        get { return _inputEnabled; }
        set {
            _inputEnabled = value;
            foreach (Transform t in transform)
                t.gameObject.SetActive(_inputEnabled);
        }
    }
    #endregion

    #region Settings
    [Header("Settings")]
    public string localSource;
    public bool printAvailableWebcams = false;
    #endregion

    #region References
    [Header("References")]
    public Renderer planeRenderer;
    #endregion

    #region Vars
    #endregion

    #region Unity Methods
    /// <summary>Safety checks and get local camera feed</summary>
    protected override void Start() {
        Debug.Assert(planeRenderer != null);

        // Initial InputSource fields
        ExclusiveType = false;
        base.Start();

        if (printAvailableWebcams)
            PrintAvailableWebcams();
        StartCoroutine(GetLocalWebcamFeed());
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
            WebCamTexture w = new WebCamTexture(localSource);
            Debug.Assert(w != null);
            w.Play();
            Debug.Assert(w.isPlaying);
            planeRenderer.material.mainTexture = w;
        } else {
            Debug.LogWarning("Webcam authorization denied for: \"" + localSource + "\"!");
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
    #endregion
}
