//System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;
using Valve.VR;

[RequireComponent(typeof(UIPanelSwitcher))]
public class Menu : MonoBehaviour {

    #region Properties

    public bool visible {
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

            // Set UI panel switcher enabled or disabled
            foreach (GameObject g in menuElements)
                g.SetActive(value);
            uiSwitcher.enabled = value;
        }
    }

    #endregion

    #region Settings
    [Header("Settings")]

    /// <summary> </summary>
    public SteamVR_Action_Boolean callMenu;

    /// <summary> Offset used when smoothing towards camera </summary>
    [Tooltip("Offset used when smoothing towards camera")]
    public Vector3 offset;

    /// <summary> Stops smoothing towards target position </summary>
    [Tooltip("Stops smoothing towards target position")]
    public float epsilon = 5f;

    /// <summary> Speed to move towards target position </summary>
    [Tooltip("Speed to move towards target position")]
    public float smoothTime = 0.05f;

    /// <summary> Angle to pitch menu up </summary>
    [Tooltip("Angle to pitch menu up")]
    public float menuPitch = 45;

    #endregion


    #region References
    [Header("References")]

    /// <summary> Menu elements that mirror menu visibility </summary>
    [Tooltip("Menu elements that mirror menu visibility")]
    public GameObject[] menuElements;

    #endregion


    #region Private vars

    private UIPanelSwitcher uiSwitcher;
    private bool lerping;

    #endregion

    #region Unity Methods

    private void Awake() {
        uiSwitcher = GetComponent<UIPanelSwitcher>();
        Debug.Assert(uiSwitcher != null,
            "[Menu] Could not get reference to UIPanelSwitcher!");
    }

    private void Start() {
        // Initial menu state
        lerping = visible = false;
    }

    #endregion

    #region Public methods

    /// <summary> Set menu visible </summary>
    /// <param name="isVisible">true to enable menu, false to hide menu</param>
    public void SetVisible(bool isVisible) {
        lerping = false;
        visible = isVisible;
    }

    private void Update() {
        // User wants menu to float towards controller
        Vector3 vel = Vector3.zero;
        lerping |= callMenu.GetState(SteamVR_Input_Sources.Any);

        if (lerping) {
            // Enable menu visible if disabled
            if (!visible)
                visible = true;

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

    #region Private methods

    /// <summary> Return target menu location to move toward </summary>
    /// <returns>Target in world space</returns>
    private Vector3 GetTargetPos() {
        return Camera.main.transform.forward +
            Camera.main.transform.TransformPoint(offset);
    }

    #endregion
}
