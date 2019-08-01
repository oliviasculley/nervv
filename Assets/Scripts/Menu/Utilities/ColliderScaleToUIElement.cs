// System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways,
RequireComponent(typeof(BoxCollider)),
RequireComponent(typeof(RectTransform))]
public class ColliderScaleToUIElement : MonoBehaviour {
    #region Vars
    BoxCollider c;
    RectTransform t;
    #endregion

    #region Unity Methods
    /// <summary>Get dynamic references</summary>
    void OnEnable() {
        c = GetComponent<BoxCollider>();
        Debug.Assert(c != null, "[ColliderScale] Could not get box collider!");

        t = GetComponent<RectTransform>();
        Debug.Assert(t != null, "[ColliderScale] Could not get rectTransform!");
    }

    /// <summary>Set collider size to rectTransform</summary>
    void Update() {
        c.size = t.rect.size;
    }
    #endregion
}
