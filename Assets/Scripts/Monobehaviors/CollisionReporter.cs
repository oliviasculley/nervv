using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NERVV {
    [RequireComponent(typeof(Collider)), ExecuteAlways]
    public class CollisionReporter : MonoBehaviour {
        #region References
        [Header("Machine")]
        public IMachine Machine;
        #endregion

        #region Unity Methods
        /// <summary>Get references on instantiation</summary>
        protected void Awake() {
            if (Machine == null)
                Machine = GetComponentInParent<IMachine>();
            if (Machine == null)
                throw new ArgumentNullException();
            throw new NotImplementedException();
        }

        protected void OnCollisionEnter(Collision collision) {
        }

        protected void OnCollisionExit(Collision collision) {
        }
        #endregion
    }
}
