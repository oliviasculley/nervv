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
        float SamplingDistance { get; set; }

        /// <summary>Minimum distance delta to apply IK</summary>
        float IKEpsilon { get; set; }

        /// <summary>Starting axis used to begin forward kinematics</summary>
        Machine.Axis StartingAxis { get; set; }
        #endregion

        #region Required methods
        /// <summary>
        /// Activates a small delta of inverse kinematics for the target position.
        /// </summary>
        /// <param name="targetPosition">Vector3 of target position in world space</param>
        void InverseKinematics(Vector3 targetPosition);

        /// <summary>
        /// Returns the final location of the robotic arm using forward kinematics
        /// </summary>
        /// <param name="anglesToCalculate">Array of floats with angles to calculate</param>
        /// <returns>Vector3 of final position in world space</returns>
        /// <remarks>
        /// Used to forecast where the machine axis will point with various different axis values
        /// without having to actually move the machine
        /// </remarks>
        Vector3 ForwardKinematics(Machine.Axis[] anglesToCalculate);
        #endregion
    }
}
