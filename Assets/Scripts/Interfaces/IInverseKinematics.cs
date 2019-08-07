// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>
    /// The base machine interface that must be implemented to
    /// activate InverseKinematics based on a target position
    /// </summary>
    public interface IInverseKinematics : IMachine {
        #region Required methods
        /// <summary>
        /// Activate a small delta of inverse kinematics for the target position.
        /// </summary>
        /// <param name="targetPosition">Vector3 of target position in world space</param>
        void InverseKinematics(Vector3 targetPosition);

        /// <summary>
        /// Returns the final location of the robotic arm using forward kinematics
        /// </summary>
        /// <param name="anglesToCalculate">Array of floats with angles to calculate</param>
        /// <returns>Vector3 of final position in world space</returns>
        Vector3 ForwardKinematics(Machine.Axis[] anglesToCalculate);
        #endregion
    }
}
