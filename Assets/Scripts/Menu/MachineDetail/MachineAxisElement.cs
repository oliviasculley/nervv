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
        protected override void OnEnable() {
            if (ElementTitle == null) throw new ArgumentNullException();

            if (Axis == null) gameObject.SetActive(false);

            base.OnEnable();
        }

        /// <summary>Update text elements</summary>
        protected void Update() {
            // Set axis value depending on direction
            if (changingAxis)
                Axis.Value += (axisDirection ? Time.deltaTime : -Time.deltaTime) * AxisSpeed;

            // Live update angles
            ElementTitle.text =
                Axis.Name + ": " + Axis.Value + "\n" +  // Axis 1: <value>
                "Torque: " + Axis.Torque;               // Torque: <value>
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
        public void StopChanging() {
            changingAxis = false;
        }

        public void InitializeElement(Machine.Axis Axis) {
            this.Axis = Axis;
            gameObject.SetActive(true);
        }
        #endregion
    }
}