// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>
    /// The base machine interface that must be implemented to
    /// activate InverseKinematics based on a target position
    /// </summary>
    public interface IInverseKinematics : IMachine {
        #region Required Fields
        /// <summary>Learning rate of gradient descent</summary>
        float IKSpeed { get; set; }

        /// <summary>Axis delta to check IK</summary>
        float IKSamplingDistance { get; set; }

        /// <summary>Minimum distance delta to apply IK in meters</summary>
        float IKEpsilonDistance { get; set; }

        /// <summary>Minimum angle delta to apply IK in degrees</summary>
        float IKEpsilonAngle { get; set; }

        /// <summary>
        /// Preference between distance or orientation for IK to use.
        /// 0 means prefer distance every time, 1 is prefer orientation.
        /// </summary>
        /// <remarks>Should be between 0 and 1!</remarks>
        float IKDistanceOrientationWeight { get; set; }

        /// <summary>Starting axis used to begin forward kinematics</summary>
        Machine.Axis StartingAxis { get; set; }
        #endregion

        #region Required methods
        /// <summary>
        /// Activates a small delta of inverse kinematics for the target position.
        /// </summary>
        /// <param name="targetPosition">Target position in world space</param>
        /// <param name="targetOrientation">Target tip orientation in world space</param>
        void InverseKinematics(Vector3 targetPosition, Quaternion targetOrientation);

        /// <summary>
        /// Returns the final location of the robotic arm using forward kinematics
        /// </summary>
        /// <param name="anglesToCalculate">Array of floats with angles to calculate</param>
        /// <param name="resultPoint">Position of last axis of Machine</param>
        /// <param name="resultOrientation">Orientation of last axis of Machine</param>
        /// <returns>Vector3 of final position in world space</returns>
        /// <remarks>
        /// Used to forecast where the machine axis will point with various different axis values
        /// without having to actually move the machine
        /// </remarks>
        void ForwardKinematics(
            Machine.Axis[] anglesToCalculate,
            out Vector3 resultPoint,
            out Quaternion resultOrientation);
        #endregion
    }
}
