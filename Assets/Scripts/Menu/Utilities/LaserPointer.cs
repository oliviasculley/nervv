// System
using System;
using System.Collections;
using System.Collections.Generic;

// UnityEngine
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

// NERVV
using NERVV.Menu;

[RequireComponent(typeof(SteamVR_Behaviour_Pose))]
/// <summary>
/// Raycasts all objects with layer "Menu" and triggers IPointerUpHandler,
/// IPointerDownHandler, IPointerClickHandler, IPointerEnterHandler and
/// IPointerExitHandler. Also generates cube object for visual pointer aid,
/// as well. Mostly based off of SteamVR's laser pointer script, but fixed
/// up where I could.
/// </summary>
/// <seealso cref="IPointerClickHandler"/>
/// <seealso cref="IPointerUpHandler"/>
/// <seealso cref="IPointerDownHandler"/>
/// <seealso cref="IPointerEnterHandler"/>
/// <seealso cref="IPointerExitHandler"/>
public class LaserPointer : MonoBehaviour {
    #region Static
    public const string LAYER_TO_CAST = "Menu";
    public const float MAX_CAST_DIST = 100;
    #endregion

    #region Settings
    [Header("Settings")]
    public SteamVR_Action_Boolean interactWithUI =
        SteamVR_Input.GetBooleanAction("InteractUI");
    public float thickness = 0.002f;
    public Color color;
    public Color clickColor = Color.green;
    #endregion

    #region Vars
    protected List<Transform> enteredTransforms;
    protected Transform currentHovered;
    protected SteamVR_Behaviour_Pose pose;
    protected RaycastHit[] hits;
    protected GameObject holder;
    protected GameObject pointer;
    #endregion

    #region Unity Methods
    /// <summary>Safety checks and initial state</summary>
    protected void OnEnable() {
        pose = GetComponent<SteamVR_Behaviour_Pose>();
        Debug.Assert(pose != null);
        Debug.Assert(interactWithUI != null);

        // Init vars
        hits = null;
        currentHovered = null;
        enteredTransforms = new List<Transform>();

        // Dynamically create holder and pointer gameObjects
        holder = new GameObject();
        holder.transform.parent = this.transform;
        holder.transform.localPosition = Vector3.zero;
        holder.transform.localRotation = Quaternion.identity;

        pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pointer.transform.parent = holder.transform;
        pointer.transform.localScale = new Vector3(thickness, thickness, 100f);
        pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
        pointer.transform.localRotation = Quaternion.identity;

        pointer.GetComponent<MeshRenderer>().material.color = color;
    }

    /// <summary>Clean up laser pointer objects</summary>
    protected void OnDisable() {
        Destroy(holder);
        Destroy(pointer);
    }

    /// <summary>Perform raycasts</summary>
    protected void Update() {
        // Raycast all objects in path
        hits = Physics.RaycastAll(
            ray: new Ray(transform.position, transform.forward),
            maxDistance: MAX_CAST_DIST,
            layerMask: LayerMask.GetMask(LAYER_TO_CAST));
        Debug.DrawRay(transform.position, transform.forward * MAX_CAST_DIST, Color.red);
        
        // Reset pointer length to shortest distance
        SetPointerLength(0, MAX_CAST_DIST, true);

        // No need to run pointer events if no hits
        if (hits.Length == 0) {
            currentHovered = null;
            return;
        }

        {   // Pointer events
            HashSet<Transform> foundHits = new HashSet<Transform>();
            IPointerEnterHandler onPointerIn;
            IPointerDownHandler onPointerDown;
            IPointerClickHandler onPointerClick;
            IPointerUpHandler onPointerUp;
            bool timeToBreak = false;

            foreach (RaycastHit h in hits) {
                // Get handlers and add to hashset
                onPointerIn = h.transform.GetComponent<IPointerEnterHandler>();
                onPointerDown = h.transform.GetComponent<IPointerDownHandler>();
                onPointerClick = h.transform.GetComponent<IPointerClickHandler>();
                onPointerUp = h.transform.GetComponent<IPointerUpHandler>();
                foundHits.Add(h.transform);
                if (onPointerIn != null || onPointerDown != null ||
                    onPointerClick != null || onPointerUp != null) {
                    SetPointerLength(h.distance);
                }

                // Trigger OnPointerIn
                if (h.transform != currentHovered && onPointerIn != null) {
                    // Set current click
                    currentHovered = h.transform;
                    onPointerIn.OnPointerEnter(new PointerEventData(EventSystem.current));

                    // If exit handler, add to be checked for exiting
                    if (h.transform.GetComponent<IPointerExitHandler>() != null) {
                        enteredTransforms.Add(h.transform);
                    }
                    timeToBreak = true;
                }

                // Trigger OnPointerDown
                if (interactWithUI.GetStateDown(pose.inputSource) && onPointerDown != null) {
                    onPointerDown.OnPointerDown(new PointerEventData(EventSystem.current));
                    timeToBreak = true;
                }

                // Trigger OnPointerClickUp
                if (interactWithUI.GetStateUp(pose.inputSource)) {
                    if (onPointerClick != null) {
                        onPointerClick.OnPointerClick(new PointerEventData(EventSystem.current));
                        timeToBreak = true;
                    }

                    if (onPointerUp != null) {
                        onPointerUp.OnPointerUp(new PointerEventData(EventSystem.current));
                        timeToBreak = true;
                    }
                }

                // After running through all pointer events, if triggered then lets break;
                if (timeToBreak) break;
            }

            // Trigger OnPointerOut
            foreach (Transform prevTransform in enteredTransforms.ToArray()) {
                // Check if destroyed
                if (prevTransform == null) {
                    enteredTransforms.Remove(null);
                    continue;
                }

                // If previousContact was not found, trigger OnPointerOut
                if (!foundHits.Contains(prevTransform)) {
                    IPointerExitHandler e = prevTransform.GetComponent<IPointerExitHandler>();
                    Debug.Assert(e != null);
                    e.OnPointerExit(new PointerEventData(EventSystem.current));
                    enteredTransforms.Remove(prevTransform);
                    if (prevTransform == currentHovered)
                        currentHovered = null;
                }
            }
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Will set pointer length to larger length unless otherwise specified
    /// </summary>
    /// <param name="dist">Length in Unity units to set pointer</param>
    /// <param name="overwrite">Reset distance to a shorter distance</param>
    protected void SetPointerLength(float dist, float maxDistance = 100, bool overwrite = true) {
        if (dist < 0) throw new ArgumentOutOfRangeException();
        var clamped = Mathf.Clamp(dist, 0, maxDistance);

        // Set pointer visibility
        holder.SetActive(clamped > Mathf.Epsilon);
        pointer.SetActive(clamped > Mathf.Epsilon);

        // Set pointer click color
        pointer.GetComponent<MeshRenderer>().material.color =
            interactWithUI.GetState(pose.inputSource) ? clickColor : color;

        // Set pointer click thickness
        pointer.transform.localScale = interactWithUI.GetState(pose.inputSource) ?
            new Vector3(thickness * 5f, thickness * 5f, pointer.transform.localScale.z) :
            new Vector3(thickness, thickness, pointer.transform.localScale.z);

        // Set pointer length
        if (clamped > pointer.transform.localScale.z || overwrite) {
            pointer.transform.localPosition = new Vector3(0f, 0f, clamped / 2f);
            pointer.transform.localScale = new Vector3(
                pointer.transform.localScale.x,
                pointer.transform.localScale.y,
                clamped);
        }
    }
    #endregion
}
