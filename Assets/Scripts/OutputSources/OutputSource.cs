using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MTConnectVR {
    /// <summary>
    /// This is the base class for outputs. These are dynamically added to OutputManager.
    /// </summary>
    public abstract class OutputSource : MonoBehaviour, IOutputSource {

        #region Output Properties

        [SerializeField,
        Tooltip("Is this output currently enabled?"), Header("Output Properties")]
        protected bool _outputEnabled = true;
        /// <summary>Is this output currently enabled?</summary>
        public virtual bool OutputEnabled {
            get { return _outputEnabled; }
            set { _outputEnabled = value; }
        }

        #endregion


        #region Output Settings

        [SerializeField, Header("Output Settings")]
        protected string _name;
        /// <summary></summary>
        public virtual string Name {
            get { return _name; }
            set { _name = value; }
        }

        [SerializeField,
        Tooltip("Is output type exclusive (Only one output of this type allowed?)")]
        protected bool _exclusiveType = false;
        /// <summary>Is output type exclusive (Only one output of this type allowed?)</summary>
        public virtual bool ExclusiveType {
            get { return _exclusiveType; }
            set { _exclusiveType = value; }
        }

        #endregion

        #region Unity methods

        protected virtual void Start() {
            // Add self to InputManager, disabling self if failure
            Debug.Assert(OutputManager.Instance != null, "Could not get ref to OutputManager!");
            if (OutputEnabled &= OutputManager.Instance.AddOutput(this))
                Debug.LogError("Could not add self to OutputManager!");
        }

        #endregion
    }
}