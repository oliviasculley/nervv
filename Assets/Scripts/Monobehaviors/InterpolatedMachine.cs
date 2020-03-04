using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NERVV {
    /// <summary>Example of a machine that implements interpolation</summary>
    /// <see cref="IInterpolation"/>
    public class InterpolatedMachine : BaseMachine, IInterpolation {
        #region Interpolation Settings
        [SerializeField,
        Tooltip("Speed to lerp to correct position"),
        Header("Interpolation Settings")]
        protected float _lerpSpeed = 10f;
        /// <summary>Speed to lerp to correct position</summary>
        ///<seealso cref="IInterpolation"/>
        public float BlendSpeed {
            get => _lerpSpeed;
            set => _lerpSpeed = value;
        }

        [SerializeField,
        Tooltip("Toggles lerping to final position")]
        protected bool _interpolation = true;
        /// <summary>Toggles lerping to final position</summary>
        ///<seealso cref="IInterpolation"/>
        public bool Interpolation {
            get => _interpolation;
            set => _interpolation = value;
        }
        #endregion

        #region Unity Methods
        protected override void OnEnable() {
            base.OnEnable();
            if (PrintDebugMessages && BlendSpeed == 0)
                Debug.LogWarning("BlendSpeed is 0, will never move!");
        }
        #endregion
    }
}

