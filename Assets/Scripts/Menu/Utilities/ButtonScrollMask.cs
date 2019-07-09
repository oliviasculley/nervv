using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ButtonScrollMask : MonoBehaviour {
    
    [Header("Properties")]
        public Transform content;
        [Range(0, 1)]
        public float minHeight, maxHeight;

    // Private vars
    RectTransform t;

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
}
