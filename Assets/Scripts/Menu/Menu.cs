using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

[RequireComponent(typeof(UIPanelSwitcher))]
public class Menu : MonoBehaviour
{

    [Header("Settings")]
    public SteamVR_Action_Boolean callMenu;
    public Vector3 offset;      // Offset used when smoothing towards camera
    public float epsilon,       // Stops smoothing towards target position
        smoothSpeed = 0.03f;    // Speed to move towards target position

    // Private vars
    private UIPanelSwitcher uiSwitcher;
    private bool lerping;

    private void Awake() {
        uiSwitcher = GetComponent<UIPanelSwitcher>();
        Debug.Assert(uiSwitcher != null,
            "[Menu] Could not get reference to UIPanelSwitcher!");
    }

    private void Start() {
        lerping = false;
    }

    private void Update() {
        // User wants menu to float towards controller
        Vector3 vel = Vector3.zero;
        lerping |= callMenu.GetState(SteamVR_Input_Sources.Any);
        
        if (lerping) {
            // Move towards target position
            transform.position = Vector3.SmoothDamp(
                transform.position,
                GetTargetPos(),
                ref vel,
                smoothSpeed
            );

            // Look at camera only around Y axis
            transform.LookAt(Camera.main.transform.position);

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
        return Camera.main.transform.forward + offset;
    }
}
