// System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuButtonAnim : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    #region Properties
    [Header("Properties")]

    public bool hovered;

    #endregion

    #region Animation Settings
    [Header("Animation Settings")]

    /// <summary>Resting height for sprite</summary>
    [Tooltip("Resting height for sprite")]
    public float unactivatedHeight = 0.05f;

    /// <summary>Height of activated sprite</summary>
    [Tooltip("Height of activated sprite")]
    public float activatedHeight = 0.025f;

    /// <summary>Speed to activate icons</summary>
    [Tooltip("Speed to activate icons")]
    public float activationSpeed = 1f;

    /// <summary>Ratio of background plane to image</summary>
    [Tooltip("Ratio of background plane to image")]
    public readonly float planeRatio = 1f;

    #endregion

    #region References
    [Header("References")]

    [Tooltip("Menu that menuButton is attached to")]
    public Menu menu;
    public GameObject buttonBackground, buttonIcon;

    #endregion

    #region Unity Methods

    private void Awake() {
        Debug.Assert(menu != null,
            "[MenuButton] Could not get reference to menu!");
        if (buttonBackground == null && buttonIcon == null)
            Debug.LogWarning("[MenuButton] Button background and icon not set, will not animate!");
    }

    private void Start() {
        // Init vars
        hovered = false;

        // Set plane size relative to icon
        buttonBackground.transform.localScale = new Vector3(
            planeRatio * buttonIcon.transform.localScale.x,
            1f,
            planeRatio * buttonIcon.transform.localScale.z
        );

        // Set buttonIcon to unhovered position
        buttonIcon.transform.localPosition = new Vector3(
            buttonIcon.transform.localPosition.x,
            buttonIcon.transform.localPosition.y,
            -unactivatedHeight
        );

        // Set buttonBackground to unhovered position
        buttonBackground.transform.localPosition = new Vector3(
            buttonBackground.transform.localPosition.x,
            buttonBackground.transform.localPosition.y,
            -unactivatedHeight
        );
    }

    private void Update() {
        // Smoothly raise or lower buttonIcon when hovered
        buttonIcon.transform.localPosition = new Vector3(
            buttonIcon.transform.localPosition.x,
            buttonIcon.transform.localPosition.y,
            Mathf.Lerp(
                buttonIcon.transform.localPosition.z,
                -(hovered ? activatedHeight : unactivatedHeight),
                Time.deltaTime * activationSpeed
            )
        );

        // Smoothly raise or lower buttonBackground to half height of icon when hovered
        buttonBackground.transform.localPosition = new Vector3(
            buttonBackground.transform.localPosition.x,
            buttonBackground.transform.localPosition.y,
            Mathf.Lerp(
                buttonBackground.transform.localPosition.z,
                -(hovered ? activatedHeight : unactivatedHeight),
                Time.deltaTime * activationSpeed
            )
        );
    }

    #endregion

    #region Public methods

    /// <summary>Hover on OnPointerEnter</summary>
    /// <param name="data">Event system data</param>
    public void OnPointerEnter(PointerEventData data) {
        hovered = true;
    }

    /// <summary>Unhover on OnPointerExit</summary>
    /// <param name="data">Event system data</param>
    public void OnPointerExit(PointerEventData data) {
        hovered = false;
    }

    #endregion
}
