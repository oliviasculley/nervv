// System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ButtonScrollMask : MonoBehaviour {
    #region Properties
    [Header("Properties")]
    public Transform content;

    [Range(0, 1)]
    public float minHeight;

    [Range(0, 1)]
    public float maxHeight;
    #endregion

    #region Vars
    RectTransform t;
    #endregion

    #region Unity Methods
    /// <summary>Get and check references on object load</summary>
    void OnEnable() {
        Debug.Assert(content != null);
        Debug.Assert((t = GetComponent<RectTransform>()) != null);
    }

    /// <summary>Set visiblity of each object depending on position</summary>
    void Update() {
        foreach (Transform obj in content) {
            MachineElement e = obj.GetComponent<MachineElement>();
            if (e != null) {
                float yDelta = (transform.position - obj.position).y;
                e.Visible = yDelta >= minHeight && yDelta <= maxHeight;
            }
        }
    }
    #endregion
}