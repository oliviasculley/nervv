using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(BoxCollider))]
public class MachineElement : MonoBehaviour {
    /// <summary>
    /// Returns string with capitalized first letter
    /// </summary>
    /// <param name="input">string to be capitalized</param>
    /// <returns>capitalized string</returns>
    public static string CapitalizeFirstLetter(string input) {
        // If invalid string, return invalid string
        if (string.IsNullOrEmpty(input))
            return input;

        // If only one char, return uppercase char
        if (input.Length == 1)
            return input.ToUpper();

        // Else return capitalized first char with rest of string
        return input.Substring(0, 1).ToUpper() + input.Substring(1).ToLower();
    }

    [Header("Element Properties")]
    public bool _visible;
    public bool Visible {
        get { return _visible; }
        set {
            if (buttons == null) buttons = GetComponentsInChildren<Button>();
            if (colliders == null) colliders = GetComponentsInChildren<BoxCollider>();
            Debug.Assert(buttons != null && colliders != null, "Could not get button and boxCollider!");

            _visible = value;
            foreach (Button b in buttons)
                b.enabled = _visible;
            foreach (BoxCollider c in colliders)
                c.enabled = _visible;
        }
    }

    // Private vars
    Button[] buttons;
    BoxCollider[] colliders;

    public void OnEnable() {
        Visible = true;
    }
}
