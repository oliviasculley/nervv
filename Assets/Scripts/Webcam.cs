using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Webcam : MonoBehaviour
{
    [Header("References")]
    public Renderer planeRenderer;

    // Private vars
    private WebCamTexture webCamTexture;

    private void Awake() {
        Debug.Assert(planeRenderer != null, "[Webcam] Could not get reference to renderer!");
    }

    private void Start()
    {
        webCamTexture = new WebCamTexture();
        StartCoroutine(GetWebcamFeed());
    }

    private void Update() {
        // Look at main camera
        transform.LookAt(Camera.main.transform.position);
        transform.Rotate(Vector3.right, 90);    // Adjust for default plane position
    }

    private IEnumerator GetWebcamFeed() {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam)) {
            planeRenderer.material.mainTexture = webCamTexture;
            webCamTexture.Play();
        } else {
            Debug.LogWarning("[Webcam] Webcam authorization denied!");
        }
    }
}
