﻿//System
using System;
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
            foreach (Transform t in transform) t.gameObject.SetActive(false);

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
    /// <exception cref="ArgumentNullException">
    /// Thrown if any References are null
    /// </exception>
    protected void Awake() {
        uiSwitcher = GetComponent<UIPanelSwitcher>();
        if (uiSwitcher == null) throw new ArgumentNullException();
        if (TeleportGameObjects == null) throw new ArgumentNullException();
        if (LaserPointers == null) throw new ArgumentNullException();
        foreach (GameObject g in TeleportGameObjects)
            if (g == null) throw new ArgumentNullException();
        foreach (LaserPointer p in LaserPointers)
            if (p == null) throw new ArgumentNullException();
        foreach (GameObject g in menuElements)
            if (g == null) throw new ArgumentNullException();
    }

    /// <summary>Set initial menu state</summary>
    protected void Start() {
        lerping = Visible = false;
    }

    /// <summary>Lerp menu towards camera if needed</summary>
    protected void Update() {
        // User wants menu to float towards controller
        Vector3 vel = Vector3.zero;
        lerping |= callMenu.state;

        if (lerping) {
            // Enable menu visible if disabled
            if (!Visible) Visible = true;

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

    #region Public methods
    /// <summary>UI Button wrapper function to set Visible</summary>
    /// <param name="isVisible">true to enable menu, false to hide menu</param>
    public void SetVisible(bool isVisible) {
        lerping = false;
        Visible = isVisible;
    }
    #endregion

    #region Methods
    /// <summary>Return target menu location to move toward</summary>
    /// <returns>Target in world space</returns>
    protected virtual Vector3 GetTargetPos() {
        return Camera.main.transform.forward +
            Camera.main.transform.TransformPoint(offset);
    }
    #endregion
}
