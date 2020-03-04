// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>
    /// Base implementation of a machine. These are automatically
    /// added to MachineManager when they are initialized.
    /// </summary>
    public abstract class BaseMachine : MonoBehaviour, IMachine {
        #region Machine Properties
        [Header("Machine Properties")]
        public List<Machine.Axis> _axes;
        /// <summary>Machine axes of possible movement/rotation</summary>
        /// <see cref="IMachine"/>
        public virtual List<Machine.Axis> Axes {
            get => _axes;
            set => _axes = value;
        }

        /// <summary>
        /// Invoked when any field value is updated.
        /// Changes to axis values will NOT invoke this!
        /// </summary>
        public EventHandler OnMachineUpdated { get; set; }
        #endregion

        #region Machine Settings
        [SerializeField,
        Tooltip("Human-readable name of machine"),
        Header("Machine Settings")]
        protected string _name;
        /// <summary>Human readable name of machine</summary>
        /// <see cref="IMachine"/>
        public virtual string Name {
            get => _name;
            set {
                _name = value;
                TriggerOnMachineUpdated(this);
            }
        }

        [SerializeField,
        Tooltip("Individual ID, used for individual machine identification and matching")]
        protected string _uuid;
        /// <summary>Individual ID, used for individual machine identification and matching</summary>
        /// <see cref="IMachine"/>
        public virtual string UUID {
            get => _uuid;
            set {
                _uuid = value;
                TriggerOnMachineUpdated(this);
            }
        }

        [SerializeField, Tooltip("Name of machine manufacturer")]
        protected string _manufacturer;
        /// <summary>Name of machine manufacturer</summary>
        /// <see cref="IMachine"/>
        public virtual string Manufacturer {
            get => _manufacturer;
            set {
                _manufacturer = value;
                TriggerOnMachineUpdated(this);
            }
        }

        [SerializeField, Tooltip("Model of machine")]
        protected string _model;
        /// <summary>Model of machine</summary>
        /// <see cref="IMachine"/>
        public virtual string Model {
            get => _model;
            set {
                _model = value;
                TriggerOnMachineUpdated(this);
            }
        }

        public bool PrintDebugMessages = false;
        #endregion

        #region Machine References
        [SerializeField,
        Tooltip("If null, will attempt to use global reference"), Header("Machine References")]
        protected MachineManager _machineManager;
        public MachineManager MachineManager {
            get {
                if (_machineManager == null) {
                    if (MachineManager.Instances.Count > 0)
                        _machineManager = MachineManager.Instances[0];
                    else
                        throw new ArgumentNullException("Could not get a ref to a MachineManager!");
                }
                return _machineManager;
            }
            set => _machineManager = value;
        }
        #endregion

        #region Unity Methods
        /// <summary>
        /// Runs initial safety checks and
        /// adds self to MachineManager
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown if Axes.AxisTransform is null</exception>
        /// <exception cref="Exception">
        /// Thrown if could not add self to MachineManager
        /// </exception>
        protected virtual void OnEnable() {
            // Safety checks
            for (int i = 0; i < Axes.Count; i++)
                if (Axes[i].AxisTransform == null)
                    

            foreach (var a in Axes) {
                if (a.AxisTransform == null)
                    throw new ArgumentNullException($"Could not find Transform for {a.ID}!");
                a.OnValueUpdated += TriggerOnMachineUpdated;
            }

            // Link to MachineManager
            if (MachineManager == null)
                throw new ArgumentNullException("MachineManager is null!");
            if (!MachineManager.AddMachine(this))
                throw new Exception("Could not add self to MachineManager!");
        }
        #endregion

        #region Public Methods
        public void TriggerOnMachineUpdated(object sender, EventArgs args = null) =>
            OnMachineUpdated?.Invoke(sender, args);
        #endregion

        #region Methods
        protected void Log(string s) { if (PrintDebugMessages) Debug.Log($"<b>[{GetType()}]</b> " + s); }
        protected void LogWarning(string s) { if (PrintDebugMessages) Debug.LogWarning($"<b>[{GetType()}]</b> " + s); }
        protected void LogError(string s) { if (PrintDebugMessages) Debug.LogError($"<b>[{GetType()}]</b> " + s); }
        #endregion
    }
}