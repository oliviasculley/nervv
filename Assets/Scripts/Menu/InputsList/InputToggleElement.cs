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
        [SerializeField, Header("Input Toggle Properties")]
        protected IInputSource _input;
        /// <summary>
        /// The current input pointed to by the toggle element.
        /// If set, enables the element!
        /// </summary>
        public IInputSource Input => _input;
        #endregion

        #region Public Methods
        /// <summary>Invokes the OnToggled event</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when Input source has not been initialized!
        /// </exception>
        public override void Toggle() {
            if (Input == null) {
                LogError("Input not set!");
                return;
            }

            Input.InputEnabled = !Input.InputEnabled;
            base.Toggle();
        }

        /// <summary>Method to initialize InputToggleButton</summary>
        public void Initialize(IInputSource input, bool initialToggleState) {
            _input = input;
            Title = _input.Name;
            base.Initialize(initialToggleState);
        }
        #endregion
    }
}
