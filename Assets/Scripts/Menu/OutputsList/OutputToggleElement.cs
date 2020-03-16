// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

using NERVV;

namespace NERVV.Menu.OutputsListPanel {
    public class OutputToggleElement : Elements.ToggleButtonElement {
        #region Properties
        [SerializeField, Header("Output Toggle Properties")]
        protected IOutputSource _output;
        /// <summary>
        /// The current output pointed to by the toggle element.
        /// If set, enables the element!
        /// </summary>
        public IOutputSource Output => _output;
        #endregion

        #region Public Methods
        /// <summary>Invokes the OnToggled event</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when Output source has not been initialized!
        /// </exception>
        public override void Toggle() {
            if (Output == null) {
                LogError("Input not set!");
                return;
            }

            Output.OutputEnabled = !Output.OutputEnabled;
            base.Toggle();
        }

        /// <summary>Method to initialize InputToggleButton</summary>
        public void Initialize(IOutputSource output, bool initialToggleState) {
            _output = output;
            Title = _output.Name;
            base.Initialize(initialToggleState);
        }
        #endregion
    }
}
