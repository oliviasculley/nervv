// System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI {
    /// <summary>
    /// Scrollrect mainly driven by button scrolls
    /// </summary>
    public class ButtonScrollRect : ScrollRect {

        #region Button Properties
        [Header("Button Properties")]

        [SerializeField] private float _targetVertNormPos;
        /// <summary> Target position as normalized value between 0 and 1 </summary>
        public float targetVertNormPos {
            get { return _targetVertNormPos; }
            set { _targetVertNormPos = Mathf.Clamp(value, 0, 1); }
        }

        #endregion

        #region Button Settings
        [Header("Button Settings")]

        /// <summary> Amount to scroll by </summary>
        [Tooltip("Amount to scroll by")]
        [Range(0, 1)]
        [SerializeField] public float scrollDelta = 0.27f;

        /// <summary> Speed to scroll viewport </summary>
        [Tooltip("Speed to scroll viewport")]
        [Range(0, 1)]
        [SerializeField] public float scrollSpeed = 0.3f;

        #endregion

        #region Unity Methods

        private new void OnEnable() {
            targetVertNormPos = 1;
        }

        private void Update() {
            // Continually lerp towards target position
            verticalNormalizedPosition = Mathf.Lerp(verticalNormalizedPosition, targetVertNormPos, scrollSpeed);
        }

        #endregion

        #region Public Methods

        /// <summary> Scrolls down by a set delta </summary>
        public void ScrollDown() {
            targetVertNormPos -= scrollDelta;
        }

        /// <summary> Scrolls up by a set delta </summary>
        public void ScrollUp() {
            targetVertNormPos += scrollDelta;
        }

        #endregion

    }
}
