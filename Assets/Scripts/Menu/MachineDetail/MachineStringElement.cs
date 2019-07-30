// System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;
using TMPro;
using Valve.VR;
using System.Reflection;

namespace MTConnectVR.Menu {
    /// <summary>Machine element in machine properties for string</summary>
    public class MachineStringElement : MachineElement {
        #region Properties
        [Header("Properties")]

        public string fieldName;
        public IMachine currMachine;

        #endregion

        #region Settings
        [Header("Settings")]

        [Tooltip("Use SteamVR minimal keyboard mode")]
        public bool useKeyboardMinimalMode = true;

        #endregion

        #region References
        [Header("References")]

        public TextMeshProUGUI elementTitle;

        #endregion

        #region Private vars

        private static MachineStringElement activeKeyboard = null;
        private string text = "";

        #endregion

        #region Unity Methods

        private new void OnEnable() {
            Debug.Assert(elementTitle != null,
                "Could not get string element title TMP_UGUI!");

            // Listen for keyboard
            SteamVR_Events.System(EVREventType.VREvent_KeyboardCharInput).Listen(OnKeyboard);
            SteamVR_Events.System(EVREventType.VREvent_KeyboardClosed).Listen(OnKeyboardClosed);
        }

        #endregion

        #region Public Functions

        /// <summary>Initialize float element with needed parameters</summary>
        /// <param name="fieldName"></param>
        /// <param name="currMachine"></param>
        public void InitializeElement(string fieldName, IMachine currMachine) {
            Debug.Assert(currMachine != null && !string.IsNullOrEmpty(fieldName));

            this.fieldName = fieldName;
            this.currMachine = currMachine;

            UpdateText();
        }

        #endregion

        #region Private Functions

        /// <summary>Gets field value with reflection</summary>
        /// <returns>Field value, can return null!</returns>
        private string GetFieldValue() {
            FieldInfo info;
            if ((info = typeof(Machine).GetField(
                    fieldName,
                    BindingFlags.NonPublic | BindingFlags.Instance)
                ) != null)
                return (string)info.GetValue(currMachine);
            Debug.LogError("Could not get field value: " + fieldName);
            return null;
        }

        /// <summary>Sets field value with reflection</summary>
        /// <param name="value">Field value</param>
        private void SetField(string value) {
            FieldInfo info;
            if (currMachine != null &&
                (info = typeof(Machine).GetField(
                    fieldName,
                    BindingFlags.NonPublic | BindingFlags.Instance)
                ) != null)
                info.SetValue(currMachine, value);
            else
                Debug.LogError("Could not set field value: " + fieldName);
        }

        /// <summary>Update text readout with current value</summary>
        private void UpdateText() {
            // Set text with current value
            elementTitle.text = CapitalizeFirstLetter(fieldName.Substring(1)) + ": ";
            if (GetFieldValue() != null)
                elementTitle.text += GetFieldValue().ToString();
        }

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
                    (int)EGamepadTextInputMode.k_EGamepadTextInputModeNormal,                       // Input mode
                    (int)EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine,           // Line mode
                    "MTConnectVR keyboard",                                                         // Description
                    256,                                                                            // Max string length
                    GetFieldValue() ?? "",                                                          // Starting text
                    useKeyboardMinimalMode,                                                         // Keyboard minimal mode
                    0                                                                               // User value
                );
            } else {
                if (GetFieldValue() == null)
                    Debug.LogError("FieldValue null for: " + fieldName);
                if (SteamVR.instance == null)
                    Debug.LogError("Could not get SteamVR Instance");
                if (SteamVR.instance != null && SteamVR.instance.overlay == null)
                    Debug.LogError("Could not get SteamVR Instance Overlay");
            }
        }

        private void OnKeyboard(VREvent_t args) {
            if (activeKeyboard != this)
                return;

            // Code used from OpenVR Samples
            // https://github.com/ValveSoftware/openvr/blob/41bfc14efef21b2959394d8b4c29b82c3bdd7d12/samples/unity_keyboard_sample/Assets/KeyboardSample.cs

            VREvent_Keyboard_t keyboard = args.data.keyboard;
            byte[] inputBytes = new byte[] { keyboard.cNewInput0, keyboard.cNewInput1, keyboard.cNewInput2, keyboard.cNewInput3, keyboard.cNewInput4, keyboard.cNewInput5, keyboard.cNewInput6, keyboard.cNewInput7 };
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

        private void OnKeyboardClosed(VREvent_t args) {
            if (activeKeyboard != this)
                return;
            else
                activeKeyboard = null;
        }

        #endregion
    }
}