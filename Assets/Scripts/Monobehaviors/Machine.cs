// System
using System;
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
        protected float _ikSamplingDistance = 0.01f;
        public float IKSamplingDistance {
            get => _ikSamplingDistance;
            set => _ikSamplingDistance = value;
        }

        /// <summary>Minimum distance delta to apply IK in meters</summary>
        [SerializeField, Tooltip("Minimum distance delta to apply IK")]
        protected float _ikEpsilonDistance = 0.0001f;
        public float IKEpsilonDistance {
            get => _ikEpsilonDistance;
            set => _ikEpsilonDistance = value;
        }

        /// <summary>Minimum angle delta to apply IK in degrees</summary>
        [SerializeField, Tooltip("Minimum angle delta to apply IK")]
        protected float _ikEpsilonAngle = 0.0001f;
        public float IKEpsilonAngle {
            get => _ikEpsilonAngle;
            set => _ikEpsilonAngle = value;
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

        protected virtual void FixedUpdate() {
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

            // DEBUG: Draw forward kinematics lines in scene view
            if (PrintDebugMessages)
                ForwardKinematics(ForwardKinematicAxes.ToArray(), out _, out _);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the final location and orientation of the machine using forward kinematics.
        /// If linear angles, does not modify the resultant vector from the previous angle.
        /// </summary>
        /// <param name="axes">Array of floats with axis values to calculate</param>
        /// <param name="resultPoint">Final position in world space</param>
        /// <param name="resultOrientation">Final orientation in world space</param>
        public virtual void ForwardKinematics(Axis[] axes, out Vector3 resultPoint, out Quaternion resultOrientation) {
            if (axes.Length == 0) throw new ArgumentException();

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

            // Return values
            resultPoint = prevPoint;
            resultOrientation = rotation;
        }

        /// <summary>
        /// When called, performs IK toward the target position by IKSpeed
        /// </summary>
        /// <param name="targetPoint">Vector3 target position in worldspace</param>
        /// <see cref="IInverseKinematics"/>
        public virtual void InverseKinematics(Vector3 targetPoint, Quaternion targetOrientation) {
            // If close enough to the target, don't need to IK anymore
            ForwardKinematics(ForwardKinematicAxes.ToArray(), out Vector3 resultPoint, out Quaternion resultOrientation);
            if (Vector3.SqrMagnitude(targetPoint - resultPoint) < IKEpsilonDistance) return;
            if (Quaternion.Angle(targetOrientation, resultOrientation) < IKEpsilonAngle) return;

            // Run linear IK with each angle
            float delta, gradient;
            for (int i = 0; i < Axes.Count; i++) {
                gradient = PartialGradient(targetPoint, targetOrientation, ForwardKinematicAxes, i);
                delta = ((gradient > 0) ? IKSpeed : -IKSpeed) * Time.deltaTime;
                Axes[i].Value -= delta;
            }
        }
        #endregion

        #region Methods
        /// <summary>Returns the gradient for a specific angleID</summary>
        /// <param name="targetPosition">Target location in worldspace</param>
        /// <param name="targetOrientation">Target orientation in world space</param>
        /// <param name="axes">Angles to calculate from</param>
        /// <param name="axisID">Angle to return gradient for</param>
        /// <param name="weight">Weight from 0 to 1 for position or orientation, respectively</param>
        /// <returns>Partial gradient as float</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when axisID is out of range</exception>
        /// <exception cref="ArgumentException">Thrown when IKSamplingDistance is 0</exception>
        /// <see cref="IInverseKinematics"/>
        protected float PartialGradient(
            Vector3 targetPosition,
            Quaternion targetOrientation,
            List<Axis> axes,
            int axisID,
            float weight = 0.5f) {
            // Safety checks
            if (axisID < 0 || axisID >= axes.Count)
                throw new ArgumentOutOfRangeException("Invalid axisID: " + axisID);
            if (IKSamplingDistance == 0)
                throw new ArgumentException("IKSamplingDistance must not be 0!");

            // Gradient : [F(x+Time per frame) - F(axisToCalculate)] / h

            // Get original values
            ForwardKinematics(axes.ToArray(), out Vector3 originalPoint, out Quaternion originalOrientation);
            var originalDistance = Vector3.SqrMagnitude(targetPosition - originalPoint);
            var originalAngle = Quaternion.Angle(targetOrientation, originalOrientation);

            // Get values modified by a delta
            axes[axisID].Value += IKSamplingDistance;
            ForwardKinematics(axes.ToArray(), out Vector3 modifiedPoint, out Quaternion modifiedOrientation);
            var modifiedDistance = Vector3.SqrMagnitude(targetPosition - modifiedPoint);
            var modifiedAngle = Quaternion.Angle(targetOrientation, modifiedOrientation);
            axes[axisID].Value -= IKSamplingDistance;

            var gradientDistance = modifiedDistance - originalDistance;
            var gradientOrientation = modifiedAngle - originalAngle;
            return Mathf.Lerp(gradientDistance, gradientOrientation, Mathf.Clamp01(weight)) / IKSamplingDistance;
        }
        #endregion
    }
}