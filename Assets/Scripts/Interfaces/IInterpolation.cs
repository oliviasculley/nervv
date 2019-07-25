// Unity Engine
using UnityEngine;

namespace MTConnectVR {
    /// <summary>
    /// Interface that must be implemented in order to
    /// enable interpolation.
    /// </summary>
    public interface IInterpolation {

        #region Required Fields

        ///<summary>Speed to lerp to correct position</summary>
        float LerpSpeed { get; set; }

        ///<summary>Toggles lerping to final position</summary>
        bool Interpolation { get; set; }

        #endregion

        #region Required methods

        /// <summary>
        /// Activate a small delta of inverse kinematics for the target position.
        /// </summary>
        /// <param name="targetPosition">Vector3 of target position in world space</param>
        void InverseKinematics(Vector3 targetPosition);

        #endregion
    }
}
