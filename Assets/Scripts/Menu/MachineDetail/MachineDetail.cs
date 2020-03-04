// System
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

// Unity Engine
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using TMPro;

namespace NERVV.Menu.MachineDetailPanel {
    public class MachineDetail : MenuPanel {
        #region Properties
        [Header("Properties")]
        protected IMachine _currMachine;
        /// <summary>New machine to display</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown if new value is null
        /// </exception>
        public IMachine CurrMachine {
            get => _currMachine;
            set {
                if (_currMachine != null) {
                    _currMachine.OnMachineUpdated -= ResetMachine;

                    foreach (var s in stringElements)
                        Destroy(s.gameObject);
                    foreach (var f in floatElements)
                        Destroy(f.gameObject);
                    foreach (var a in axisElements)
                        Destroy(a.gameObject);

                    // Disable old components
                    DisableAxisHandlers();
                    LeftIKSphere.SetActive(false);
                    RightIKSphere.SetActive(false);
                }

                _currMachine = value ?? throw new ArgumentNullException();
                _currMachine.OnMachineUpdated += ResetMachine;

                // Generate string fields
                if (GenerateStringElements) {
                    var fields = new string[] {
                        nameof(_currMachine.Name),
                        nameof(_currMachine.UUID),
                        nameof(_currMachine.Model),
                        nameof(_currMachine.Manufacturer)
                    };
                    foreach (var str in fields) {
                        var prop = typeof(IMachine).GetProperty(str);
                        Debug.Assert(prop != null);
                        GenerateStringElement(prop);
                    }
                }
                
                // Generate float fields
                if (GenerateFloatElements) {
                    if (_currMachine is IInterpolation IntMachine) {
                        var fields = new string[] { nameof(IntMachine.BlendSpeed) };
                        foreach (var str in fields) {
                            var prop = typeof(IInterpolation).GetProperty(str);
                            Debug.Assert(prop != null);
                            GenerateFloatElement(prop);
                        }
                    }

                    if (_currMachine is IInverseKinematics IKMachine) {
                        var fields = new string[] {
                            nameof(IKMachine.IKSpeed),
                            nameof(IKMachine.IKEpsilonDistance),
                            nameof(IKMachine.IKSamplingDistance)
                        };
                        foreach (var str in fields) {
                            var prop = typeof(IInverseKinematics).GetProperty(str);
                            Debug.Assert(prop != null);
                            GenerateFloatElement(prop);
                        }
                    }
                }

                // Generate angles
                if (GenerateAxisElements)
                    foreach (Machine.Axis a in CurrMachine.Axes)
                        GenerateAxisElement(a);

                // Generate circular axis handle gameObjects
                if (GenerateAxisHandlers) {
                    DisableAxisHandlers();
                    foreach (Machine.Axis a in CurrMachine.Axes)
                        if (a.Type == Machine.Axis.AxisType.Rotary)
                            GenerateAxisHandle(a);
                }
            }
        }

        /// <summary>List of angle controllers generated for new machine</summary>
        public List<AxisHandler> AxisHandlers = new List<AxisHandler>();
        #endregion

        #region Settings
        [Header("Settings")]
        public SteamVR_Action_Boolean ActivateLeftIK;
        public SteamVR_Action_Boolean ActivateRightIK;
        public SteamVR_Action_Boolean InteractUI;

        /// <summary>Generates string fields</summary>
        [Tooltip("Generates string fields")]
        public bool GenerateStringElements = true;

        /// <summary>Generates float fields</summary>
        [Tooltip("Generates float fields")]
        public bool GenerateFloatElements = true;

        /// <summary>Generates axis elements</summary>
        [Tooltip("Generates axis elements")]
        public bool GenerateAxisElements = true;

        /// <summary>Generates circular axis interaction objects</summary>
        [Tooltip("Generates circular axis interaction objects")]
        public bool GenerateAxisHandlers = true;
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

        #region Vars
        protected List<MachineStringElement> stringElements = new List<MachineStringElement>();
        protected List<MachineFloatElement> floatElements = new List<MachineFloatElement>();
        protected List<MachineAxisElement> axisElements = new List<MachineAxisElement>();
        #endregion

        #region Unity Methods
        /// <summary>Check references and safety checks</summary>
        protected override void OnEnable() {
            // Get references
            if (machineTitle == null)                   throw new ArgumentNullException();
            if (machineElementStringPrefab == null)     throw new ArgumentNullException();
            if (machineElementFloatPrefab == null)      throw new ArgumentNullException();
            if (machineElementAxisPrefab == null)       throw new ArgumentNullException();
            if (machineElementParent == null)           throw new ArgumentNullException();
            if (RightIKSphere == null)                  throw new ArgumentNullException();
            if (RightIKSphere == null)                  throw new ArgumentNullException();
            if (AxisHandlerPrefab == null)              throw new ArgumentNullException();
            if (InteractUI == null)                     throw new ArgumentNullException();

            if (PrintDebugMessages) Debug.Log("OnEnable() run!");
            Debug.Assert(AxisHandlers != null);

            base.OnEnable();
        }

