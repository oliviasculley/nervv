// System
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

// Unity Engine
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NERVV.Menu.MachineDetailPanel {
    /// <summary>Machine element in machine properties for float</summary>
    public class MachineFloatElement : MachineElement {
        #region Properties
        [SerializeField, Header("Properties")]
        protected PropertyInfo _property = null;
        public PropertyInfo Property {
            get => _property;
            set {
                if (value == null) throw new ArgumentNullException();
                if (GetMemberType(value) != typeof(float)) throw new ArgumentException();
                _property = value;
            }
        }

        [SerializeField]
        protected IMachine _currMachine = null;
        public IMachine CurrMachine {
            get => _currMachine;
            set => _currMachine = value ?? throw new ArgumentNullException();
        }
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
        protected override void OnEnable() {
            base.OnEnable();

            if (elementTitle == null) throw new ArgumentNullException("Element title is null!");

            if (Property == null || CurrMachine == null)
                gameObject.SetActive(false);

            UpdateText();
        }
        #endregion

        #region Public Functions
        /// <summary>Initialize float element with needed parameters</summary>
        /// <param name="fieldName"></param>
        /// <param name="currMachine"></param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when currMachine or fieldName is null
        /// </exception>
        public void InitializeElement(PropertyInfo Property, IMachine CurrMachine, float minValue = default, float maxValue = default) {
            this.Property = Property;
            this.CurrMachine = CurrMachine;
            this.minValue = minValue;
            this.maxValue = maxValue;

            gameObject.SetActive(true);
        }

        /// <summary>Increments float value</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when currMachine or fieldName is null
        /// </exception>
        public void Increment() {
            SetField(GetFieldValue() + delta);
            UpdateText();
        }

        /// <summary>Decrements float value</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when currMachine or fieldName is null
        /// </exception>
        public void Decrement() {
            SetField(GetFieldValue() - delta);
            UpdateText();
        }
        #endregion

        #region Methods
        /// <summary>Gets field value with reflection</summary>
        /// <returns>Field value</returns>
        protected float GetFieldValue() => (float)GetMemberValue(Property, CurrMachine);

        /// <summary>Sets field value with reflection</summary>
        /// <param name="value">Field value</param>
        protected void SetField(float value) => SetMemberValue(Property, CurrMachine, value);

        /// <summary>Update text readout with current value</summary>
        /// <remarks>Format: "Name: value"</remarks>
        protected void UpdateText() =>
            elementTitle.text = $"{CapitalizeFirstLetter(Property.Name)}: {GetFieldValue()}";
        #endregion
    }
}