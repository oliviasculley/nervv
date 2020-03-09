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

        /// <summary>
        /// Called when the machine should stop moving for any reason.
        /// Use TriggerSafety() to trigger instead!
        /// </summary>
        public EventHandler<SafetyEventArgs> OnSafetyTriggered { get; set; }
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

        public void TriggerSafety(object sender, SafetyEventArgs args = null) {
            LogError($"{Name} has triggered safety!");
            OnSafetyTriggered?.Invoke(sender, args);
        }
        #endregion

        #region Methods
        protected void Log(string s) { if (PrintDebugMessages) Debug.Log($"<b>[{GetType()}]</b> " + s); }
        protected void LogWarning(string s) { if (PrintDebugMessages) Debug.LogWarning($"<b>[{GetType()}]</b> " + s); }
        protected void LogError(string s) { if (PrintDebugMessages) Debug.LogError($"<b>[{GetType()}]</b> " + s); }
        #endregion

        #region EventArg Class
        public class SafetyEventArgs : EventArgs {
            public CollisionReporter Reporter;
            public SafetyEventArgs(CollisionReporter reporter) => Reporter = reporter;
        }
        #endregion

        #region Axis Class
        /// <summary>Axis class, controls robot's dimensions of movement.</summary>
        [Serializable]
        public class Axis {
            #region Properties
            [SerializeField, Tooltip("Value of axis in external worldspace"),
            Header("Properties")]
            protected float _externalValue;
            /// <summary>Value of axis in external worldspace</summary>
            public virtual float ExternalValue {
                get => _externalValue;
                set {
                    _externalValue = ValueRestricted ?
                        Mathf.Clamp(
                            value,
                            MinExternalValue,
                            MaxExternalValue) :
                        value;

                    // Trigger callback if present
                    OnValueUpdated?.Invoke(this, null);
                }
            }

            /// <summary>Callback when ExternalValue is modified</summary>
            public EventHandler OnValueUpdated;

            /// <summary>Value of axis in Unity worldspace</summary>
            public virtual float Value {
                get {
                    // Get value with offset and external value
                    float value = (_externalValue + Offset) * ScaleFactor;

                    // If rotary, keep between 0 and 360
                    if (Type == AxisType.Rotary)
                        value = Mathf.Repeat(value, 360);

                    return value;
                }
                set => _externalValue += (Value - value) / ScaleFactor;
            }

            [SerializeField,
            Tooltip("Saved torque value. Physics are not simulated using this value")]
            protected float _torque;
            /// <summary>
            /// Saved torque value. Physics are not simulated using this value
            /// </summary>
            public virtual float Torque {
                get => _torque;
                set => _torque = value;
            }
            #endregion

            #region Settings
            [SerializeField,
            Tooltip("ID of axis, used for pattern matching and search"),
            Header("Settings")]
            protected string _id;
            /// <summary>ID of axis, used for pattern matching and search</summary>
            public virtual string ID {
                get => _id;
                set => _id = value;
            }

            [SerializeField, Tooltip("Human-readable name of axis")]
            protected string _name;
            /// <summary>
            /// Human-readable name of axis. Should only be used for informative purposes.
            /// </summary>
            public virtual string Name {
                get => _name;
                set => _name = value;
            }

            [SerializeField, Tooltip("Short description of axis")]
            protected string _desc;
            /// <summary>Short description of axis</summary>
            public virtual string Description {
                get => _desc;
                set => _desc = value;
            }

            [SerializeField, Tooltip("Is value restricted by min and max?")]
            protected bool _valueRestricted = false;
            public virtual bool ValueRestricted {
                get => _valueRestricted;
                set => _valueRestricted = value;
            }

            [SerializeField,
            Tooltip("Maximum allowed deviation. External angles" +
                " will at most be clamped to this value.")]
            protected float _maxExternalValue;
            /// <summary>
            /// Maximum allowed deviation. External angles
            /// will at most be clamped by this value.
            /// </summary>
            public virtual float MaxExternalValue {
                get => _maxExternalValue;
                set => _maxExternalValue = value;
            }

            [SerializeField,
            Tooltip("Maximum allowed deviation. External angles" +
                " will at least be clamped by this value.")]
            protected float _minExternalValue;
            /// <summary>
            /// Maximum allowed deviation. External angles
            /// will at least be clamped by this value.
            /// </summary>
            public virtual float MinExternalValue {
                get => _minExternalValue;
                set => _minExternalValue = value;
            }

            [SerializeField,
            Tooltip("Offset used to correct from an external" +
                " worldspace to Unity's worldspace")]
            protected float _offset;
            /// <summary>
            /// Offset used to correct from an external
            /// worldspace to Unity's worldspace
            /// </summary>
            public virtual float Offset {
                get => _offset;
                set => _offset = value;
            }

            [SerializeField,
            Tooltip("Scale used to correct from an external" +
                " worldspace to Unity's worldspace")]
            protected float _scaleFactor = 1f;
            /// <summary>
            /// Scale used to correct from an external
            /// worldspace to Unity's worldspace
            /// </summary>
            public virtual float ScaleFactor {
                get => _scaleFactor;
                set => _scaleFactor = value;
            }

            public enum AxisType { Rotary, Linear, None }
            [SerializeField, Tooltip("Type of axis")]
            protected AxisType _type;
            /// <summary>Type of axis</summary>
            public virtual AxisType Type {
                get => _type;
                set => _type = value;
            }

            [SerializeField,
            Tooltip("Vector3 Location/Rotation in Unity worldspace." +
                " Scales with Value")]
            protected Vector3 _axisVector3;
            /// <summary>
            /// Vector3 Location/Rotation in Unity worldspace. Scales with Value.
            /// Use this to set position/rotation of objects from the axis.
            /// </summary>
            public virtual Vector3 AxisVector3 {
                get => _axisVector3 * Value;
                set => _axisVector3 = value.normalized;
            }

            [SerializeField,
            Tooltip("Ref to transform in scene")]
            protected Transform _axisTransform;
            /// <summary>Ref to transform in scene</summary>
            public virtual Transform AxisTransform {
                get => _axisTransform;
                set => _axisTransform = value;
            }

            [SerializeField,
            Tooltip("If nested, ID of each child axis")]
            protected string[] _childAxesID;
            /// <summary>If nested, ID of each child axis</summary>
            public virtual string[] ChildAxisID {
                get => _childAxesID;
                set => _childAxesID = value;
            }
            #endregion
        }
        #endregion
    }
}