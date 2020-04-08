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
        protected Machine.Axis _startingAxis;
        /// <summary>Base axis to start IK from</summary>
        ///<seealso cref="IInverseKinematics"/>
        public Machine.Axis StartingAxis {
            get => _startingAxis;
            set => _startingAxis = value;
        }

        /// <summary>
        /// Preference between distance or orientation for IK to use.
        /// 0 means prefer distance every time, 1 is prefer orientation.
        /// </summary>
        /// <remarks>Should be between 0 and 1!</remarks>
        [SerializeField, Range(0, 1),
        Tooltip("Preference between distance or orientation for IK to use.")]
        protected float _ikDistanceOrientationWeight = 0.05f;
        public float IKDistanceOrientationWeight {
            get => _ikDistanceOrientationWeight;
            set => _ikDistanceOrientationWeight = Mathf.Clamp01(value);
        }
        #endregion

        #region Vars
        protected List<Machine.Axis> ForwardKinematicAxes;
        #endregion

        #region Unity Methods
        /// <summary>Runs initial safety checks and adds self to MachineManager</summary>
        protected override void OnEnable() {
            base.OnEnable();
            if (PrintDebugMessages && IKSpeed == 0)
                Debug.LogWarning("IK Learning rate is zero, IK will not move!");

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
            ForwardKinematicAxes = ForwardKinematicAxes ?? new List<Machine.Axis>();
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
        public virtual void ForwardKinematics(Machine.Axis[] axes, out Vector3 resultPoint, out Quaternion resultOrientation) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When called, performs IK toward the target position by IKSpeed
        /// </summary>
        /// <param name="targetPoint">Vector3 target position in worldspace</param>
        /// <see cref="IInverseKinematics"/>
        public virtual void InverseKinematics(Vector3 targetPoint, Quaternion targetOrientation) {
            throw new NotImplementedException();
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
            List<Machine.Axis> axes,
            int axisID,
            float weight = 0.5f) {
            throw new NotImplementedException();
        }
        #endregion
    }
}
