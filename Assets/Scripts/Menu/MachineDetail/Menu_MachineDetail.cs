using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEngine.UI;

using TMPro;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Menu_MachineDetail : MonoBehaviour
{
    [Header("Properties")]
    public Machine currMachine;

    [Header("Settings")]
    public string[] stringFieldNamesToGenerate; // String fields to generate handlers for
    public string[] floatFieldNamesToGenerate;  // Float fields to generate handlers for
    public bool generateAngles = true;          // Generate angle modifiers
    public float floatDelta = 0.25f;            // Delta to change float
    public bool useKeyboardMinimalMode = true;  // Use SteamVR minimal keyboard mode
    public bool keyboardShowing = false;      // Is keyboard activated?

    [Header("References")]
    public TextMeshProUGUI machineTitle;
    public Transform machineElementParent;
    public GameObject machineElementStringPrefab, machineElementFloatPrefab; // Changeable machine element

    // Private vars
    private string text = "", keyboardStringField = "";
    private static Menu_MachineDetail activeKeyboard = null;

    private void OnEnable() {
        // Get references
        Debug.Assert(machineTitle != null,
            "[Menu: Machine Detail] Could not get ref to machine title!");
        Debug.Assert(machineElementStringPrefab != null && machineElementFloatPrefab != null,
            "[Menu: Machine Detail] Could not get machine element prefabs!");
        Debug.Assert(machineElementParent != null,
            "[Menu: Machine Detail] Could not get machine element parent!");

        // Safety checks
        foreach (string s in stringFieldNamesToGenerate)
            Debug.Assert(!string.IsNullOrEmpty(s),
                "[Menu: Machine Detail] Invalid string field name!");

        // Listen for keyboard
        SteamVR_Events.System(EVREventType.VREvent_KeyboardCharInput).Listen(OnKeyboard);
        SteamVR_Events.System(EVREventType.VREvent_KeyboardClosed).Listen(OnKeyboardClosed);
    }

    /* Public methods */

    public void DisplayMachine(Machine m) {
        // Safety checks
        if ((currMachine = m) == null) {
            Debug.LogWarning("[Menu: Machine Detail] Invalid machine! Skipping...");
            return;
        }

        // Delete all previous elements
        foreach (Transform t in machineElementParent)
            Destroy(t.gameObject);

        // Generate string fields
        foreach (string s in stringFieldNamesToGenerate)
            GenerateStringElement(s);

        // Generate float fields
        foreach (string s in floatFieldNamesToGenerate)
            GenerateFloatElement(s);

        // Generate angles
        if (generateAngles)
            foreach (Machine.Axis a in currMachine.axes)
                GenerateAxisElement(a);
    }

    /* Private methods */

    #region Element Generators

    /// <summary>
    /// Generates handler that allows for modification of the corresponding axis field
    /// </summary>
    /// <param name="axisName">axis to create handler for</param>
    private void GenerateAxisElement(Machine.Axis a) {
        GameObject g = Instantiate(machineElementFloatPrefab, machineElementParent);
        g.transform.SetAsLastSibling();

        // Get Button components
        TextMeshProUGUI TMP = g.transform.Find("FieldName").GetComponent<TextMeshProUGUI>();
        Button dec = g.transform.Find("ButtonDecrement").GetComponent<Button>();
        Button inc = g.transform.Find("ButtonIncrement").GetComponent<Button>();
        Debug.Assert(TMP != null && dec != null && inc != null);

        // Set name
        TMP.text = a.GetName() + ": ";

        // Add onClick listener to buttons
        Machine.Axis axis = a;
        dec.onClick.AddListener(delegate {
            a.SetValue(Machine.NormalizeAngle(a.GetValue() - 1));
        });
        inc.onClick.AddListener(delegate {
            a.SetValue(Machine.NormalizeAngle(a.GetValue() + 1));
        });
    }

    /// <summary>
    /// Generates handler that allows for modification of the corresponding float field
    /// </summary>
    /// <param name="fieldName">Name of field to modify</param>
    private void GenerateFloatElement(string fieldName) {
        GameObject g = Instantiate(machineElementFloatPrefab, machineElementParent);
        g.transform.SetAsLastSibling();

        // Get Button components
        TextMeshProUGUI TMP = g.transform.Find("FieldName").GetComponent<TextMeshProUGUI>();
        Button dec = g.transform.Find("ButtonDecrement").GetComponent<Button>();
        Button inc = g.transform.Find("ButtonIncrement").GetComponent<Button>();
        Debug.Assert(TMP != null && dec != null && inc != null);

        // Set text with current value
        TMP.text = CapitalizeFirstLetter(fieldName) + ": " +
            typeof(Machine).GetField(fieldName).GetValue(currMachine).ToString();

        // Add onClick listener to buttons
        string s = fieldName;
        inc.onClick.AddListener(delegate {
            typeof(Machine).GetField(s).SetValue(currMachine,
                (float) typeof(Machine).GetField(s).GetValue(currMachine) + floatDelta
            );
        });
        dec.onClick.AddListener(delegate {
            typeof(Machine).GetField(s).SetValue(currMachine,
                (float)typeof(Machine).GetField(s).GetValue(currMachine) - floatDelta
            );
        });
    }

    /// <summary>
    /// Generates handler that allows for modification of the corresponding string field 
    /// </summary>
    /// <param name="fieldName">Name of field to modify</param>
    private void GenerateStringElement(string fieldName) {
        GameObject g = Instantiate(machineElementStringPrefab, machineElementParent);
        g.transform.SetAsLastSibling();
        
        // Get Button components
        TextMeshProUGUI TMP = g.transform.Find("FieldName").GetComponent<TextMeshProUGUI>();
        Button b = g.GetComponent<Button>();

        Debug.Assert(TMP != null && b != null);

        // Capitalize name and get value
        TMP.text = CapitalizeFirstLetter(fieldName) + ": " +
            (string) typeof(Machine).GetField(fieldName).GetValue(currMachine);

        // Add onClick listener to inputField
        string s = fieldName;
        b.onClick.AddListener(delegate {
            if (keyboardShowing)
                return;

            keyboardShowing = true;
            activeKeyboard = this;
            keyboardStringField = s;

            // Open OpenVR keyboard
            SteamVR.instance.overlay.ShowKeyboard(
                (int)EGamepadTextInputMode.k_EGamepadTextInputModeNormal,                       // Input mode
                (int)EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine,           // Line mode
                "MTConnectVR keyboard",                                                         // Description
                256,                                                                            // Max string length
                (string)typeof(Machine).GetField(keyboardStringField).GetValue(currMachine),    // Starting text
                useKeyboardMinimalMode,                                                         // Keyboard minimal mode
                0                                                                               // User value
            );
        });
    }

    #endregion

    #region Helper functions

    private void OnKeyboard(VREvent_t args)
    {
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
        typeof(Machine).GetField(keyboardStringField).SetValue(currMachine, text);
    }

    private void OnKeyboardClosed(VREvent_t args)
    {
        if (activeKeyboard != this)
            return;

        keyboardShowing = false;
        activeKeyboard = null;

        // TODO: change this to be less hacky
        DisplayMachine(currMachine);
    }

    /// <summary>
    /// Returns string with capitalized first letter
    /// </summary>
    /// <param name="input">string to be capitalized</param>
    /// <returns>capitalized string</returns>
    private string CapitalizeFirstLetter(string input)
    {
        // If invalid string, return invalid string
        if (string.IsNullOrEmpty(input))
            return input;

        // If only one char, return uppercase char
        if (input.Length == 1)
            return input.ToUpper();

        // Else return capitalized first char with rest of string
        return input.Substring(0, 1).ToUpper() + input.Substring(1).ToLower();
    }

    #endregion
}
