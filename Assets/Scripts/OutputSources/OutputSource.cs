// System
using System;

// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>
    /// This is the base class for outputs. These are dynamically added to OutputManager.
    /// </summary>
    [Serializable]
    public abstract class OutputSource : MonoBehaviour, IOutputSource {
        #region Output Properties
        [SerializeField,
        Tooltip("Is this output currently enabled?"), Header("Output Properties")]
        protected bool _outputEnabled = true;
        /// <summary>Is this output currently enabled?</summary>
        public virtual bool OutputEnabled {
            get { return _outputEnabled; }
            set {
                _outputEnabled = value;
                if (InputManager.Instance != null && _outputEnabled)
                    InputManager.Instance.DisableInputs();
            }
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
            Debug.Assert(OutputManager.Instance != null);
            bool success = OutputManager.Instance.AddOutput(this);
            if (!success)
                Debug.LogError("Could not add self to OutputManager!");
            OutputEnabled &= success;
        }
        #endregion
    }
}