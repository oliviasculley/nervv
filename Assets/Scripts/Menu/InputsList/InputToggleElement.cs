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
        /// If set, enables the toggle button!
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if set to null</exception>
        public IInputSource Input {
            get => _input;
            set {
                _input = value ?? throw new ArgumentNullException();
                Title = _input.Name;
                InitialState = _input.InputEnabled;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>Invokes the OnMainBodyClicked event</summary>
        public override void InvokeOnMainBodyClicked() {
            ToggleButton.isOn = !ToggleButton.isOn;
            base.InvokeOnMainBodyClicked();
        }

        /// <summary>Invokes the OnToggled event</summary>
        public override void InvokeOnToggled() {
            Input.InputEnabled = !Input.InputEnabled;
            base.InvokeOnToggled();
        }
        #endregion
    }
}
