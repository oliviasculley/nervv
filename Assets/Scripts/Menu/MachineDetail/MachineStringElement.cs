using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class MachineStringElement : MachineElement
{
    [Header("Properties")]
    public string fieldName;
    public Machine currMachine;
    public bool keyboardShowing = false;      // Is keyboard activated?

    [Header("Settings")]
    public bool useKeyboardMinimalMode = true;  // Use SteamVR minimal keyboard mode

    [Header("References")]
    public TextMeshProUGUI elementTitle;

    // Private vars
    private static MachineStringElement activeKeyboard = null;
    private string text = "";

    private new void OnEnable() {
        Debug.Assert(elementTitle != null,
            "Could not get string element title TMP_UGUI!");

        // Listen for keyboard
        SteamVR_Events.System(EVREventType.VREvent_KeyboardCharInput).Listen(OnKeyboard);
        SteamVR_Events.System(EVREventType.VREvent_KeyboardClosed).Listen(OnKeyboardClosed);
    }

    /* Public Functions */

    /// <summary>
    /// Initialize float element with needed parameters
    /// </summary>
    /// <param name="fieldName"></param>
    /// <param name="currMachine"></param>
    public void InitializeElement(string fieldName, Machine currMachine) {
        Debug.Assert(currMachine != null && !string.IsNullOrEmpty(fieldName));

        this.fieldName = fieldName;
        this.currMachine = currMachine;

        UpdateText();
    }

    /* Private Functions */

    /// <summary>
    /// Gets field value with reflection
    /// </summary>
    /// <returns>Field value</returns>
    private string GetField() {
        return (string)typeof(Machine).GetField(fieldName).GetValue(currMachine);
    }

    /// <summary>
    /// Sets field value with reflection
    /// </summary>
    /// <param name="value">Field value</param>
    private void SetField(string value) {
        typeof(Machine).GetField(fieldName).SetValue(currMachine, value);
    }

    /// <summary>
    /// Update text readout with current value
    /// </summary>
    private void UpdateText() {
        // Set text with current value
        elementTitle.text =
            CapitalizeFirstLetter(fieldName) +
            ": " +
            GetField().ToString();
    }

    #region SteamVR Keyboard Helper functions

    public void OpenKeyboard() {
        if (keyboardShowing)
            return;

        keyboardShowing = true;
        activeKeyboard = this;

        // Open OpenVR keyboard
        SteamVR.instance.overlay.ShowKeyboard(
            (int)EGamepadTextInputMode.k_EGamepadTextInputModeNormal,                       // Input mode
            (int)EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine,           // Line mode
            "MTConnectVR keyboard",                                                         // Description
            256,                                                                            // Max string length
            (string)typeof(Machine).GetField(fieldName).GetValue(currMachine),              // Starting text
            useKeyboardMinimalMode,                                                         // Keyboard minimal mode
            0                                                                               // User value
        );
    }

    private void OnKeyboard(VREvent_t args) {
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
                keyboardShowing = false;
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
    }

    private void OnKeyboardClosed(VREvent_t args) {
        if (activeKeyboard != this)
            return;

        keyboardShowing = false;
        activeKeyboard = null;
    }

    #endregion
}
