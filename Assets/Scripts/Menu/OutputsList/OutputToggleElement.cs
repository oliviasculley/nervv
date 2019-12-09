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
        /// If set, enables the toggle button!
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if set to null</exception>
        public IOutputSource Output {
            get => _output;
            set {
                _output = value ?? throw new ArgumentNullException();
                Title = _output.Name;
                InitialState = _output.OutputEnabled;
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
            Output.OutputEnabled = !Output.OutputEnabled;
            base.InvokeOnToggled();
        }
        #endregion
    }
}
