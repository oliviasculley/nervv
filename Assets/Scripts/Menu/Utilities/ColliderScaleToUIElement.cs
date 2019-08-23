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
        Debug.Assert((c = GetComponent<BoxCollider>()) != null);
        Debug.Assert((t = GetComponent<RectTransform>()) != null);
    }

    /// <summary>Set collider size to rectTransform</summary>
    void Update() {
        c.size = t.rect.size;
    }
    #endregion
}
