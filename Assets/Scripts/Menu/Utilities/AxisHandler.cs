using System.Collections;
using System.Collections.Generic;
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
    public float MaxDeltaPerFrameFactor = 100;
    #endregion

    #region Vars
    /// <summary>Hands available to start new grabs with</summary>
    protected Transform availableHands;

    /// <summary>Reference to hand for currently grabbing</summary>
    protected Transform currHand;
    protected Vector3 prevHandPos;
    protected bool grabbing;
    #endregion

    #region Unity Methods
    /// <summary>Error checking, init state, register callbacks</summary>
    protected void OnEnable() {
        Debug.Assert(Axis != null);
        Debug.Assert(InteractUI != null);

        availableHands = null;
        currHand = null;
        grabbing = false;
        InteractUI.onStateDown += OnGrabDown;
        InteractUI.onStateUp += OnGrabUp;
        InteractUI.onState += OnGrab;
    }

    /// <summary>OnGrabDown register starting hand pos and grabbing bool</summary>
    public void OnGrabDown (SteamVR_Action_Boolean b, SteamVR_Input_Sources inputSource) {
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
