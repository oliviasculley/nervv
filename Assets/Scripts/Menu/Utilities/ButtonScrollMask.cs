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
    public float minHeight, maxHeight;

    #endregion

    #region Private vars

    RectTransform t;

    #endregion

    #region Unity Methods

    private void OnEnable() {
        Debug.Assert(content != null, "[ColliderScale] Could not get content parent!");

        t = GetComponent<RectTransform>();
        Debug.Assert(t != null, "[ColliderScale] Could not get rectTransform!");
    }

    private void Update() {
        // Set visiblity of each object depending on position
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
