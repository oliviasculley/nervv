using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(RectTransform))]
public class ColliderScaleToUIElement : MonoBehaviour
{
    // Private vars
    BoxCollider c;
    RectTransform t;

    private void OnEnable()
    {
        c = GetComponent<BoxCollider>();
        Debug.Assert(c != null, "[ColliderScale] Could not get box collider!");

        t = GetComponent<RectTransform>();
        Debug.Assert(t != null, "[ColliderScale] Could not get rectTransform!");
    }

    private void Update()
    {
        c.size = t.rect.size;
    }
}
