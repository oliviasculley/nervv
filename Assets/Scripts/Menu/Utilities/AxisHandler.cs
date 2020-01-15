// System
using System;

// Unity Engine
using UnityEngine;

using Valve.VR;
using Valve.VR.InteractionSystem;

using NERVV;

public class AxisHandler : MonoBehaviour {
    #region Properties
    [SerializeField, Header("Dynamically Set Properties")]
    protected Machine.Axis _axis;
    /// <summary>
    /// Machine axis. Enables/Disables the axis handler. InteractUI must
    /// be set before setting Axis!
    /// </summary>
    public Machine.Axis Axis {
        get => _axis;
        set {
            _axis = value;
            if (InteractUI == null) throw new ArgumentNullException();
            gameObject.SetActive(_axis != null);
        }
    }
    #endregion

    #region Settings
    [Header("Settings")]
    public float MaxDeltaPerFrameFactor = 100;
    [Range(0, 1)]
    public float MaxOpacity = 0.5f;
    [Range(0, 1)]
    public float MinOpacity = 0.1f;
    public float ColorBlendSpeed = 10;
    public bool PrintDebugMessages = false;
    #endregion

    #region References
    [Header("References")]
    public GameObject Torus;
    public SteamVR_Action_Boolean InteractUI = null;
    #endregion

    #region Vars
    /// <summary>Hands available to start new grabs with</summary>
    protected Transform availableHand = null;

    /// <summary>Reference to hand for currently grabbing</summary>
    protected Transform currHand = null;
    protected Vector3 prevHandPos = Vector3.zero;
    protected bool grabbing = false;
    protected MeshRenderer mr = null;
    protected MaterialPropertyBlock mrProperty;
    #endregion

    #region Unity Methods
    /// <summary>Error checking, init state, register callbacks</summary>
    protected void OnEnable() {
        // Check references
        if (Axis == null) {
            Axis = Axis;    // Trigger setter and disable
            return;
        }
        Torus = Torus ?? throw new ArgumentNullException();
        mr = Torus.GetComponent<MeshRenderer>();
        if (mr == null) throw new ArgumentNullException();
        mrProperty = new MaterialPropertyBlock();

        // Initial state
        availableHand = null;
        currHand = null;
        grabbing = false;
        if (InteractUI == null) throw new ArgumentNullException();
        InteractUI.onStateDown += OnGrabDown;
        InteractUI.onStateUp += OnGrabUp;
        InteractUI.onState += OnGrab;
        SetAlpha(MinOpacity, mr.material);
    }

    /// <summary>Lerp towards full color when hand is colliding</summary>
    protected void Update() {
        var value = Mathf.Lerp(
            a: mr.material.GetColor("_BaseColor").a,
            b: (availableHand != null || grabbing) ? MaxOpacity : MinOpacity,
            t: 0.1f); //Mathf.Clamp01(Time.deltaTime * ColorBlendSpeed));

        SetAlpha(value, mr.material);
        if (PrintDebugMessages) Debug.Log(value);
    }
    #endregion

    #region Grab Callbacks
    /// <summary>OnGrabDown register starting hand pos and grabbing bool</summary>
    public void OnGrabDown(SteamVR_Action_Boolean b, SteamVR_Input_Sources inputSource) {
        if (availableHand == null) return;
        if (currHand != null && PrintDebugMessages)
            Debug.Log("Currhand is not null: " + currHand.name + "!");
        prevHandPos = (currHand = availableHand).position;
        grabbing = true;
    }

    /// <summary>If input triggered and is grabbing, add rotation delta to joint</summary>
    public void OnGrab(SteamVR_Action_Boolean b, SteamVR_Input_Sources inputSource) {
        if (!grabbing) return;
        Debug.Assert(currHand != null);

        // Get dragging angle for that frame
        //var projectedPrevHandVector = new Vector3(prevHandPos.x, 0, prevHandPos.z);
        //var projectedCurrHandVector = new Vector3(
        //    currHand.transform.position.x,
        //    0,
        //    currHand.transform.position.z);

        //var delta = -Vector3.SignedAngle(
        //    transform.InverseTransformPoint(projectedPrevHandVector),
        //    transform.InverseTransformPoint(projectedCurrHandVector),
        //    Vector3.up);

        // Get dragging angle for that frame
        var delta = -Vector3.SignedAngle(
            transform.InverseTransformPoint(prevHandPos),
            transform.InverseTransformPoint(currHand.transform.position),
            Vector3.up);

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
        var h = collider.GetComponentInParent<Hand>();
        if (h != null) availableHand = h.transform;
    }

    /// <summary>Unregister incoming hand object</summary>
    protected void OnTriggerExit(Collider collider) {
        var h = collider.GetComponentInParent<Hand>();
        if (h != null) availableHand = null;
    }
    #endregion

    #region Methods
    /// <summary>Convenience function to set alpha for a material</summary>
    /// <param name="f">Alpha value between 0 and 1</param>
    /// <param name="m">Material to set alpha</param>
    protected void SetAlpha(float f, Material m) {
        if (m == null) throw new ArgumentNullException();
        mrProperty.SetColor(
            "_BaseColor",
            new Color(1,1,1, Mathf.Clamp01(f)));
        mr.SetPropertyBlock(mrProperty);
    }
    #endregion
}
