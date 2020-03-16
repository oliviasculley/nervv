// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

using NERVV;

namespace NERVV.Menu.InputsListPanel {
    public class InputToggleElement : Elements.ToggleButtonElement {
        #region Properties
        [SerializeField]
        protected IInputSource _input;
        public IInputSource Input => _input;
        #endregion

        #region Unity Methods
        protected override void OnEnable() {
            if (Input == null) {
                Debug.LogError("Input not set onEnable!")
            }

            base.OnEnable();
        }
        #endregion

        #region Public Methods
        /// <summary>Invokes the OnToggled event</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when Input source has not been initialized!
        /// </exception>
        public override void Toggle() {
            Input.InputEnabled = !Input.InputEnabled;
            base.InvokeOnToggled();
        }

        /// <summary>Method to initialize InputToggleButton</summary>
        public void Initialize(IInputSource input, bool initialToggleState) {
            _input = input;
            base.Initialize(initialToggleState);
        }
        #endregion
    }
}
