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
    public abstract class Machine : MonoBehaviour, IMachine, IInterpolation, IInverseKinematics {
        #region Machine Properties
        [Header("Machine Properties")]
        public List<Axis> _axes;
        /// <summary>Machine axes of possible movement/rotation</summary>
        /// <see cref="IMachine"/>
        public virtual List<Axis> Axes {
            get { return _axes; }
            set { _axes = value; }
        }
        #endregion

        #region Machine Settings
        [SerializeField,
        Tooltip("Human-readable name of machine"),
        Header("Machine Settings")]
        protected string _name;
        /// <summary>Human readable name of machine</summary>
        /// <see cref="IMachine"/>
        public virtual string Name {
            get { return _name; }
            set { _name = value; }
        }

        [SerializeField,
        Tooltip("Individual ID, used for individual machine identification and matching")]
        protected string _uuid;
        /// <summary>Individual ID, used for individual machine identification and matching</summary>
        /// <see cref="IMachine"/>
        public virtual string UUID {
            get { return _uuid; }
            set { _uuid = value; }
        }

        [SerializeField, Tooltip("Name of machine manufacturer")]
        protected string _manufacturer;
        /// <summary>Name of machine manufacturer</summary>
        /// <see cref="IMachine"/>
        public virtual string Manufacturer {
            get { return _manufacturer; }
            set { _manufacturer = value; }
        }

        [SerializeField, Tooltip("Model of machine")]
        protected string _model;
        /// <summary>Model of machine</summary>
        /// <see cref="IMachine"/>
        public virtual string Model {
            get { return _model; }
            set { _model = value; }
        }

        public bool PrintDebugMessages = false;
        #endregion

        #region Interpolation Settings
        [SerializeField,
        Tooltip("Speed to lerp to correct position"),
        Header("Interpolation Settings")]
        protected float _lerpSpeed = 10f;
        /// <summary>Speed to lerp to correct position</summary>
        ///<seealso cref="IInterpolation"/>
        public float BlendSpeed {
            get { return _lerpSpeed; }
            set { _lerpSpeed = value; }
        }

        [SerializeField,
        Tooltip("Toggles lerping to final position")]
        protected bool _interpolation = true;
        /// <summary>Toggles lerping to final position</summary>
        ///<seealso cref="IInterpolation"/>
        public bool Interpolation {
            get { return _interpolation; }
            set { _interpolation = value; }
        }
        #endregion

        #region IK Settings
        /// <summary>Learning rate of gradient descent</summary>
        [Tooltip("Learning rate of gradient descent"), Header("IK Settings")]
        public float IKSpeed = 10f;
        
        /// <summary>Axis delta to check IK</summary>
        [Tooltip("Axis delta to check IK")]
        public float SamplingDistance = 0.01f;

        /// <summary>Minimum distance delta to apply IK</summary>
        [Tooltip("Minimum distance delta to apply IK")]
        public float IKEpsilon = 0.0001f;
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
                    throw new ArgumentNullException("Could not find Transform for axis " + i + "!");
            if (PrintDebugMessages && IKSpeed == 0)
                Debug.LogWarning("IK Learning rate is zero, IK will not move!");
            if (PrintDebugMessages && BlendSpeed == 0)
                Debug.LogWarning("BlendSpeed is 0, will never move!");

            // Link to MachineManager
            if (MachineManager.Instance == null)
                throw new ArgumentNullException("MachineManager.Instance is null!");
            if (!MachineManager.Instance.AddMachine(this))
                throw new Exception("Could not add self to MachineManager!");
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
            if (Vector3.SqrMagnitude(target - ForwardKinematics(Axes.ToArray())) < IKEpsilon)
                return;

            // Run linear IK with each angle
            float delta;
            for (int i = 0; i < Axes.Count; i++) {
                delta = ((PartialGradient(target, Axes, i) > 0) ? IKSpeed : -IKSpeed) * Time.deltaTime;

                Axes[i].Value -= delta;

                //Axes[i].ExternalValue =
                //    Mathf.Clamp(
                //        Axes[i].ExternalValue - delta,
                //        (-Axes[i].MaxDelta) + (IKSpeed * Time.deltaTime),
                //        Axes[i].MaxDelta - (IKSpeed * Time.deltaTime)
                //    );
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
        [System.Serializable]
        public class Axis {
            #region Properties
            [SerializeField,
            Tooltip("Value of axis in external worldspace"),
            Header("Properties")]protected float _externalValue;
            public virtual float ExternalValue {
                get { return _externalValue; }
                set { _externalValue = value; }
            }

            /// <summary>Value of axis in Unity worldspace</summary>
            public virtual float Value {
                get {
                    // Get value with offset and external value
                    float value = (_externalValue % MaxDelta + Offset) * ScaleFactor;

                    // If rotary, keep between 0 and 360
                    if (Type == AxisType.Rotary)
                        value = Mathf.Repeat(value, 360);

                    return value;
                }
                set { _externalValue += (Value - value) / ScaleFactor; }
            }

            [SerializeField,
            Tooltip("")]
            protected float _torque;
            /// <summary></summary>
            public virtual float Torque {
                get { return _torque; }
                set { _torque = value; }
            }
            #endregion

            #region Settings
            [SerializeField,
            Tooltip("ID of axis, used for pattern matching and search"),
            Header("Settings")]
            protected string _id;
            /// <summary>ID of axis, used for pattern matching and search</summary>
            public virtual string ID {
                get { return _id; }
                set { _id = value; }
            }

            [SerializeField, Tooltip("Human-readable name of axis")]
            protected string _name;
            /// <summary>Human-readable name of axis. Should only be used for informative purposes.</summary>
            public virtual string Name {
                get { return _name; }
                set { _name = value; }
            }

            [SerializeField, Tooltip("Short description of axis")]
            protected string _desc;
            /// <summary>Short description of axis</summary>
            public virtual string Description {
                get { return _desc; }
                set { _desc = value; }
            }

            [SerializeField,
            Tooltip("Maximum allowed deviation. External angles" +
                " will be moduloed by this value.")]
            protected float _maxDelta;
            /// <summary>
            /// Maximum allowed deviation. External angles
            /// will be moduloed by this value.
            /// </summary>
            public virtual float MaxDelta {
                get { return _maxDelta; }
                set { _maxDelta = value; }
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
                get { return _offset; }
                set { _offset = value; }
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
                get { return _scaleFactor; }
                set { _scaleFactor = value; }
            }

            public enum AxisType { Rotary, Linear, None }
            [SerializeField, Tooltip("Type of axis")]
            protected AxisType _type;
            /// <summary>Type of axis</summary>
            public virtual AxisType Type {
                get { return _type; }
                set { _type = value; }
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
                get { return _axisVector3 * Value; }
                set { _axisVector3 = value.normalized; }
            }

            [SerializeField,
            Tooltip("Ref to transform in scene")]
            protected Transform _axisTransform;
            /// <summary>Ref to transform in scene</summary>
            public virtual Transform AxisTransform {
                get { return _axisTransform; }
                set { _axisTransform = value; }
            }
            #endregion
        }
        #endregion
    }
}