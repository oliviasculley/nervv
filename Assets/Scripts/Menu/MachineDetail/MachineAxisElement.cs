// System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NERVV.Menu {
    public class MachineAxisElement : MachineElement {
        #region Properties
        [Header("Properties")]
        public Machine.Axis Axis;
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
        /// <summary>Check references</summary>
        new void OnEnable() {
            Debug.Assert(ElementTitle != null,
                "Could not get axis element title TMP_UGUI!");
        }

        /// <summary>Update Axis elements</summary>
        void Update() {
            // If updating axis, modify
            if (changingAxis) {
                Debug.Assert(Axis != null,
                    "Invalid axis!");

                // Set axis value depending on direction
                Axis.Value += (axisDirection ? Time.deltaTime : -Time.deltaTime) * AxisSpeed;
            }

            // Live update angles
            UpdateText();
        }
        #endregion

        #region Public Methods
        /// <summary>Initialize float element with needed parameters</summary>
        /// <param name="fieldName"></param>
        /// <param name="currMachine"></param>
        public void InitializeElement(Machine.Axis axis) {
            Debug.Assert(axis != null,
                "Invalid axis!");

            Axis = axis;
            UpdateText();
        }

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
        #endregion

        #region Methods
        /// <summary>Update text readout with current value</summary>
        void UpdateText() {
            Debug.Assert(Axis != null,
                "Invalid axis!");

            // Set text with current value
            ElementTitle.text =
                Axis.Name + ": " + Axis.Value + "\n" +  // Axis 1: <value>
                "Torque: " + Axis.Torque;               // Torque: <value>
        }
        #endregion
    }
}