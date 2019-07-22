using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;

using MTConnectVR;

namespace MTConnectVR.Menu {
    public class MachineFloatElement : MachineElement {
        [Header("Properties")]
        public string fieldName;
        public IMachine currMachine;

        [Header("Settings")]
        [Tooltip("Delta to increment or decrement value by")]
        public float delta = 1f;
        public float minValue, maxValue;

        [Header("References")]
        public TextMeshProUGUI elementTitle;

        private new void OnEnable() {
            Debug.Assert(elementTitle != null,
                "Could not get Float element title TMP_UGUI!");
        }

        #region Public Functions

        /// <summary>
        /// Initialize float element with needed parameters
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="currMachine"></param>
        public void InitializeElement(string fieldName, IMachine currMachine, float minValue = default, float maxValue = default) {
            Debug.Assert(currMachine != null && !string.IsNullOrEmpty(fieldName));

            this.fieldName = fieldName;
            this.currMachine = currMachine;
            this.minValue = minValue;
            this.maxValue = maxValue;

            UpdateText();
        }

        public void Increment() {
            Debug.Assert(currMachine != null && !string.IsNullOrEmpty(fieldName));

            if (GetFieldValue() != null) {
                SetField((float)GetFieldValue() + delta);
                UpdateText();
            }
        }

        public void Decrement() {
            Debug.Assert(currMachine != null && !string.IsNullOrEmpty(fieldName));

            if (GetFieldValue() != null) {
                SetField((float)GetFieldValue() - delta);
                UpdateText();
            }
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Gets field value with reflection
        /// </summary>
        /// <returns>Field value</returns>
        private float? GetFieldValue() {
            System.Reflection.FieldInfo info;
            if ((info = typeof(Machine).GetField(
                    fieldName,
                    BindingFlags.NonPublic | BindingFlags.Instance
                )) != null)
                return (float)info.GetValue(currMachine);
            Debug.LogError("Could not get field value: " + fieldName);
            return null;
        }

        /// <summary>
        /// Sets field value with reflection
        /// </summary>
        /// <param name="value">Field value</param>
        private void SetField(float value) {
            FieldInfo info;
            if (currMachine != null &&
                (info = typeof(Machine).GetField(
                    fieldName,
                    BindingFlags.NonPublic | BindingFlags.Instance)
                ) != null) {
                // Use min/max if values are available
                if (minValue != default || maxValue != default)
                    typeof(Machine).GetField(
                        fieldName,
                        BindingFlags.NonPublic | BindingFlags.Instance
                    ).SetValue(currMachine,
                        Mathf.Clamp(value, minValue, maxValue)
                    );
                else
                    typeof(Machine).GetField(
                        fieldName,
                        BindingFlags.NonPublic | BindingFlags.Instance
                    ).SetValue(currMachine, value);
            } else {
                Debug.LogError("Could not set field value: " + fieldName);
            }
        }

        /// <summary>
        /// Update text readout with current value
        /// </summary>
        private void UpdateText() {
            // Set text with current value
            elementTitle.text = CapitalizeFirstLetter(fieldName.Substring(1)) + ": ";
            if (GetFieldValue() != null)
                elementTitle.text += GetFieldValue().ToString();
        }

        #endregion
    }
}