// Unity Engine
using UnityEngine;

namespace MTConnectVR {
    /// <summary>
    /// The base machine interface that must be implemented to
    /// be considered as a machine.
    /// </summary>
    public interface IInverseKinematics {

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
