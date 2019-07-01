using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuButtonAnim : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Properties")]
    public bool hovered;

    [Header("Animation Settings")]
    public float unactivatedHeight = 0.05f;     // Resting height for sprite
    public float activatedHeight = 0.025f;      // Height of activated sprite
    public float activationSpeed = 1f;          // Speed to activate icons
    public readonly float planeRatio = 1f;  // Ratio of background plane to image

    [Header("References")]
    public Menu menu;                           // Menu that menuButton is attached to
    public GameObject buttonBackground, buttonIcon;

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

    /* Public methods */

    public void OnPointerEnter(PointerEventData data)
    {
        hovered = true;
    }

    public void OnPointerExit(PointerEventData data)
    {
        hovered = false;
    }
}
