using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class MenuButtonAnim : MonoBehaviour
{
    [Header("Properties")]
    public bool hovered;

    [Header("Settings")]
    public Sprite icon;

    [Header("Animation Settings")]
    public float unactivatedHeight = 0.05f;     // Resting height for sprite
    public float activatedHeight = 0.025f;      // Height of activated sprite
    public float activationSpeed = 1f;          // Speed to activate icons
    public readonly float planeRatio = 1f;  // Ratio of background plane to image

    [Header("References")]
    public Menu menu;                           // Menu that menuButton is attached to
    public Image buttonIcon;                    // Reference to button image
    public GameObject buttonBackground;         // Reference to button background

    // Private vars
    RectTransform buttonIconRectTransform;

    private void Awake() {
        Debug.Assert(menu != null,
            "[MenuButton] Could not get reference to menu!");
        Debug.Assert(buttonIcon != null,
            "[MenuButton] Could not get reference to icon!");
        Debug.Assert(buttonBackground != null,
            "[MenuButton] Could not get reference to background plane!");
        Debug.Assert(icon != null,
            "[MenuButton] Icon is null, will not be visible!");

        buttonIconRectTransform = buttonIcon.GetComponent<RectTransform>();
        Debug.Assert(buttonIconRectTransform != null,
            "[MenuButton] Could not get reference to buttonIcon rectTransform!");
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
        buttonIconRectTransform.localPosition = new Vector3(
            buttonIconRectTransform.localPosition.x,
            buttonIconRectTransform.localPosition.y,
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
        buttonIconRectTransform.localPosition = new Vector3(
            buttonIconRectTransform.localPosition.x,
            buttonIconRectTransform.localPosition.y,
            Mathf.Lerp(
                buttonIconRectTransform.localPosition.z,
                -((hovered) ? activatedHeight : unactivatedHeight),
                Time.deltaTime * activationSpeed
            )
        );

        // Smoothly raise or lower buttonBackground to half height of icon when hovered
        buttonBackground.transform.localPosition = new Vector3(
            buttonBackground.transform.localPosition.x,
            buttonBackground.transform.localPosition.y,
            Mathf.Lerp(
                buttonBackground.transform.localPosition.z,
                -((hovered) ? activatedHeight : unactivatedHeight),
                Time.deltaTime * activationSpeed
            )
        );
    }

    /* Public methods */

    /// <summary>
    /// Sets the hovered status of this button
    /// </summary>
    /// <param name="hovered">Boolean of hovered status</param>
    public void SetHovered(bool hovered) {
        this.hovered = hovered;
    }
}
