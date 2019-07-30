// System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

namespace MTConnectVR {
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

        [Header("Machine Settings")]

        [Tooltip("Name of Machine")]
        [SerializeField] protected string _name;
        /// <summary>Name of Machine</summary>
        /// <see cref="IMachine"/>
        public virtual string Name {
            get { return _name; }
            set { _name = value; }
        }

        [Tooltip("Individual ID, used for individual machine identification and matching")]
        [SerializeField] protected string _uuid;
        /// <summary>Individual ID, used for individual machine identification and matching</summary>
        /// <see cref="IMachine"/>
        public virtual string UUID {
            get { return _uuid; }
            set { _uuid = value; }
        }

        [Tooltip("Name of machine manufacturer")]
        [SerializeField] protected string _manufacturer;
        /// <summary>Name of machine manufacturer</summary>
        /// <see cref="IMachine"/>
        public virtual string Manufacturer {
            get { return _manufacturer; }
            set { _manufacturer = value; }
        }

        [Tooltip("Model of machine")]
        [SerializeField] protected string _model;
        /// <summary>Model of machine</summary>
        /// <see cref="IMachine"/>
        public virtual string Model {
            get { return _model; }
            set { _model = value; }
        }

        #endregion

        #region Interpolation Settings
        [Header("Interpolation Settings")]

        [Tooltip("Speed to lerp to correct position")]
        [SerializeField] protected float _lerpSpeed = 10f;
        /// <summary>Speed to lerp to correct position</summary>
        ///<seealso cref="IInterpolation"/>
        public float LerpSpeed {
            get { return _lerpSpeed; }
            set { _lerpSpeed = value; }
        }

        [Tooltip("Toggles lerping to final position")]
        [SerializeField] protected bool _interpolation = true;
        /// <summary>Toggles lerping to final position</summary>
        ///<seealso cref="IInterpolation"/>
        public bool Interpolation {
            get { return _interpolation; }
            set { _interpolation = value; }
        }

        #endregion

        #region IK Settings
        [Header("IK Settings")]

        /// <summary>Learning rate of gradient descent</summary>
        [Tooltip("Learning rate of gradient descent")]
        public float IKSpeed = 10f;
        
        /// <summary>Axis delta to check IK</summary>
        [Tooltip("Axis delta to check IK")]
        public float SamplingDistance = 0.01f;

        /// <summary>Minimum distance delta to apply IK</summary>
        [Tooltip("Minimum distance delta to apply IK")]
        public float IKEpsilon = 0.0001f;

        #endregion

        #region Protected Vars

        protected Transform[] components;

        #endregion

        #region Unity Methods

