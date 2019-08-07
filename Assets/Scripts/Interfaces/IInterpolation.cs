// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>
    /// Interface that must be implemented in order to
    /// enable interpolation.
    /// </summary>
    public interface IInterpolation : IMachine {
        #region Required Fields
        /// <summary>Speed to lerp to blend position</summary>
        float BlendSpeed { get; set; }

        /// <summary>Toggles lerping to final position</summary>
        bool Interpolation { get; set; }
        #endregion
    }
}
