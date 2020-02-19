// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>
    /// Interface that must be implemented in order to become an object
    /// </summary>
    public interface IObject {
        /// <summary>Transform for object</summary>
        Transform Transform { get; }

        /// <summary>Collider for object</summary>
        Collider Collider { get; }

        /// <summary>Mesh for object</summary>
        Mesh Mesh { get; }
    }
}
