// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>
    /// Interface that must be implemented in order to
    /// enable interpolation.
    /// </summary>
    public interface IObject {
        #region Required Fields
        /// <summary>transform of object</summary>
        Transform transform { get; }

        /// <summary>Toggles lerping to final position</summary>
        bool Interpolation { get; set; }
        #endregion
    }
}
