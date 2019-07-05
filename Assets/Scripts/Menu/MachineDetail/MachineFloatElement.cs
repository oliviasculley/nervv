using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MachineFloatElement : MachineElement
{
    [Header("Properties")]
    public string fieldName;
    public Machine currMachine;

    [Header("Settings")]
    public float delta = 1f;    // Delta to increment or decrement value by
    public float minValue, maxValue;

    [Header("References")]
    public TextMeshProUGUI elementTitle;

    private new void OnEnable() {
        Debug.Assert(elementTitle != null,
            "Could not get Float element title TMP_UGUI!");
    }

    /* Public Functions */

    /// <summary>
    /// Initialize float element with needed parameters
    /// </summary>
    /// <param name="fieldName"></param>
    /// <param name="currMachine"></param>
    public void InitializeElement(string fieldName, Machine currMachine, float minValue = default, float maxValue = default) {
        Debug.Assert(currMachine != null && !string.IsNullOrEmpty(fieldName));

        this.fieldName = fieldName;
        this.currMachine = currMachine;
        this.minValue = minValue;
        this.maxValue = maxValue;

        UpdateText();
    }

    public void Increment() {
        Debug.Assert(currMachine != null && !string.IsNullOrEmpty(fieldName));

        SetField(GetField() + delta);
        UpdateText();
    }

    public void Decrement() {
        Debug.Assert(currMachine != null && !string.IsNullOrEmpty(fieldName));

        SetField(GetField() - delta);
        UpdateText();
    }

    /* Private Functions */

    /// <summary>
    /// Gets field value with reflection
    /// </summary>
    /// <returns>Field value</returns>
    private float GetField() {
        return (float)typeof(Machine).GetField(fieldName).GetValue(currMachine);
    }

    /// <summary>
    /// Sets field value with reflection
    /// </summary>
    /// <param name="value">Field value</param>
    private void SetField(float value) {
        // Use min/max if values are available
        if (minValue != default || maxValue != default)
            typeof(Machine).GetField(fieldName).SetValue(currMachine,
                Mathf.Clamp(value, minValue, maxValue)
            );
        else
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
}
