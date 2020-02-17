// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class WebcamViewerHandle : MonoBehaviour {
    #region Properties
    //[Header("Properties")]
    public bool IsGrabbing => grabbing;
    public Transform CurrentHand => currHand;
    public event EventHandler OnGrab;
    public event EventHandler OnGrabDown;
    public event EventHandler OnGrabUp;
    #endregion

    #region Settings
    [Header("Settings")]
    public bool PrintDebugMessages = false;
    #endregion

    #region References
    [Header("References")]
    public SteamVR_Action_Boolean InteractUI;
    #endregion

    #region Vars
    /// <summary>Hands available to start new grabs with</summary>
    protected Transform availableHand = null;

    /// <summary>Reference to hand for currently grabbing</summary>
    protected Transform currHand = null;
    protected Vector3 prevHandPos = Vector3.zero;
    protected bool grabbing = false;
    #endregion

    #region Unity Methods
    /// <summary>Initializes state and enables callbacks</summary>
    protected void OnEnable() {
        if (InteractUI == null) throw new ArgumentNullException("No InteractUI set for SteamVR!");

        // Initial state
        availableHand = null;
        currHand = null;
        grabbing = false;

        // Enable callbacks
        InteractUI.onStateDown += SteamVROnGrabDown;
        InteractUI.onStateUp += SteamVROnGrabUp;
        InteractUI.onState += SteamVROnGrab;
    }

    /// <summary>Removes callbacks from InteractUI</summary>
    protected void OnDisable() {
        if (InteractUI != null) {
            InteractUI.onStateDown -= SteamVROnGrabDown;
            InteractUI.onStateUp -= SteamVROnGrabUp;
            InteractUI.onState -= SteamVROnGrab;
        }
    }

    /// <summary>Register incoming hand object</summary>
    protected void OnTriggerEnter(Collider collider) {
        var h = collider.GetComponentInParent<Hand>();
        if (h != null) {
            availableHand = h.transform;
        }
    }

    /// <summary>Unregister incoming hand object</summary>
    protected void OnTriggerExit(Collider collider) {
        var h = collider.GetComponentInParent<Hand>();
        if (h != null) {
            availableHand = null;
        }
    }
    #endregion

    #region Grab Callbacks
    /// <summary>OnGrabDown register starting hand pos and grabbing bool</summary>
    public void SteamVROnGrabDown(SteamVR_Action_Boolean b, SteamVR_Input_Sources inputSource) {
        Log("Grab down");
        if (availableHand == null) return;
        if (currHand != null)
            LogError($"Currhand is not null: {currHand.name}!");
        prevHandPos = (currHand = availableHand).position;
        grabbing = true;
        OnGrabDown?.Invoke(this, null);
    }

    /// <summary>If input triggered and is grabbing, add rotation delta to joint</summary>
    public void SteamVROnGrab(SteamVR_Action_Boolean b, SteamVR_Input_Sources inputSource) {
        Log("OnGrabbing!");
        if (!grabbing) return;
        Debug.Assert(currHand != null);

        // Invoke callbacks
        OnGrab?.Invoke(this, null);
    }

    /// <summary>Stop grabbing</summary>
    public void SteamVROnGrabUp(SteamVR_Action_Boolean b, SteamVR_Input_Sources inputSource) {
        if (!grabbing) return;
        Log("Grab up!");

        grabbing = false;
        Debug.Assert(currHand != null);
        currHand = null;
        OnGrabUp?.Invoke(this, null);
    }
    #endregion

    #region Methods
    protected void Log(string s) { if (PrintDebugMessages) Debug.Log($"<b>[{GetType()}]</b> " + s); }
    protected void LogWarning(string s) { if (PrintDebugMessages) Debug.LogWarning($"<b>[{GetType()}]</b> " + s); }
    protected void LogError(string s) { if (PrintDebugMessages) Debug.LogError($"<b>[{GetType()}]</b> " + s); }
    #endregion
}
