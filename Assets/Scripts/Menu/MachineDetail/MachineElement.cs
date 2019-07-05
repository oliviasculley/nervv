using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public bool visible {
        get { return _visible; }
        set {
            _visible = value;
            foreach (Transform t in transform)
                t.gameObject.SetActive(_visible);
        }
    }

    public void OnEnable() {
        visible = true;
    }
}
