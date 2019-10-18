// System
using System;

// Unity Engine
using UnityEngine;

using Valve.VR;
using Valve.VR.InteractionSystem;

using NERVV;

public class AxisHandler : MonoBehaviour {
    #region Properties
    [Header("Dynamically Set Properties")]
    public Machine.Axis Axis;
    public SteamVR_Action_Boolean InteractUI;
    #endregion

    #region Settings
    [Header("Settings")]
    public float MaxDeltaPerFrameFactor = 100;
    [Range(0, 1)]
    public float MaxOpacity = 0.5f;
    [Range(0, 1)]
    public float MinOpacity = 0.1f;
    public float ColorBlendSpeed = 100f;
    #endregion

    #region References
    [Header("References")]
    public GameObject Torus;
    #endregion

    #region Vars
    /// <summary>Hands available to start new grabs with</summary>
    protected Transform availableHands;

    /// <summary>Reference to hand for currently grabbing</summary>
    protected Transform currHand;
    protected Vector3 prevHandPos;
    protected bool grabbing;
    protected MeshRenderer mr;
    #endregion

    #region Unity Methods
    /// <summary>Error checking, init state, register callbacks</summary>
    protected void OnEnable() {
        if (Axis == null) throw new ArgumentNullException();
        mr = (Torus ?? throw new ArgumentNullException()).GetComponent<MeshRenderer>();
        if (mr == null) throw new ArgumentNullException();

        availableHands = null;
        currHand = null;
        grabbing = false;
        (InteractUI ?? throw new ArgumentNullException()).onStateDown += OnGrabDown;
        InteractUI.onStateUp += OnGrabUp;
        InteractUI.onState += OnGrab;
        mr.material.color = new Color(
            mr.material.color.r,
            mr.material.color.g,
            mr.material.color.b,
            MinOpacity);
    }

    /// <summary>Lerp towards full color when hand is colliding</summary>
    protected void Update() {
        mr.material.color = new Color(
            mr.material.color.r,
            mr.material.color.g,
            mr.material.color.b,
            Mathf.Lerp(
                mr.material.color.a,
                (availableHands != null || grabbing) ? MaxOpacity : MinOpacity,
                Mathf.Clamp01(Time.deltaTime * ColorBlendSpeed)
            )
        );
    }
    #endregion

    #region Grab Callbacks
    /// <summary>OnGrabDown register starting hand pos and grabbing bool</summary>
    public void OnGrabDown(SteamVR_Action_Boolean b, SteamVR_Input_Sources inputSource) {
        if (availableHands == null) return;
        Debug.Assert(currHand == null);
        prevHandPos = (currHand = availableHands).position;
        grabbing = true;
    }

    /// <summary>If input triggered and is grabbing, add rotation delta to joint</summary>
    public void OnGrab(SteamVR_Action_Boolean b, SteamVR_Input_Sources inputSource) {
        if (!grabbing) return;

        Debug.Assert(currHand != null);

        // Get dragging angle for that frame
        var delta = -Vector3.SignedAngle(
            transform.InverseTransformPoint(prevHandPos),
            transform.InverseTransformPoint(currHand.transform.position), Vector3.up);

        var maxDelta = Time.deltaTime * MaxDeltaPerFrameFactor;
        delta = Mathf.Clamp(delta, -maxDelta, maxDelta);

        // Add to axis value
        Axis.Value += delta;

        prevHandPos = currHand.position;
    }

    /// <summary>Stop grabbing</summary>
    public void OnGrabUp(SteamVR_Action_Boolean b, SteamVR_Input_Sources inputSource) {
        if (!grabbing) return;

        grabbing = false;
        Debug.Assert(currHand != null);
        currHand = null;
    }
    #endregion

    #region Trigger Collider Methods
    /// <summary>Register incoming hand object</summary>
    protected void OnTriggerEnter(Collider collider) {
        var h = collider.GetComponent<Hand>();
        if (h != null) availableHands = h.transform;
    }

    /// <summary>Unregister incoming hand object</summary>
    protected void OnTriggerExit(Collider collider) {
        var h = collider.GetComponent<Hand>();
        if (h != null) availableHands = null;
    }
    #endregion
}
