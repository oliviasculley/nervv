using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI {
    /// <summary>
    /// Scrollrect mainly driven by button scrolls
    /// </summary>
    public class ButtonScrollRect : ScrollRect {

        [Header("Button Properties")]
        [SerializeField] private float _targetVertNormPos;
        public float targetVertNormPos {
            get { return _targetVertNormPos; }
            set { _targetVertNormPos = Mathf.Clamp(value, 0, 1); }
        }

        [Header("Button Settings")]
        [Range(0, 1)]
        public float scrollDelta = 0.25f;   // Amount to scroll by
        [Range(0, 1)]
        public float scrollSpeed = 0.3f;    // Speed to scroll viewport

        private new void OnEnable() {
            targetVertNormPos = 1;
        }

        private void Update() {
            // Continually lerp towards target position
            verticalNormalizedPosition = Mathf.Lerp(verticalNormalizedPosition, targetVertNormPos, scrollSpeed);
        }

        /* Public Methods */

        /// <summary>
        /// Scrolls down by a set delta
        /// </summary>
        public void ScrollDown() {
            targetVertNormPos -= scrollDelta;
        }

        /// <summary>
        /// Scrolls up by a set delta
        /// </summary>
        public void ScrollUp() {
            targetVertNormPos += scrollDelta;
        }
    }
}
