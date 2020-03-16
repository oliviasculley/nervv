// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NERVV.Menu.MachineDetailPanel {
    public class MachineAxisElement : MachineElement {
        #region Properties
        [SerializeField, Header("Properties")]
        protected BaseMachine.Axis _axis;
        public BaseMachine.Axis Axis {
            get => _axis;
            set {
                // Remove old axis callback
                if (_axis != null)
                    _axis.OnValueUpdated -= UpdateAxisText;

                // Set new value
                _axis = value ?? throw new ArgumentNullException();

                // Update text and add callback
                UpdateAxisText(this, null);
                _axis.OnValueUpdated += UpdateAxisText;

                gameObject.SetActive(true);
            }
        }
        #endregion

        #region Settings
        [Header("Settings")]
        public float AxisSpeed = 100f;
        #endregion

        #region References
        [Header("References")]
        public TextMeshProUGUI ElementTitle;
        #endregion

        #region Vars
        /// <summary>Is changing axis?</summary>
        bool changingAxis;

        /// <summary>Which direction (incrementing/decrementing)?</summary>
        bool axisDirection;
        #endregion

        #region Unity Methods
        /// <summary>Checks references and disables element if axis is not specified</summary>
        protected override void OnEnable() {
            if (ElementTitle == null) throw new ArgumentNullException();
            if (Axis == null) gameObject.SetActive(false);

            base.OnEnable();
        }

        /// <summary>Set axis value depending on direction</summary>
        protected void FixedUpdate() {
            if (changingAxis)
                Axis.Value += Time.deltaTime * (axisDirection ? 1 : -1) * AxisSpeed;
        }

        /// <summary>Removes axis value callback</summary>
        protected override void OnDisable() {
            Axis.OnValueUpdated -= UpdateAxisText;

            base.OnDisable();
        }
        #endregion

        #region Public Methods
        /// <summary>Starts increasing or decreasing axis value</summary>
        /// <param name="direction">Direction to start changing axis value</param>
        public void StartChanging(bool direction) {
            changingAxis = true;
            axisDirection = direction;
        }

        /// <summary>Stops modifying axis value</summary>
        public void StopChanging() => changingAxis = false;

        /// <summary>Method to initialize eleement</summary>
        public void InitializeElement(BaseMachine.Axis axis) => Axis = axis;

        /// <summary>Function to update axis text element values</summary>
        /// <remarks>Format:
        /// AxisName: value
        /// Torque: value
        /// </remarks>
        /// <param name="sender">Unused</param>
        /// <param name="args">Unused</param>
        public void UpdateAxisText(object sender, EventArgs args) =>
            ElementTitle.text = $"{Axis.Name}: {Axis.Value}\nTorque: {Axis.Torque}";
        #endregion
    }
}