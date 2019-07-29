﻿using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;
using UnityEngine.Networking;

public class Webcam : MonoBehaviour {

    #region Settings
    [Header("Settings")]

    //public readonly string source = "http://192.168.1.26:8082/";
    public readonly string source = "http://192.168.1.26:8081/";
    public string localSource;
    public float pollInterval;

    #endregion

    #region References
    [Header("References")]

    public Renderer planeRenderer;

    #endregion

    #region Private vars

    private WebCamTexture w;
    private float timeToTrigger = 0.0f;

    #endregion

    #region Unity Methods

    private void Awake() {
        Debug.Assert(planeRenderer != null, "[Webcam] Could not get reference to renderer!");
        Debug.Assert(!string.IsNullOrEmpty(source), "Webcam URL is null or empty!");
        if (pollInterval == 0)
            Debug.LogWarning("Poll interval set to 0, will send GET request every frame!");
    }

    private void Start() {
        w = new WebCamTexture(localSource);
        StartCoroutine(GetLocalWebcamFeed());
    }

    private void Update() {
        // Look at main camera
        transform.LookAt(Camera.main.transform.position);

        
        // Check if time to trigger
        //if (Time.time > timeToTrigger)
        //{
        //    // Set new time to trigger
        //    timeToTrigger += pollInterval;

        //    // Call GET request
        //    StartCoroutine(GetRemoteWebcamFeed());
        //}
    }

    #endregion

    #region Webcam Methods

    /// <summary> Unity coroutine for updating a plane with a remote image </summary>
    private IEnumerator GetRemoteWebcamFeed() {
        WWWForm form = new WWWForm();
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(source)) {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError) {
                Debug.LogError("GET request returned error: " + www.error);
            } else {
                //Debug.Log("[INFO] GET request returned: " + 
                //  ((DownloadHandlerTexture)www.downloadHandler).texture);
                planeRenderer.material.mainTexture = DownloadHandlerTexture.GetContent(www);
            }
        }
    }

    /// <summary> Unity Coroutine for streaming local webcam to plane </summary>
    private IEnumerator GetLocalWebcamFeed() {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam)) {
            planeRenderer.material.mainTexture = w;
            w.Play();
        } else {
            Debug.LogWarning("[Webcam] Webcam authorization denied!");
        }
    }

    #endregion
}
