// System
using System;

// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>
    /// This is the base implementation for inputs. These are dynamically
    /// added to the InputManager on load. View IInputSource.cs for more info.
    /// </summary>
    [Serializable]
    public abstract class InputSource : MonoBehaviour, IInputSource {
        #region Input Properties
        [SerializeField,
        Tooltip(
            "If the input source is actively publishing to machines " +
            "or not. Note that the input source may still be inactive " +
            "even when this is false, just not actively publishing."),
        Header("Input Properties")]
        protected bool _inputEnabled = true;
        /// <summary>
        /// If the input source is actively publishing to machines
        /// or not. Note that the input source may still be inactive
        /// even when this is false, just not actively publishing.
        /// </summary>
        public virtual bool InputEnabled {
            get => _inputEnabled;
            set {
                _inputEnabled = value;
                if (OutputManager != null && _inputEnabled)
                    OutputManager.DisableOutputs();
            }
        }
        #endregion

        #region Input Settings
        [SerializeField, Header("Input Settings")] protected string _name;
        public virtual string Name {
            get => _name;
            set => _name = value;
        }

        [SerializeField,
        Tooltip(
            "Are multiple instantiations of this script allowed? " +
            "InputManager will reject multiple types of this script " +
            "if ExclusiveType is true when added to InputManager.")]
        bool _exclusiveType;
        /// <summary>Are multiple instantiations of this script allowed?</summary>
        /// <remarks>
        /// InputManager will reject multiple types of this script
        /// if ExclusiveType is true when added to InputManager.
        /// </remarks>
        public virtual bool ExclusiveType {
            get => _exclusiveType;
            set => _exclusiveType = value;
        }

        public bool PrintDebugMessages = false;
        #endregion

        #region Input References
        [SerializeField,
        Tooltip("If null, will attempt to use global reference"), Header("Input References")]
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

        [SerializeField,
        Tooltip("If null, will attempt to use global reference")]
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
        #endregion

        #region Unity Methods
        /// <summary>Add self to InputManager, disabling self if failure</summary>
        /// <exception cref="ArgumentNullException">
        /// Throws if InputManager is null
        /// </exception>
        protected virtual void OnEnable() {
            var success = InputManager.AddInput(this);
            if (PrintDebugMessages && !success)
                Debug.LogError("Could not add self to InputManager!");
            InputEnabled &= success;
        }

        /// <summary>Removes input from InputManager</summary>
        /// <exception cref="ArgumentNullException">
        /// Throws if InputManager is null
        /// </exception>
        protected virtual void OnDisable() {
            if (PrintDebugMessages && !InputManager.RemoveInput(this))
                Debug.LogError("Could not remove self from InputManager!");
        }

        protected void Log(string s) { if (PrintDebugMessages) Debug.Log("<b>[" + GetType() + "]</b>" + s); }
        protected void LogWarning(string s) { if (PrintDebugMessages) Debug.LogWarning("<b>[" + GetType() + "]</b>" + s); }
        protected void LogError(string s) { if (PrintDebugMessages) Debug.LogError("<b>[" + GetType() + "]</b>" + s); }
        #endregion
    }
}