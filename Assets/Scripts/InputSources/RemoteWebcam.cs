// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;
using UnityEngine.Networking;

// NERVV
using NERVV;

public class RemoteWebcam : InputSource {
    #region Settings
    [Header("Settings")]
    public string source;
    #endregion

    #region References
    [Header("References")]
    public Renderer planeRenderer;
    #endregion

    #region Vars
    #endregion

    #region Unity Methods
    /// <summary>Safety checks</summary>
    /// <exception cref="ArgumentException">
    /// Thrown when source string is empty or null
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when plane renderer is null
    /// </exception>
    protected override void OnEnable() {
        if (planeRenderer == null)
            throw new ArgumentNullException("Plane renderer is null!");
        if (string.IsNullOrEmpty(source))
            throw new ArgumentException("Source is empty or null!");

        // Initial InputSource fields
        Name = "RemoteWebcam: " + source;
        ExclusiveType = false;
        base.OnEnable();
    }

    /// <summary>Orient webcam plane and get remote feeds</summary>
    private void Update() {
        // Look at main camera
        transform.LookAt(Camera.main.transform.position);

        // TODO: Implement ROS Remote camera feed
    }
    #endregion

    #region Webcam Methods
    /// <summary>Unity coroutine for updating a plane with a remote image</summary>
    //IEnumerator GetRemoteWebcamFeed() {
    //    WWWForm form = new WWWForm();
    //    using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(source)) {
    //        yield return www.SendWebRequest();

    //        if (www.isNetworkError || www.isHttpError) {
    //            Debug.LogError("GET request returned error: " + www.error);
    //        } else {
    //            //Debug.Log("[INFO] GET request returned: " + 
    //            //  ((DownloadHandlerTexture)www.downloadHandler).texture);
    //            planeRenderer.material.mainTexture = DownloadHandlerTexture.GetContent(www);
    //        }
    //    }
    //}
    #endregion
}
