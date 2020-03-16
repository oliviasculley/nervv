// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NERVV.Menu.Elements {
    public class ToggleButtonElement : MenuComponent {
        #region Properties
        [SerializeField, Header("Properties")]
        protected bool _toggled = false;
        /// <summary>Contains toggled state</summary>
        public bool Toggled {
            get => _toggled;
            set {
                _toggled = value;
                ToggleButton.SetIsOnWithoutNotify(_toggled);
                InvokeOnToggled();
            }
        }

        /// <summary>Invoked when button is toggled</summary>
        public event EventHandler OnToggled;
        #endregion

        #region Settings
        [SerializeField, Header("Settings")]
        protected string _title = "";
        public string Title {
            get => _title;
            set {
                _title = value;
                TitleText.text = _title;
            }
        }
        #endregion

        #region References
        [SerializeField, Header("References")]
        protected Button MainItemButton;

        [SerializeField]
        protected Toggle ToggleButton;

        [SerializeField]
        protected TextMeshProUGUI TitleText;
        #endregion

        #region Unity Methods
        /// <summary>Run safety checks on references</summary>
        protected void Awake() {
            if (MainItemButton == null) throw new ArgumentNullException();
            if (ToggleButton == null) throw new ArgumentNullException();
            if (TitleText == null) throw new ArgumentNullException();
            gameObject.SetActive(false);
        }
        #endregion

        #region Public Methods
        /// <summary>Invokes the OnToggled event</summary>
        public virtual void InvokeOnToggled() => OnToggled?.Invoke(this, null);

        /// <summary>Convenience method used to toggle element</summary>
        public virtual void Toggle() => Toggled = !Toggled;

        /// <summary>Base method to initialize element</summary>
        /// <remarks>Don't use this one if there is another method with more parameters!</remarks>
        public virtual void Initialize(bool initialToggleState) {
            _toggled = initialToggleState;
            ToggleButton.SetIsOnWithoutNotify(_toggled);
            gameObject.SetActive(true);
        }
        #endregion
    }
}