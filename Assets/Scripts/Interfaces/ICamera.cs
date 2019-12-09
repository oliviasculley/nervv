// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>
    /// An interface for a machine with a camera
    /// </summary>
    public interface ICamera : IMachine {
        #region Required Fields
        /// <summary>Field of view of the camera in degrees</summary>
        Vector2 CameraFOV { get; }
        #endregion
    }
}
