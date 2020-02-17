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
        protected Machine.Axis _axis;
        public Machine.Axis Axis {
            get => _axis;
            set => _axis = value ?? throw new ArgumentNullException();
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
        /// <summary>Checks references and adds axis value callback</summary>
        protected override void OnEnable() {
            if (ElementTitle == null) throw new ArgumentNullException();

            if (Axis == null) gameObject.SetActive(false);
            Axis.OnValueUpdated += UpdateAxisText;

            base.OnEnable();
        }

        /// <summary>Set axis value depending on direction</summary>
        protected void Update() {
            if (changingAxis)
                Axis.Value += (axisDirection ? Time.deltaTime : -Time.deltaTime) * AxisSpeed;
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

        public void InitializeElement(Machine.Axis Axis) {
            this.Axis = Axis;
            gameObject.SetActive(true);
        }

        /// <summary>Function to update axis text element values</summary>
        /// <param name="sender">Unused</param>
        /// <param name="args">Unused</param>
        public void UpdateAxisText(object sender, EventArgs args) {
            ElementTitle.text =
                Axis.Name + ": " + Axis.Value + "\n" +  // Axis 1: <value>
                "Torque: " + Axis.Torque;               // Torque: <value>
        }
        #endregion
    }
}