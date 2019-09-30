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

        /// <summary>List of angle controllers generated for new machine</summary>
        public List<AxisHandler> AxisHandlers;
        #endregion

        #region Settings
        [Header("Settings")]
        public SteamVR_Action_Boolean activateLeftIK;
        public SteamVR_Action_Boolean activateRightIK;
        public SteamVR_Action_Boolean InteractUI;

        /// <summary>String fields to generate handlers for</summary>
        [Tooltip("String fields to generate handlers for")]
        public string[] stringFieldNamesToGenerate;

        /// <summary>Float fields to generate handlers for</summary>
        [Tooltip("Float fields to generate handlers for")]
        public string[] floatFieldNamesToGenerate;

        /// <summary>Generates axis elements</summary>
        [Tooltip("Generates axis elements")]
        public bool generateAxisElements = true;

        /// <summary>Generates circular axis interaction objects</summary>
        [Tooltip("Generates circular axis interaction objects")]
        public bool generateAxisHandles = true;
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

        /// <summary>Circular axis handler prefab</summary>
        [Tooltip("Circular axis handler prefab")]
        public GameObject AxisHandlerPrefab;
        #endregion

        #region Prefabs
        [Header("Prefabs")]
        public GameObject machineElementStringPrefab;
        public GameObject machineElementFloatPrefab;
        public GameObject machineElementAxisPrefab;
        #endregion

        #region Unity Methods
        /// <summary>Check references and safety checks</summary>
        protected void OnEnable() {
            // Get references
            Debug.Assert(machineTitle != null);
            Debug.Assert(machineElementStringPrefab != null);
            Debug.Assert(machineElementFloatPrefab != null);
            Debug.Assert(machineElementAxisPrefab != null);
            Debug.Assert(machineElementParent != null);
            Debug.Assert(RightIKSphere != null);
            Debug.Assert(RightIKSphere != null);
            Debug.Assert(AxisHandlerPrefab != null);
            Debug.Assert(InteractUI != null);

            AxisHandlers = new List<AxisHandler>();
            Debug.Assert(AxisHandlers != null);

            // Safety checks
            foreach (string s in stringFieldNamesToGenerate)
                Debug.Assert(!string.IsNullOrEmpty(s));
        }

        /// <summary>Check and perform IK on current machine</summary>
        protected void Update() {
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

        protected void OnDisable() {
            DisableAxisHandlers();
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

            // Generate circular axis handle gameObjects
            if (generateAxisHandles) {
                DisableAxisHandlers();
                foreach (Machine.Axis a in currMachine.Axes)
                    GenerateAxisHandle(a);
            }
        }
        #endregion

        #region UI Element Generators
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

        #region AxisHandler Methods
        /// <summary>Finds JointAxisHandler in scene</summary>
        public void GenerateAxisHandle(Machine.Axis a) {
            Transform t = a.AxisTransform.Find("AxisHandler");
            if (t == null) { // Generate new AxisHandle object from prefab if can't find it
                GameObject g = Instantiate(AxisHandlerPrefab, a.AxisTransform);
                g.name = "AxisHandler";
                g.transform.localPosition = Vector3.zero;
                t = g.transform;
            }
            
            Debug.Assert(t != null);
            var handler = t.GetComponent<AxisHandler>();
            Debug.Assert(handler != null);

            handler.Axis = a;
            handler.InteractUI = InteractUI;

            handler.enabled = true;
            t.gameObject.SetActive(true);

            AxisHandlers.Add(handler);
        }

        /// <summary>Disables all axis handlers and clears AxisHandlers list</summary>
        public void DisableAxisHandlers() {
            foreach (AxisHandler h in AxisHandlers)
                h.gameObject.SetActive(false);
            AxisHandlers.Clear();
        }
        #endregion
    }
}