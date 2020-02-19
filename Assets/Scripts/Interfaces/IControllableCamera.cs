// System
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>
    /// An interface for a controllable camera attached to a machine
    /// </summary>
    public interface IControllableCamera : ICamera {
        /// <summary>If camera is controllable, adjusts camera</summary>
        /// <param name="deltaX">Delta in degrees to adjust camera in the x axis</param>
        /// <param name="deltaY">Delta in degrees to adjust camera in the y axis</param>
        /// <returns>If camera was able to be adjusted</returns>
        bool AdjustCamera(float deltaX = 0, float deltaY = 0);

        /// <summary>List of axes that can affect the controllable camera</summary>
        List<Machine.Axis> CameraAxes { get; set; }
    }
}
