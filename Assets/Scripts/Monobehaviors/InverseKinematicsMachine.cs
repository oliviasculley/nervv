// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>Example of a machine implementing inverse kinematics</summary>
    /// <see cref="IInverseKinematics"/>
    public class InverseKinematicsMachine : BaseMachine, IInverseKinematics {
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
        protected Machine.Axis _startingAxis;
        /// <summary>Base axis to start IK from</summary>
        ///<seealso cref="IInverseKinematics"/>
        public Machine.Axis StartingAxis {
            get => _startingAxis;
            set => _startingAxis = value;
        }
        #endregion

        #region Unity Methods
        protected override void OnEnable() {
            base.OnEnable();
            if (PrintDebugMessages && IKSpeed == 0)
                Debug.LogWarning("IK Learning rate is zero, IK will not move!");
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the final location of the robotic arm using forward kinematics
        /// </summary>
        /// <param name="axes">Array of floats with axis values to calculate</param>
        /// <returns>Vector3 of final position in world space</returns>
        public virtual Vector3 ForwardKinematics(Machine.Axis[] axes) {
            if (axes.Length == 0) return Vector3.zero;

            Vector3 prevPoint = axes[0].AxisTransform.position;
            Quaternion rotation = transform.rotation;

            Vector3 nextPoint;
            for (int i = 0; i < axes.Length - 1; i++) {
                if (axes[i].Type == Machine.Axis.AxisType.Rotary) {
                    // Rotary axes
                    rotation *= Quaternion.AngleAxis(Mathf.Repeat(axes[i].Value, 360), Axes[i].AxisVector3);
                    Debug.DrawRay(prevPoint, rotation * axes[i + 1].AxisTransform.localPosition, Color.red);
                    nextPoint = prevPoint + (rotation * axes[i + 1].AxisTransform.localPosition);
                } else if (axes[i].Type == Machine.Axis.AxisType.Linear) {
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
        float PartialGradient(Vector3 target, List<Machine.Axis> axes, int axisID) {
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
    }
}

