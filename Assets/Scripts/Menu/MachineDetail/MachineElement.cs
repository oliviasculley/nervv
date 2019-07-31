using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MachineElement : MonoBehaviour {

    #region Static

    /// <summary>Returns string with capitalized first letter</summary>
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

    #endregion

    #region Element Properties

    [Header("Element Properties")]
    public bool _visible;
    public bool Visible {
        get { return _visible; }
        set {
            if (buttons == null) buttons = GetComponentsInChildren<Button>();
            if (colliders == null) colliders = GetComponentsInChildren<BoxCollider>();
            if (triggers == null) triggers = GetComponentsInChildren<EventTrigger>();
            Debug.Assert(
                buttons != null && colliders != null && triggers != null,
                "Could not get button and boxCollider!");

            _visible = value;
            foreach (Button b in buttons)
                b.enabled = _visible;
            foreach (BoxCollider c in colliders)
                c.enabled = _visible;
            foreach (EventTrigger t in triggers)
                t.enabled = _visible;
        }
    }

    #endregion

    #region Private vars

    Button[] buttons;
    BoxCollider[] colliders;
    EventTrigger[] triggers;

    #endregion

    #region Unity Methods

    public void OnEnable() {
        Visible = true;
    }

    #endregion
}
