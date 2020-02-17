// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>Implementation of a machine with all features</summary>
    /// <see cref="BaseMachine"/>
    /// <seealso cref="IInterpolation"/>
    /// <seealso cref="IInverseKinematics"/>
    public class Machine : BaseMachine, IInterpolation, IInverseKinematics {
        #region Interpolation Settings
        [SerializeField,
        Tooltip("Speed to blend to correct position"),
        Header("Interpolation Settings")]
        protected float _blendSpeed = 10f;
        /// <summary>Speed to blend to correct position</summary>
        ///<seealso cref="IInterpolation"/>
        public float BlendSpeed {
            get => _blendSpeed;
            set => _blendSpeed = value;
        }

        [SerializeField,
        Tooltip("Toggles lerping to final position")]
        protected bool _interpolation = true;
        /// <summary>Toggles lerping to final position</summary>
        ///<seealso cref="IInterpolation"/>
        public bool Interpolation {
            get => _interpolation;
            set => _interpolation = value;
        }
        #endregion

        #region IK Settings
        /// <summary>Learning rate of gradient descent</summary>
        [SerializeField, Tooltip("Learning rate of gradient descent"), Header("IK Settings")]
        protected float _ikSpeed = 10f;
        public float IKSpeed {
            get => _ikSpeed;
            set => _ikSpeed = value;
        }


        /// <summary>Axis delta to check IK</summary>
        [SerializeField, Tooltip("Axis delta to check IK")]
        protected float _samplingDistance = 0.01f;
        public float SamplingDistance {
            get => _samplingDistance;
            set => _samplingDistance = value;
        }

        /// <summary>Minimum distance delta to apply IK</summary>
        [SerializeField, Tooltip("Minimum distance delta to apply IK")]
        protected float _ikEpsilon = 0.0001f;
        public float IKEpsilon {
            get => _ikEpsilon;
            set => _ikEpsilon = value;
        }

        [SerializeField, Tooltip("Base axis to start IK from")]
        protected string _startingAxisID;
        /// <summary>Base axis to start IK from</summary>
        ///<seealso cref="IInverseKinematics"/>
        public Axis StartingAxis {
            get => Axes.Find(x => x.ID == _startingAxisID);
            set => _startingAxisID = value.ID;
        }
        #endregion

        #region Vars
        protected List<Axis> ForwardKinematicAxes;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Runs initial safety checks and adds self to MachineManager
        /// </summary>
        protected override void OnEnable() {
            base.OnEnable();

            if (PrintDebugMessages) {
                if (IKSpeed == 0)
                    Debug.LogWarning("IK Learning rate is zero, IK will not move!");
                if (BlendSpeed == 0)
                    Debug.LogWarning("BlendSpeed is 0, will never move!");
            }

            // Axes safety checks
            HashSet<string> seenIDs = new HashSet<string>();
            foreach (var a in Axes) {
                if (a.AxisTransform == null)
                    throw new ArgumentNullException($"Axis transform null for axis {a.ID}!");
                if (seenIDs.Contains(a.ID))
                    throw new ArgumentException($"Axis {a.ID} has identical axis ID! These must be unique.");
                seenIDs.Add(a.ID);
                foreach (var id in a.ChildAxisID)
                    if (Axes.FindIndex(x => x.ID == id) == -1)
                        throw new KeyNotFoundException($"Could not find childAxis with ID {id}!");
            }

            // Setup forward kinematic axes
            ForwardKinematicAxes = ForwardKinematicAxes ?? new List<Axis>();
            ForwardKinematicAxes.Clear();

            var currAxis = StartingAxis;
            ForwardKinematicAxes.Add(currAxis);
            while (currAxis.ChildAxisID != null && currAxis.ChildAxisID.Length > 0) {
                if (currAxis.ChildAxisID.Length != 1)
                    throw new NotImplementedException();

                var nextAxis = Axes.Find(x => x.ID == currAxis.ChildAxisID[0]);
                Debug.Assert(nextAxis != null);
                ForwardKinematicAxes.Add(nextAxis);
                currAxis = nextAxis;
            }
        }

        protected virtual void Update() {
            if (Interpolation) {
                // Continually lerp towards final position
                for (int i = 0; i < Axes.Count; i++)
                    Axes[i].AxisTransform.localRotation = Quaternion.Lerp(
                        Axes[i].AxisTransform.localRotation,
                        Quaternion.Euler(Axes[i].AxisVector3),
                        Mathf.Clamp(BlendSpeed * Time.deltaTime, 0, 1));
            } else {
                // Get latest correct axis angle
                for (int i = 0; i < Axes.Count; i++)
                    Axes[i].AxisTransform.localEulerAngles = Axes[i].AxisVector3;
            }

            // DEBUG: Draw forward kinematics every frame
            ForwardKinematics(ForwardKinematicAxes.ToArray());
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the final location of the robotic arm using forward kinematics
        /// </summary>
        /// <param name="axes">Array of floats with axis values to calculate</param>
        /// <returns>Vector3 of final position in world space</returns>
        public virtual Vector3 ForwardKinematics(Axis[] axes) {
            if (axes.Length == 0) return Vector3.zero;

            Vector3 prevPoint = axes[0].AxisTransform.position;
            Quaternion rotation = transform.rotation;
            Vector3 nextPoint;
            for (int i = 0; i < axes.Length - 1; i++) {
                if (axes[i].Type == Axis.AxisType.Rotary) {
                    // Rotary axes
                    rotation *= Quaternion.AngleAxis(Mathf.Repeat(axes[i].Value, 360), Axes[i].AxisVector3);
                    Debug.DrawRay(prevPoint, rotation * axes[i + 1].AxisTransform.localPosition, Color.red);
                    nextPoint = prevPoint + (rotation * axes[i + 1].AxisTransform.localPosition);
                } else if (axes[i].Type == Axis.AxisType.Linear) {
                    // Linear Axes
                    Debug.DrawRay(prevPoint, Axes[i].AxisVector3 * Axes[i].Value, Color.red);
                    if (PrintDebugMessages) Debug.Log(Axes[i].AxisVector3 * Axes[i].Value);
                    nextPoint = prevPoint + (Axes[i].AxisVector3 * Axes[i].Value);
                } else {
                    // Invalid axis type
                    if (PrintDebugMessages)
                        Debug.LogError("Invalid axis type, cannot perform forward kinematics on this axis! Skipping...");
                    nextPoint = prevPoint;
                }
                prevPoint = nextPoint;
            }
            return prevPoint;
        }

        /// <summary>
        /// When called, performs IK toward the target position by IKSpeed
        /// </summary>
        /// <param name="target">Vector3 target position in worldspace</param>
        /// <see cref="IInverseKinematics"/>
        public virtual void InverseKinematics(Vector3 target) {
            // If close enough to the target, don't need to IK anymore
            if (Vector3.SqrMagnitude(target - ForwardKinematics(ForwardKinematicAxes.ToArray())) < IKEpsilon)
                return;

            // Run linear IK with each angle
            float delta;
            for (int i = 0; i < Axes.Count; i++) {
                delta = ((PartialGradient(target, ForwardKinematicAxes, i) > 0) ? IKSpeed : -IKSpeed) * Time.deltaTime;
                Axes[i].Value -= delta;
            }
        }
        #endregion

        #region Methods
        /// <summary>Returns the gradient for a specific angleID</summary>
        /// <param name="target">Vector3 target location in worldspace</param>
        /// <param name="axes">Angles to calculate from</param>
        /// <param name="axisID">Angle to return gradient for</param>
        /// <returns>Partial gradient as float</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when axisID is out of range</exception>
        /// <see cref="IInverseKinematics"/>
        float PartialGradient(Vector3 target, List<Axis> axes, int axisID) {
            // Safety checks
            if (axisID < 0 || axisID >= axes.Count)
                throw new ArgumentOutOfRangeException("Invalid axisID: " + axisID);

            // Gradient : [F(x+Time per frame) - F(axisToCalculate)] / h

            float f_x = Vector3.SqrMagnitude(target - ForwardKinematics(axes.ToArray()));
            axes[axisID].Value += SamplingDistance;
            float f_x_plus_d = Vector3.SqrMagnitude(target - ForwardKinematics(axes.ToArray()));
            axes[axisID].Value -= SamplingDistance;

            return (f_x_plus_d - f_x) / SamplingDistance;
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