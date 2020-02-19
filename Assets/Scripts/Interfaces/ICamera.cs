// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>
    /// An interface for a camera attached to a machine
    /// </summary>
    public interface ICamera {
        /// <summary>Machine camera is attached to. Must not be null!</summary>
        Machine CurrentMachine { get; }

        /// <summary>Field of view of the camera in degrees</summary>
        Vector2 CameraFOV { get; }
        
        /// <summary>Image data encoded as JPG</summary>
        byte[] ImageDataJPG { get; }

        /// <summary>Image data encoded as PNG</summary>
        byte[] ImageDataPNG { get; }
    }
}
