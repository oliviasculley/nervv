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
            get => _outputEnabled;
            set {
                _outputEnabled = value;
                if (InputManager != null && _outputEnabled)
                    InputManager.DisableInputs();
            }
        }
        #endregion

        #region Output Settings
        [SerializeField, Header("Output Settings")]
        protected string _name;
        /// <summary></summary>
        public virtual string Name {
            get => _name;
            set => _name = value;
        }

        [SerializeField,
        Tooltip("Is output type exclusive (Only one output of this type allowed?)")]
        protected bool _exclusiveType = false;
        /// <summary>Is output type exclusive (Only one output of this type allowed?)</summary>
        public virtual bool ExclusiveType {
            get => _exclusiveType;
            set => _exclusiveType = value;
        }

        public bool PrintDebugMessages = false;
        #endregion

        #region Output References
        [SerializeField,
        Tooltip("If null, will attempt to use global reference"), Header("Output References")]
        protected OutputManager _outputManager;
        public OutputManager OutputManager {
            get {
                if (_outputManager == null) {
                    if (OutputManager.Instances.Count > 0)
                        _outputManager = OutputManager.Instances[0];
                    else
                        throw new ArgumentNullException("Could not get a ref to an OutputManager!");
                }
                return _outputManager;
            }
            set => _outputManager = value;
        }

        [SerializeField,
        Tooltip("If null, will attempt to use global reference")]
        protected InputManager _inputManager = null;
        public InputManager InputManager {
            get {
                if (_inputManager == null) {
                    if (InputManager.Instances.Count > 0)
                        _inputManager = InputManager.Instances[0];
                    else
                        throw new ArgumentNullException("Could not get a ref to an InputManager!");
                }
                return _inputManager;
            }
            set => _inputManager = value;
        }
        #endregion

        #region Unity methods
        /// <summary>Adds self to OutputManager</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown if OutputManager is null
        /// </exception>
        protected virtual void Start() {
            bool success = OutputManager.AddOutput(this);
            if (PrintDebugMessages && !success)
                Debug.LogError("Could not add self to OutputManager!");
            OutputEnabled &= success;
        }

        /// <summary>Removes self from OutputManager</summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown if OutputManager is null
        /// </exception>
        protected virtual void OnDisable() {
            if (!OutputManager.RemoveOutput(this) && PrintDebugMessages)
                Debug.LogError("Could not remove self from OutputManager!");
        }
        #endregion
    }
}