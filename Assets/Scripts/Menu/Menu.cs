//System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;
using NERVV;

using Valve.VR;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(UIPanelSwitcher))]
public class Menu : MonoBehaviour {
    #region Properties
    public bool Visible {
        get {
            // If any children are active, menu is active
            foreach (Transform t in transform)
                if (t.gameObject.activeSelf)
                    return true;
            return false;
        }
        set {
            // Set all children of menu false
            foreach (Transform t in transform)
                t.gameObject.SetActive(false);

            // Set other objects to value
            foreach (GameObject g in menuElements) g.SetActive(value);
            uiSwitcher.enabled = value;
            foreach (GameObject g in TeleportGameObjects) g.SetActive(!value);
            foreach (LaserPointer p in LaserPointers) p.enabled = value;
        }
    }
    #endregion

    #region Settings
    [Header("Settings")]
    public SteamVR_Action_Boolean callMenu;

    /// <summary>Offset used when smoothing towards camera</summary>
    [Tooltip("Offset used when smoothing towards camera")]
    public Vector3 offset;

    /// <summary>Stops smoothing towards target position</summary>
    [Tooltip("Stops smoothing towards target position")]
    public float epsilon = 5f;

    /// <summary>Speed to move towards target position</summary>
    [Tooltip("Speed to move towards target position")]
    public float smoothTime = 0.05f;

    /// <summary>Angle to pitch menu up</summary>
    [Tooltip("Angle to pitch menu up")]
    public float menuPitch = 45;
    #endregion

    #region References
    /// <summary>Menu elements that mirror menu visibility</summary>
    [Tooltip("Menu elements that mirror menu visibility"), Header("References")]
    public GameObject[] menuElements;
    public GameObject[] TeleportGameObjects;
    public LaserPointer[] LaserPointers;
    #endregion

    #region Vars
    UIPanelSwitcher uiSwitcher;
    bool lerping;
    #endregion

    #region Unity Methods
    /// <summary>Get and check references</summary>
    void Awake() {
        uiSwitcher = GetComponent<UIPanelSwitcher>();
        Debug.Assert(uiSwitcher != null);
        Debug.Assert(TeleportGameObjects != null);
        Debug.Assert(LaserPointers != null);
        foreach (GameObject g in TeleportGameObjects)
            Debug.Assert(g != null);
        foreach (LaserPointer p in LaserPointers)
            Debug.Assert(p != null);
        foreach (GameObject g in menuElements)
            Debug.Assert(g != null);
    }

    /// <summary>Set initial menu state</summary>
    void Start() {
        lerping = Visible = false;
    }
    #endregion

    #region Public methods
    /// <summary>Set menu visible</summary>
    /// <param name="isVisible">true to enable menu, false to hide menu</param>
    public void SetVisible(bool isVisible) {
        lerping = false;
        Visible = isVisible;
    }

    /// <summary>Lerp menu towards camera if needed</summary>
    void Update() {
        // User wants menu to float towards controller
        Vector3 vel = Vector3.zero;
        lerping |= callMenu.GetState(SteamVR_Input_Sources.Any);

        if (lerping) {
            // Enable menu visible if disabled
            if (!Visible)
                Visible = true;

            // Move towards target position
            transform.position = Vector3.SmoothDamp(
                transform.position,
                GetTargetPos(),
                ref vel,
                smoothTime
            );

            // Look at camera
            transform.LookAt(Camera.main.transform.position);
            transform.Rotate(Vector3.up, 180f);

            // Disable lerping if close enough
            if ((transform.position - (GetTargetPos())).sqrMagnitude < epsilon)
                lerping = false;
        }
    }
    #endregion

    #region Methods
    /// <summary>Return target menu location to move toward</summary>
    /// <returns>Target in world space</returns>
    Vector3 GetTargetPos() {
        return Camera.main.transform.forward +
            Camera.main.transform.TransformPoint(offset);
    }
    #endregion
}
