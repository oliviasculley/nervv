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
            get { return _inputEnabled; }
            set {
                _inputEnabled = value;
                if (OutputManager.Instance != null && _inputEnabled)
                    OutputManager.Instance.DisableOutputs();
            }
        }
        #endregion

        #region Input Settings
        [SerializeField, Header("Input Settings")] protected string _name;
        public virtual string Name {
            get { return _name; }
            set { _name = value; }
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
            get { return _exclusiveType; }
            set { _exclusiveType = value; }
        }

        public bool PrintDebugMessages = false;
        #endregion

        #region Unity Methods
        /// <summary>Add self to InputManager, disabling self if failure</summary>
        /// <exception cref="ArgumentNullException">
        /// Throws if InputManager.Instance is null
        /// </exception>
        protected virtual void OnEnable() {
            if (InputManager.Instance == null)
                throw new ArgumentNullException("InputManager.Instance is null!");

            var success = InputManager.Instance.AddInput(this);
            if (PrintDebugMessages && !success)
                Debug.LogError("Could not add self to InputManager!");
            InputEnabled &= success;
        }

        /// <summary>Removes input from InputManager</summary>
        /// <exception cref="ArgumentNullException">
        /// Throws if InputManager.Instance is null
        /// </exception>
        protected virtual void OnDisable() {
            if (InputManager.Instance == null)
                throw new ArgumentNullException("InputManager.Instance is null!");

            if (PrintDebugMessages && !InputManager.Instance.RemoveInput(this))
                Debug.LogError("Could not remove self from InputManager!");
        }
        #endregion
    }
}