// System
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

// Unity Engine
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using TMPro;

namespace NERVV.Menu {
    public class MachineDetail : MonoBehaviour {
        #region Properties
        [Header("Properties")]
        public Machine currMachine;
        #endregion

        #region Settings
        [Header("Settings")]
        public SteamVR_Action_Boolean activateLeftIK;
        public SteamVR_Action_Boolean activateRightIK;

        /// <summary>String fields to generate handlers for</summary>
        [Tooltip("String fields to generate handlers for")]
        public string[] stringFieldNamesToGenerate;

        /// <summary>Float fields to generate handlers for</summary>
        [Tooltip("Float fields to generate handlers for")]
        public string[] floatFieldNamesToGenerate;

        /// <summary>Generates axis elements</summary>
        [Tooltip("Generates axis elements")]
        public bool generateAxisElements = true;
        #endregion

        #region References
        [Header("References")]
        public TextMeshProUGUI machineTitle;

        /// <summary>Parent to spawn machine elements</summary>
        [Tooltip("Parent to spawn machine elements")]
        public Transform machineElementParent;

        /// <summary>Sphere on controller to signal IK</summary>
        [Tooltip("Sphere on controller to signal IK")]
        public GameObject RightIKSphere;

        /// <summary>Sphere on controller to signal IK</summary>
        [Tooltip("Sphere on controller to signal IK")]
        public GameObject LeftIKSphere;
        #endregion

        #region Prefabs
        [Header("Prefabs")]
        public GameObject machineElementStringPrefab;
        public GameObject machineElementFloatPrefab;
        public GameObject machineElementAxisPrefab;
        #endregion

        #region Unity Methods
        /// <summary>Check references and safety checks</summary>
        void OnEnable() {
            // Get references
            Debug.Assert(machineTitle != null);
            Debug.Assert(machineElementStringPrefab != null);
            Debug.Assert(machineElementFloatPrefab != null);
            Debug.Assert(machineElementAxisPrefab != null);
            Debug.Assert(machineElementParent != null);
            Debug.Assert(RightIKSphere != null);
            Debug.Assert(RightIKSphere != null);

            // Safety checks
            foreach (string s in stringFieldNamesToGenerate)
                Debug.Assert(!string.IsNullOrEmpty(s));
        }

        /// <summary>Check and perform IK on current machine</summary>
        void Update() {
            // If activated, perform IK on current menu machine
            if (activateLeftIK.state && currMachine != null) {
                currMachine.InverseKinematics(LeftIKSphere.transform.position);
            } else if (activateRightIK.state && currMachine != null) {
                currMachine.InverseKinematics(RightIKSphere.transform.position);
            }

            // Set sphere visualizer visibility
            LeftIKSphere.SetActive(activateLeftIK.state);
            RightIKSphere.SetActive(activateRightIK.state);
        }
        #endregion

        #region Public methods
        public void DisplayMachine(Machine m) {
            // Safety checks
            if ((currMachine = m) != null && currMachine.GetType() == typeof(Machine)) {
                Debug.LogWarning("[Menu: Machine Detail] Invalid machine! Skipping...");
                return;
            }

            // Delete all previous elements
            foreach (Transform t in machineElementParent)
                Destroy(t.gameObject);

            // Generate string fields
            foreach (string s in stringFieldNamesToGenerate)
                GenerateStringElement(s);

            // Generate float fields
            foreach (string s in floatFieldNamesToGenerate)
                GenerateFloatElement(s);

            // Generate angles
            if (generateAxisElements)
                foreach (Machine.Axis a in currMachine.Axes)
                    GenerateAxisElement(a);
        }
        #endregion

        #region Element Generators
        /// <summary>
        /// Generates handler that allows for modification of the corresponding axis field
        /// </summary>
        /// <param name="axisName">axis to create handler for</param>
        void GenerateAxisElement(Machine.Axis a) {
            GameObject g = Instantiate(machineElementAxisPrefab, machineElementParent);
            g.transform.SetAsLastSibling();

            MachineAxisElement e = g.GetComponent<MachineAxisElement>();
            Debug.Assert(e != null);

            e.InitializeElement(a);
        }

        /// <summary>
        /// Generates handler that allows for modification of the corresponding float field
        /// </summary>
        /// <param name="fieldName">Name of field to modify</param>
        void GenerateFloatElement(string fieldName) {
            GameObject g = Instantiate(machineElementFloatPrefab, machineElementParent);
            g.transform.SetAsLastSibling();

            MachineFloatElement e = g.GetComponent<MachineFloatElement>();
            Debug.Assert(e != null);

            e.InitializeElement(fieldName, currMachine);
        }

        /// <summary>
        /// Generates handler that allows for modification of the corresponding string field 
        /// </summary>
        /// <param name="fieldName">Name of field to modify</param>
        void GenerateStringElement(string fieldName) {
            GameObject g = Instantiate(machineElementStringPrefab, machineElementParent);
            g.transform.SetAsLastSibling();

            MachineStringElement e = g.GetComponent<MachineStringElement>();
            Debug.Assert(e != null);

            e.InitializeElement(fieldName, currMachine);
        }
        #endregion
    }
}