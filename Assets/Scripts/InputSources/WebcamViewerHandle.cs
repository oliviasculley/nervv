﻿// System
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
    protected void Start() {
        if (InteractUI == null) throw new ArgumentNullException("No InteractUI set for SteamVR!");

        // Initial state
        availableHand = null;
        currHand = null;
        grabbing = false;
        if (InteractUI == null) throw new ArgumentNullException();
        InteractUI.onStateDown += SteamVROnGrabDown;
        InteractUI.onStateUp += SteamVROnGrabUp;
        InteractUI.onState += SteamVROnGrab;
    }


    /// <summary>Register incoming hand object</summary>
    protected void OnTriggerEnter(Collider collider) {
        var h = collider.GetComponent<Hand>();
        if (h != null) availableHand = h.transform;
    }

    /// <summary>Unregister incoming hand object</summary>
    protected void OnTriggerExit(Collider collider) {
        var h = collider.GetComponent<Hand>();
        if (h != null) availableHand = null;
    }
    #endregion

    #region Grab Callbacks
    /// <summary>OnGrabDown register starting hand pos and grabbing bool</summary>
    public void SteamVROnGrabDown(SteamVR_Action_Boolean b, SteamVR_Input_Sources inputSource) {
        if (availableHand == null) return;
        if (currHand != null)
            Debug.LogError("Currhand is not null: " + currHand.name + "!");
        prevHandPos = (currHand = availableHand).position;
        grabbing = true;
    }

    /// <summary>If input triggered and is grabbing, add rotation delta to joint</summary>
    public void SteamVROnGrab(SteamVR_Action_Boolean b, SteamVR_Input_Sources inputSource) {
        if (!grabbing) return;
        Debug.Assert(currHand != null);

        // Invoke callbacks
        OnGrab?.Invoke(this, null);
    }

    /// <summary>Stop grabbing</summary>
    public void SteamVROnGrabUp(SteamVR_Action_Boolean b, SteamVR_Input_Sources inputSource) {
        if (!grabbing) return;

        grabbing = false;
        Debug.Assert(currHand != null);
        currHand = null;
    }
    #endregion
}
