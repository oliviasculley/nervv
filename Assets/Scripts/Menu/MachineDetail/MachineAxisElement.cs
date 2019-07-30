using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MTConnectVR.Menu {
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

        #region Private vars

        /// <summary>Is changing axis?</summary>
        private bool changingAxis;

        /// <summary>Which direction (incrementing/decrementing)?</summary>
        private bool axisDirection;

        #endregion

        #region Unity Methods

        private new void OnEnable() {
            Debug.Assert(ElementTitle != null, "Could not get axis element title TMP_UGUI!");
        }

        private void Update() {
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

        #region Public Functions

        /// <summary>Initialize float element with needed parameters</summary>
        /// <param name="fieldName"></param>
        /// <param name="currMachine"></param>
        public void InitializeElement(Machine.Axis axis) {
            Debug.Assert(axis != null,
                "Invalid axis!");

            this.Axis = axis;

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

        #region Private Functions

        /// <summary>Update text readout with current value</summary>
        private void UpdateText() {
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