        /// <summary>Check and perform IK on current machine</summary>
        protected void Update() {
            // Perform IK on current menu machine if activated
            if (CurrMachine?.GetType() == typeof(IInverseKinematics)) {   
                if (ActivateLeftIK.state) { 
                    ((IInverseKinematics)CurrMachine).InverseKinematics(
                        LeftIKSphere.transform.position,
                        LeftIKSphere.transform.rotation);
                } else if (ActivateRightIK.state) {
                    ((IInverseKinematics)CurrMachine).InverseKinematics(
                        RightIKSphere.transform.position,
                        RightIKSphere.transform.rotation);
                }

                // Set sphere visualizer visibility
                LeftIKSphere.SetActive(ActivateLeftIK.state);
                RightIKSphere.SetActive(ActivateRightIK.state);
            }
            
            // If menu panel is not active, disable axis handlers
            if (AxisHandlers.Count > 0 && gameObject.activeSelf == false)
                DisableAxisHandlers();
        }

        /// <summary>Disables axis handlers</summary>
        protected override void OnDisable() {
            DisableAxisHandlers();
            foreach (var s in stringElements)
                Destroy(s.gameObject);
            foreach (var f in floatElements)
                Destroy(f.gameObject);
            foreach (var a in axisElements)
                Destroy(a.gameObject);
            LeftIKSphere.SetActive(false);
            RightIKSphere.SetActive(false);

            base.OnDisable();
        }
        #endregion

        #region UI Element Generators
        /// <summary>Regenerates current machine on value changes</summary>
        /// <param name="sender">Unused</param>
        /// <param name="args">Unused</param>
        protected void ResetMachine(object sender, EventArgs args) => CurrMachine = CurrMachine;

        /// <summary>
        /// Generates handler that allows for modification of the corresponding axis field
        /// </summary>
        /// <param name="axisName">axis to create handler for</param>
        /// <see cref="MachineAxisElement"/>
        protected void GenerateAxisElement(Machine.Axis a) {
            GameObject g = Instantiate(machineElementAxisPrefab, machineElementParent);
            g.transform.SetAsLastSibling();

            MachineAxisElement e = g.GetComponentInChildren<MachineAxisElement>();
            Debug.Assert(e != null);

            e.InitializeElement(a); // Set fields
            axisElements.Add(e);
        }

        /// <summary>
        /// Generates handler that allows for modification of the corresponding float field
        /// </summary>
        /// <param name="fieldName">Name of field to modify</param>
        /// <see cref="MachineFloatElement"/>
        protected void GenerateFloatElement(PropertyInfo propertyInfo) {
            GameObject g = Instantiate(machineElementFloatPrefab, machineElementParent);
            g.transform.SetAsLastSibling();

            MachineFloatElement e = g.GetComponentInChildren<MachineFloatElement>();
            Debug.Assert(e != null);

            e.InitializeElement(propertyInfo, CurrMachine);
            g.SetActive(true);
            floatElements.Add(e);
        }

        /// <summary>
        /// Generates handler that allows for modification of the corresponding string field 
        /// </summary>
        /// <param name="fieldName">Name of field to modify</param>
        /// <exception cref="FieldAccessException">Could not get fieldInfo for field</exception>
        /// <see cref="MachineStringElement"/>
        protected void GenerateStringElement(PropertyInfo propertyInfo) {
            GameObject g = Instantiate(machineElementStringPrefab, machineElementParent);
            g.transform.SetAsLastSibling();

            MachineStringElement e = g.GetComponentInChildren<MachineStringElement>();
            Debug.Assert(e != null);

            e.InitializeElement(propertyInfo, CurrMachine);
            g.SetActive(true);
            stringElements.Add(e);
        }

        /// <summary>
        /// Tries to find a AxisHandler around machine to allow direct axis rotation,
        /// or if it can't, then generates a new one
        /// </summary>
        /// <param name="a">Axis to generate display for</param>
        /// <see cref="AxisHandler"/>
        protected void GenerateAxisHandle(Machine.Axis a) {
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

            handler.GrabAction = InteractUI;
            handler.Axis = a;

            AxisHandlers.Add(handler);
            if (PrintDebugMessages) Debug.Log("Add Count: " + AxisHandlers.Count);
        }
        #endregion

        #region AxisHandler Methods
        /// <summary>Disables all axis handlers and clears AxisHandlers list</summary>
        /// <see cref="AxisHandler"/>
        public void DisableAxisHandlers() {
            if (PrintDebugMessages) Debug.Log("Disable Count: " + AxisHandlers.Count);
            if (AxisHandlers.Count == 0) return;

            if (PrintDebugMessages) Debug.Log("Disabling handlers...");
            foreach (AxisHandler h in AxisHandlers)
                h.gameObject.SetActive(false);
            AxisHandlers.Clear();
        }
        #endregion
    }
}