// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>
    /// Interface that must be implemented in order to become an object.
    /// NObject => Nervv Object
    /// </summary>
    public interface INObject {
        /// <summary>Human readable name of NObject</summary>
        string Name { get; }

        /// <summary>Individual ID, used for individual NObject identification and matching</summary>
        string UUID { get; }

        /// <summary>Transform for object</summary>
        Transform Transform { get; }

        /// <summary>Collider for object</summary>
        Collider Collider { get; }

        /// <summary>Mesh for object</summary>
        Mesh Mesh { get; }
    }
}
