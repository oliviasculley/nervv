// System
using System.Collections;
using System.Collections.Generic;

// UnityEngine
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

// NERVV
using NERVV.Menu;

[RequireComponent(typeof(SteamVR_Behaviour_Pose))]
public class LaserPointer : MonoBehaviour {
    #region Settings
    [Header("Settings")]
    public SteamVR_Action_Boolean interactWithUI =
        SteamVR_Input.GetBooleanAction("InteractUI");
    public float thickness = 0.002f;
    public Color color;
    public Color clickColor = Color.green;
    #endregion

    #region References
    [Header("References")]
    public Menu menu;
    #endregion

    #region Vars
    List<Transform> enteredTransforms;
    Transform currentHovered;
    SteamVR_Behaviour_Pose pose;
    RaycastHit[] hits;
    GameObject holder;
    GameObject pointer;
    #endregion

    #region Unity Methods
    /// <summary>Safety checks and initial state</summary>
    void OnEnable() {
        pose = GetComponent<SteamVR_Behaviour_Pose>();
        Debug.Assert(pose != null);
        Debug.Assert(interactWithUI != null);
        Debug.Assert(menu != null);

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
    void OnDisable() {
        Destroy(holder);
        Destroy(pointer);
    }

    /// <summary>Perform raycasts</summary>
    void Update() {
        // Set self enabled or disabled from Menu state
        holder.SetActive(menu.Visible);
        pointer.SetActive(menu.Visible);
        if (!menu.Visible)
            return;

        // Raycast all objects in path
        hits = Physics.RaycastAll(new Ray(transform.position, transform.forward), 100, LayerMask.GetMask("Menu"));
        Debug.DrawRay(transform.position, transform.forward * 100, Color.red);
        if (hits.Length == 0) {
            SetPointerLength(0);
        } else {
            var max = 100f;
            foreach (RaycastHit hit in hits)
                max = hit.distance < max ? hit.distance : max;
            SetPointerLength(max, true);
        }

        // Pointer events
        OnPointerOut();
        OnPointerIn();
        OnPointerDown();
        OnPointerClickUp();

        // Set pointer click color
        pointer.GetComponent<MeshRenderer>().material.color =
            interactWithUI.GetState(pose.inputSource) ? clickColor : color;

        // Set pointer click thickness
        pointer.transform.localScale = interactWithUI.GetState(pose.inputSource) ?
            new Vector3(thickness * 5f, thickness * 5f, pointer.transform.localScale.z) :
            new Vector3(thickness, thickness, pointer.transform.localScale.z);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Will set pointer length to smaller length unless otherwise specified
    /// </summary>
    /// <param name="dist">Length in Unity units to set pointer</param>
    /// <param name="overwrite">Set distance to a longer distance</param>
    void SetPointerLength(float dist, bool overwrite = true) {
        if (dist < pointer.transform.localScale.z || overwrite) {
            pointer.transform.localPosition = new Vector3(0f, 0f, dist / 2f);
            pointer.transform.localScale = new Vector3(pointer.transform.localScale.x, pointer.transform.localScale.y, dist);
        }
    }
    #endregion

    #region EventHandlers
    void OnPointerClickUp() {
        IPointerClickHandler onPointerClick;
        IPointerUpHandler onPointerUp;
        if (interactWithUI.GetStateUp(pose.inputSource)) {
            foreach (RaycastHit h in hits)
                if ((onPointerClick = h.transform.GetComponent<IPointerClickHandler>()) != null) {
                    onPointerClick.OnPointerClick(new PointerEventData(EventSystem.current));
                    break;
                }

            foreach (RaycastHit h in hits)
                if ((onPointerUp = h.transform.GetComponent<IPointerUpHandler>()) != null) {
                    onPointerUp.OnPointerUp(new PointerEventData(EventSystem.current));
                    break;
                }
        }
    }

    void OnPointerDown() {
        IPointerDownHandler onPointerDown;
        if (interactWithUI.GetStateDown(pose.inputSource))
            foreach (RaycastHit h in hits)
                if ((onPointerDown = h.transform.GetComponent<IPointerDownHandler>()) != null) {
                    onPointerDown.OnPointerDown(new PointerEventData(EventSystem.current));
                    break;
                }
    }

    void OnPointerOut() {
        foreach (Transform prevTransform in enteredTransforms.ToArray()) {
            // Check if destroyed
            if (prevTransform == null) {
                enteredTransforms.Remove(null);
                continue;
            }

            bool found = false; // is prevTransform in list of raycasts?

            // Go through all hits and find found
            foreach (RaycastHit h in hits) {
                if (h.transform == prevTransform) {
                    found = true;
                    break;
                }
            }

            // If previousContact was not found, trigger OnPointerOut
            if (!found) {
                IPointerExitHandler e = prevTransform.GetComponent<IPointerExitHandler>();
                Debug.Assert(e != null);
                e.OnPointerExit(
                    new PointerEventData(EventSystem.current)
                );
                enteredTransforms.Remove(prevTransform);
                if (prevTransform == currentHovered)
                    currentHovered = null;
            }
        }
    }

    void OnPointerIn() {
        // OnPointerIn for first object with IPointerEnterHandler
        IPointerEnterHandler onPointerIn;
        foreach (RaycastHit h in hits) {
            if (h.transform != currentHovered &&
                (onPointerIn = h.transform.GetComponent<IPointerEnterHandler>()) != null) {

                // Set current click
                currentHovered = h.transform;
                onPointerIn.OnPointerEnter(new PointerEventData(EventSystem.current));

                // If exit handler, add to be checked for exiting
                if (h.transform.GetComponent<IPointerExitHandler>() != null) {
                    enteredTransforms.Add(h.transform);
                }
                break;
            }
        }
        if (hits.Length == 0) {
            currentHovered = null;
        }
    }
    #endregion
}
