using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

[RequireComponent(typeof(UIPanelSwitcher))]
public class Menu : MonoBehaviour
{
    // Properties
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

    [Header("Settings")]
    public SteamVR_Action_Boolean callMenu;
    public Vector3 offset;      // Offset used when smoothing towards camera
    public float epsilon = 5f,  // Stops smoothing towards target position
        smoothTime = 0.05f,     // Speed to move towards target position
        menuPitch = 45;         // Angle to pitch menu up

    [Header("References")]
    public GameObject[] menuElements;   // Menu elements that mirror menu visibility

    // Private vars
    private UIPanelSwitcher uiSwitcher;
    private bool lerping;

    private void Awake() {
        uiSwitcher = GetComponent<UIPanelSwitcher>();
        Debug.Assert(uiSwitcher != null,
            "[Menu] Could not get reference to UIPanelSwitcher!");
    }

    private void Start() {
        // Initial menu state
        lerping = visible = false;
    }

    /// <summary>
    /// Set menu visible
    /// </summary>
    /// <param name="isVisible">true to enable menu, false to hide menu</param>
    public void SetVisible(bool isVisible)
    {
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

    /* Private methods */

    /// <summary>
    /// Return target menu location to move toward
    /// </summary>
    /// <returns>Target in world space</returns>
    private Vector3 GetTargetPos() {
        return Camera.main.transform.forward +
            Camera.main.transform.TransformPoint(offset);
    }
}
