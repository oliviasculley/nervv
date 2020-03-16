// System
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

// Unity Engine
using UnityEngine;
using TMPro;
using Valve.VR;

namespace NERVV.Menu.MachineDetailPanel {
    /// <summary>Machine element in machine properties for string</summary>
    public class MachineStringElement : MachineElement {
        #region Properties
        [SerializeField, Header("Properties")]
        protected PropertyInfo _property = null;
        public PropertyInfo Property {
            get => _property; 
            set {
                if (value == null) throw new ArgumentNullException();
                if (GetMemberType(value) != typeof(string)) throw new ArgumentException();
                _property = value;
            }
        }

        [SerializeField]
        protected IMachine _currMachine = null;
        public IMachine CurrMachine {
            get => _currMachine;
            set => _currMachine = value ?? throw new ArgumentNullException();
        }
        #endregion

        #region Settings
        [Tooltip("Use SteamVR minimal keyboard mode"), Header("Settings")]
        public bool useKeyboardMinimalMode = true;
        #endregion

        #region References
        [Header("References")]
        public TextMeshProUGUI elementTitle;
        #endregion

        #region Vars
        static MachineStringElement activeKeyboard = null;
        string text = "";
        #endregion

        #region Unity Methods
        /// <summary>Check references and start SteamVR keyboard</summary>
        /// <exception cref="ArgumentNullException">Thrown if elementTitle is null</exception>
        /// <exception cref="ArgumentException">Thrown if field is not string type</exception>
        protected override void OnEnable() {
            base.OnEnable();

            if (elementTitle == null) throw new ArgumentNullException();

            // Ensure necessary fields are filled
            if (Property == null || CurrMachine == null)
                gameObject.SetActive(false);

            // Listen for keyboard
            SteamVR_Events.System(EVREventType.VREvent_KeyboardCharInput).Listen(OnKeyboard);
            SteamVR_Events.System(EVREventType.VREvent_KeyboardClosed).Listen(OnKeyboardClosed);

            UpdateText();
        }
        #endregion

        #region Public Functions
        /// <summary>Initialize float element with needed parameters</summary>
        /// <param name="fieldName"></param>
        /// <param name="CurrMachine"></param>
        public void InitializeElement(PropertyInfo Property, IMachine CurrMachine) {
            this.Property = Property;
            this.CurrMachine = CurrMachine;

            gameObject.SetActive(true);
        }
        #endregion

        #region Methods
        /// <summary>Gets string from field with reflection</summary>
        protected string GetFieldValue() => (string)GetMemberValue(Property, CurrMachine);

        /// <summary>Sets string field with reflection</summary>
        /// <param name="value">Field value</param>
        protected void SetField(string value) => SetMemberValue(Property, CurrMachine, value);

        /// <summary>Update text readout with field name and current value</summary>
        protected void UpdateText() =>
            elementTitle.text = $"{CapitalizeFirstLetter(Property.Name)}: {GetFieldValue()}";
        #endregion

        #region SteamVR Keyboard Helper functions
        public void OpenKeyboard() {
            if (activeKeyboard == null)
                activeKeyboard = this;
            else
                return;

            // Open OpenVR keyboard
            if (GetFieldValue() != null && SteamVR.instance != null && SteamVR.instance.overlay != null) {
                SteamVR.instance.overlay.ShowKeyboard(
                    (int)EGamepadTextInputMode.k_EGamepadTextInputModeNormal,                   // Input mode
                    (int)EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine,       // Line mode
                    "NERVV keyboard",                                                           // Description
                    256,                                                                        // Max string length
                    GetFieldValue() ?? "",                                                      // Starting text
                    useKeyboardMinimalMode,                                                     // Keyboard minimal mode
                    0                                                                           // User value
                );
            } else {
                if (GetFieldValue() == null)
                    LogError($"FieldValue null for: {Property.Name}");
                if (SteamVR.instance == null)
                    LogError("Could not get SteamVR Instance");
                if (SteamVR.instance != null && SteamVR.instance.overlay == null)
                    LogError("Could not get SteamVR Instance Overlay");
            }
        }

        /// <summary>Callback when keyboard input is registered</summary>
        /// <remarks>
        /// Code used from OpenVR Samples. See: https://github.com/ValveSoftware/openvr/blob/41bfc14efef21b2959394d8b4c29b82c3bdd7d12/samples/unity_keyboard_sample/Assets/KeyboardSample.cs
        /// </remarks>
        void OnKeyboard(VREvent_t args) {
            if (activeKeyboard != this)
                return;

            VREvent_Keyboard_t keyboard = args.data.keyboard;
            byte[] inputBytes = new byte[] {
                keyboard.cNewInput0,
                keyboard.cNewInput1,
                keyboard.cNewInput2,
                keyboard.cNewInput3,
                keyboard.cNewInput4,
                keyboard.cNewInput5,
                keyboard.cNewInput6,
                keyboard.cNewInput7
            };

            int len = 0;
            for (; inputBytes[len] != 0 && len < 7; len++) ;
            string input = System.Text.Encoding.UTF8.GetString(inputBytes, 0, len);

            // Get value from keyboard
            if (useKeyboardMinimalMode) {
                if (input == "\b") {
                    if (text.Length > 0) {
                        text = text.Substring(0, text.Length - 1);
                    }
                } else if (input == "\x1b") {
                    // Close the keyboard
                    var vr = SteamVR.instance;
                    vr.overlay.HideKeyboard();
                    activeKeyboard = null;
                } else {
                    text += input;
                }
            } else {
                System.Text.StringBuilder textBuilder = new System.Text.StringBuilder(1024);
                uint size = SteamVR.instance.overlay.GetKeyboardText(textBuilder, 1024);
                text = textBuilder.ToString();
            }

            // Set field with value from keyboard
            SetField(text);
            UpdateText();
        }

        void OnKeyboardClosed(VREvent_t args) {
            if (activeKeyboard != this)
                return;
            else
                activeKeyboard = null;
        }
        #endregion
    }
}