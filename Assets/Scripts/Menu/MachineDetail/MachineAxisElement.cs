using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MachineAxisElement : MachineElement {
    [Header("Properties")]
    public Machine.Axis axis;

    [Header("Settings")]
    public float delta = 1f;

    [Header("References")]
    public TextMeshProUGUI elementTitle;

    private new void OnEnable() {
        Debug.Assert(elementTitle != null,
            "Could not get axis element title TMP_UGUI!");
    }

    private void Update() {
        // Live update angles
        UpdateText();
    }

    /* Public Functions */

    /// <summary>
    /// Initialize float element with needed parameters
    /// </summary>
    /// <param name="fieldName"></param>
    /// <param name="currMachine"></param>
    public void InitializeElement(Machine.Axis axis) {
        Debug.Assert(axis != null,
            "Invalid axis!");

        this.axis = axis;

        UpdateText();
    }

    public void Increment() {
        Debug.Assert(axis != null,
            "Invalid axis!");

        axis.SetValue(axis.GetValue() + delta);
        UpdateText();
    }

    public void Decrement() {
        Debug.Assert(axis != null,
            "Invalid axis!");

        axis.SetValue(axis.GetValue() - delta);
        UpdateText();
    }

    /* Private Functions */

    /// <summary>
    /// Update text readout with current value
    /// </summary>
    private void UpdateText() {
        Debug.Assert(axis != null,
            "Invalid axis!");

        // Set text with current value
        elementTitle.text =
            axis.GetName() +
            ": " +
            axis.GetValue();
    }
}
