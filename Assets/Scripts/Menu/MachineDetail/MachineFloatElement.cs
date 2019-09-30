// System
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

// Unity Engine
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NERVV.Menu {
    /// <summary>Machine element in machine properties for float</summary>
    public class MachineFloatElement : MachineElement {
        #region Properties
        [Header("Properties")]
        public string fieldName;
        public IMachine currMachine;
        #endregion

        #region Settings
        /// <summary>Delta to increment or decrement value by</summary>
        [Tooltip("Delta to increment or decrement value by"), Header("Settings")]
        public float delta = 1f;
        public float minValue;
        public float maxValue;
        #endregion

        #region References
        [Header("References")]
        public TextMeshProUGUI elementTitle;
        #endregion

        #region Unity Methods
        /// <summary>Check references</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown if element title is null!
        /// </exception>
        new void OnEnable() {
            if (elementTitle == null)
                throw new ArgumentNullException("Element title is null!");
        }
        #endregion

        #region Public Functions
        /// <summary>Initialize float element with needed parameters</summary>
        /// <param name="fieldName"></param>
        /// <param name="currMachine"></param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when currMachine or fieldName is null
        /// </exception>
        public void InitializeElement(string fieldName, IMachine currMachine, float minValue = default, float maxValue = default) {
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException("fieldName is null!");

            this.fieldName = fieldName;
            this.currMachine = currMachine ?? throw new ArgumentNullException("currMachine is null!");
            this.minValue = minValue;
            this.maxValue = maxValue;

            UpdateText();
        }

        /// <summary>Increments float value</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when currMachine or fieldName is null
        /// </exception>
        public void Increment() {
            if (currMachine == null) throw new ArgumentNullException("currMachine is null!");
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException("fieldName is null!");

            if (GetFieldValue() != null) {
                SetField((float)GetFieldValue() + delta);
                UpdateText();
            }
        }

        /// <summary>Decrements float value</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when currMachine or fieldName is null
        /// </exception>
        public void Decrement() {
            if (currMachine == null) throw new ArgumentNullException("currMachine is null!");
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException("fieldName is null!");

            if (GetFieldValue() != null) {
                SetField((float)GetFieldValue() - delta);
                UpdateText();
            }
        }
        #endregion

        #region Methods
        /// <summary>Gets field value with reflection</summary>
        /// <returns>Field value</returns>
        float? GetFieldValue() {
            FieldInfo info;
            if ((info = typeof(Machine).GetField(
                    fieldName,
                    BindingFlags.NonPublic | BindingFlags.Instance
                )) != null)
                return (float)info.GetValue(currMachine);
            Debug.LogError("Could not get field value: " + fieldName);
            return null;
        }

        /// <summary>Sets field value with reflection</summary>
        /// <param name="value">Field value</param>
        void SetField(float value) {
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

        /// <summary>Update text readout with current value</summary>
        void UpdateText() {
            // Set text with current value
            elementTitle.text = CapitalizeFirstLetter(fieldName.Substring(1)) + ": ";
            if (GetFieldValue() != null)
                elementTitle.text += GetFieldValue().ToString();
        }
        #endregion
    }
}