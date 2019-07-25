// System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

namespace MTConnectVR {
    /// <summary>
    /// This is the base implementation for inputs. These are dynamically
    /// added to the InputManager on load. View IInputSource.cs for more info.
    /// </summary>
    public abstract class InputSource : MonoBehaviour, IInputSource {

        #region Input Properties
        [Header("Input Properties")]

        [Tooltip(
            "If the input source is actively publishing to machines " +
            "or not. Note that the input source may still be inactive " +
            "even when this is false, just not actively publishing.")]
        [SerializeField] protected bool _inputEnabled = true;
        /// <summary>
        /// If the input source is actively publishing to machines
        /// or not. Note that the input source may still be inactive
        /// even when this is false, just not actively publishing.
        /// </summary>
        public virtual bool InputEnabled {
            get { return _inputEnabled; }
            set { _inputEnabled = value; }
        }

        #endregion

        #region Input Settings
        [Header("Input Settings")]

        [SerializeField] protected string _name;
        public virtual string Name {
            get { return _name; }
            set { _name = value; }
        }
        [Tooltip(
            "Are multiple instantiations of this script allowed? " +
            "InputManager will reject multiple types of this script " +
            "if ExclusiveType is true when added to InputManager.")]
        [SerializeField] private bool _exclusiveType;
        /// <summary>
        /// Are multiple instantiations of this script allowed?
        /// InputManager will reject multiple types of this script
        /// if ExclusiveType is true when added to InputManager.
        /// </summary>
        public virtual bool ExclusiveType {
            get { return _exclusiveType; }
            set { _exclusiveType = value; }
        }

        #endregion

        #region Unity Methods

        protected virtual void Start() {
            // Add self to InputManager, disabling self if failure
            Debug.Assert(InputManager.Instance != null, "Could not get ref to InputManager!");
            if (InputEnabled &= InputManager.Instance.AddInput(this))
                Debug.LogError("Could not add self to InputManager!");
        }

        #endregion

    }
}