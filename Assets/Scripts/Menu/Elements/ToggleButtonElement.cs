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
        public bool Toggled {
            get => _toggled;
            set {
                _toggled = value;
                // No need to invoke OnToggled,
                // toggle component will do that for us
            }
        }

        public event EventHandler OnMainBodyClicked;
        public event EventHandler OnToggled;
        #endregion

        #region Settings
        [SerializeField, Header("Settings")]
        protected bool _initialState = false;
        /// <summary>Initial state. Set this value in order to set element active!</summary>
        public bool InitialState {
            get => _initialState;
            set {
                _initialState = value;
                ToggleButton.isOn = _initialState;
                gameObject.SetActive(true);
            }
        }

        [SerializeField]
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
        /// <summary>Invokes the OnMainBodyClicked event</summary>
        public virtual void InvokeOnMainBodyClicked() {
            EventHandler handler = OnMainBodyClicked;
            handler?.Invoke(this, null);
        }

        /// <summary>Invokes the OnToggled event</summary>
        public virtual void InvokeOnToggled() {
            EventHandler handler = OnToggled;
            handler?.Invoke(this, null);
        }
        #endregion
    }
}