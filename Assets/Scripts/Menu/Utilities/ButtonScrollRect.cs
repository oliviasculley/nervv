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
        [SerializeField, Header("Button Properties")]
        float _targetVertNormPos;
        /// <summary>Target position as normalized value between 0 and 1</summary>
        public float targetVertNormPos {
            get => _targetVertNormPos;
            set => _targetVertNormPos = Mathf.Clamp(value, 0, 1);
        }
        #endregion

        #region Button Settings
        /// <summary>Amount to scroll by</summary>
        [SerializeField,
        Range(0, 1),
        Tooltip("Amount to scroll by"), Header("Button Settings")]
        public float scrollDelta = 0.27f;

        /// <summary>Speed to scroll viewport</summary>
        [SerializeField, Range(0, 1), Tooltip("Speed to scroll viewport")]
        public float scrollSpeed = 0.3f;
        #endregion

        #region Unity Methods
        /// <summary>Set initial state</summary>
        new void OnEnable() {
            targetVertNormPos = 1;
        }

        /// <summary>Continually lerp towards target position</summary>
        void Update() {
            verticalNormalizedPosition =
                Mathf.Lerp(verticalNormalizedPosition, targetVertNormPos, scrollSpeed);
        }
        #endregion

        #region Public Methods
        /// <summary>Scrolls down by a set delta</summary>
        public void ScrollDown() {
            targetVertNormPos -= scrollDelta;
        }

        /// <summary>Scrolls up by a set delta</summary>
        public void ScrollUp() {
            targetVertNormPos += scrollDelta;
        }
        #endregion
    }
}