        /// <summary>Runs initial safety checks and adds self to MachineManager</summary>
        protected virtual void Start() {
            // Safety checks
            for (int i = 0; i < Axes.Count; i++)
                Debug.Assert(components[i] != null,
                    "Could not find component " + i + "!");
            if (IKSpeed == 0)
                Debug.LogWarning("IK Learning rate is zero, IK will not move!");
            if (LerpSpeed == 0)
                Debug.LogWarning("LerpSpeed is 0, will never move!");

            // Link to MachineManager
            Debug.Assert(MachineManager.Instance != null,
                "Could not get ref to MachineManager!");
            Debug.Assert(MachineManager.Instance.AddMachine(this),
                "Could not add self to MachineManager!");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the final location of the robotic arm using forward kinematics
        /// </summary>
        /// <param name="axes">Array of floats with axis values to calculate</param>
        /// <returns>Vector3 of final position in world space</returns>
        public virtual Vector3 ForwardKinematics(Axis[] axes) {
            if (components.Length == 0)
                return Vector3.zero;

            Vector3 prevPoint = components[0].position;
            Quaternion rotation = transform.rotation;

            Vector3 nextPoint;
            for (int i = 0; i < axes.Length - 1; i++) {
                if (axes[i].Type == Axis.AxisType.Rotary) {
                    // Rotary axes
                    rotation *= Quaternion.AngleAxis(Mathf.Repeat(axes[i].Value, 360), Axes[i].AxisVector3);
                    Debug.DrawRay(prevPoint, rotation * components[i + 1].localPosition, Color.red);
                    nextPoint = prevPoint + (rotation * components[i + 1].localPosition);
                } else if (axes[i].Type == Axis.AxisType.Linear) {
                    // Linear Axes
                    Debug.DrawRay(prevPoint, Axes[i].AxisVector3 * Axes[i].Value, Color.red);
                    Debug.Log(Axes[i].AxisVector3 * Axes[i].Value);
                    nextPoint = prevPoint + (Axes[i].AxisVector3 * Axes[i].Value);
                } else {
                    // Invalid axis type
                    Debug.LogError("Invalid axis type, cannot perform forward kinematics on this axis!");
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

        #region Private Methods

        /// <summary>Returns the gradient for a specific angleID</summary>
        /// <param name="target">Vector3 target location in worldspace</param>
        /// <param name="axes">Angles to calculate from</param>
        /// <param name="axisID">Angle to return gradient for</param>
        /// <returns>Partial gradient as float</returns>
        /// <see cref="IInverseKinematics"/>
        private float PartialGradient(Vector3 target, List<Axis> axes, int axisID) {
            // Safety checks
            Debug.Assert(axisID >= 0 && axisID < axes.Count,
                "Invalid axisID: " + axisID);

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
            [Header("Properties")]

            [Tooltip("Value of axis in external worldspace")]
            [SerializeField] protected float _externalValue;
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

            [SerializeField] protected float _torque;
            /// <summary></summary>
            public virtual float Torque {
                get { return _torque; }
                set { _torque = value; }
            }

            #endregion


            #region Settings
            [Header("Settings")]

            [Tooltip("ID of axis, used for matching")]
            [SerializeField] protected string _id;
            /// <summary>ID of axis, used for matching</summary>
            public virtual string ID {
                get { return _id; }
                set { _id = value; }
            }

            [Tooltip("Name of axis")]
            [SerializeField] protected string _name;
            /// <summary>Name of axis. Should only be used for informative purposes.</summary>
            public virtual string Name {
                get { return _name; }
                set { _name = value; }
            }

            [Tooltip("Short description of axis")]
            [SerializeField] protected string _desc;
            /// <summary>Short description of axis</summary>
            public virtual string Description {
                get { return _desc; }
                set { _desc = value; }
            }

            [Tooltip("Maximum allowed deviation. External angles will be moduloed by this value.")]
            [SerializeField] protected float _maxDelta;
            /// <summary>Maximum allowed deviation. External angles will be moduloed by this value.</summary>
            public virtual float MaxDelta {
                get { return _maxDelta; }
                set { _maxDelta = value; }
            }

            [Tooltip("Offset used to correct from an external worldspace to Unity's worldspace")]
            [SerializeField] protected float _offset;
            /// <summary>Offset used to correct from an external worldspace to Unity's worldspace</summary>
            public virtual float Offset {
                get { return _offset; }
                set { _offset = value; }
            }

            [Tooltip("Scale used to correct from an external worldspace to Unity's worldspace")]
            [SerializeField] protected float _scaleFactor = 1f;
            /// <summary>Scale used to correct from an external worldspace to Unity's worldspace</summary>
            public virtual float ScaleFactor {
                get { return _scaleFactor; }
                set { _scaleFactor = value; }
            }

            public enum AxisType { Rotary, Linear, None }
            [Tooltip("Type of axis")]
            [SerializeField] protected AxisType _type;
            /// <summary>Type of axis</summary>
            public virtual AxisType Type {
                get { return _type; }
                set { _type = value; }
            }

            [Tooltip("Vector3 Location/Rotation in Unity worldspace. Scales with Value")]
            [SerializeField] protected Vector3 _axisVector3;
            /// <summary>
            /// Vector3 Location/Rotation in Unity worldspace. Scales with Value.
            /// Use this to set position/rotation of objects from the axis.
            /// </summary>
            public virtual Vector3 AxisVector3 {
                get { return _axisVector3 * Value; }
                set { _axisVector3 = value.normalized; }
            }

            #endregion
        }

        #endregion
    }
}