// System
using System;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>
    /// The InputManager handles different input sources and sends
    /// the data to the machines through the MachineManager. Static
    /// references to self are added in Awake(), so any calls to
    /// Instances must happen in Start() or later.
    /// </summary>
    public class InputManager : MonoBehaviour {
        #region Static
        public static List<InputManager> Instances = new List<InputManager>();
        #endregion

        #region Properties
        [SerializeField, Tooltip("List of input sources in scene"), Header("Properties")]
        protected List<IInputSource> _inputs = new List<IInputSource>();
        /// <summary>List of input sources in scene</summary>
        public List<IInputSource> Inputs => _inputs;

        public event EventHandler<InputEventArgs> OnInputAdded;
        public event EventHandler<InputEventArgs> OnInputRemoved;
        #endregion

        #region Settings
        /// <summary>
        /// Inputs that won't get disabled by default when an output source
        /// initializes. Can still get disabled if DisableInputs(true) is called!
        /// </summary>
        [SerializeField, Tooltip("Inputs that won't get disabled by default" +
            " when an output source runs. Can still get disabled if " +
            "DisableInputs(true) is called!"), Header("Settings")]
        public List<InputSource> DisableExceptions = new List<InputSource>();

        public bool PrintDebugMessages = false;
        #endregion

        #region Vars
        /// <summary>Keeps track of exclusive types in inputs</summary>
        protected List<Type> knownExclusives = new List<Type>();
        #endregion

        #region Unity Methods
        /// <summary>Add static ref to self and init vars</summary>
        protected virtual void OnEnable() {
            // Init vars
            Instances = Instances ?? new List<InputManager>() ??
                throw new ArgumentNullException();
            knownExclusives = knownExclusives ?? new List<Type>() ??
                throw new ArgumentNullException();
            _inputs = _inputs ?? new List<IInputSource>() ??
                throw new ArgumentNullException();
            DisableExceptions = DisableExceptions ?? new List<InputSource>() ??
                throw new ArgumentNullException();

            // Add static ref to self
            if (PrintDebugMessages && Instances.Contains(this))
                Debug.LogWarning("Reference already exists in Instances list!");
            Instances.Add(this);
        }

        /// <summary>Remove static ref to self</summary>
        protected virtual void OnDisable() {
            Debug.Assert(Instances.Remove(this));
        }
        #endregion

        #region Public Methods
        /// <summary>Adds an input to list of inputs</summary>
        /// <param name="input">Input source to add</param>
        /// <returns>Succesfully added?</returns>
        public virtual bool AddInput(IInputSource input) {
            // Check for duplicate inputs
            if (_inputs.Contains(input))
                return false;

            // Check for same type for exclusive types
            if (knownExclusives.Contains(input.GetType()))
                return false;

            // Add to knownExclusives if exclusive
            if (input.ExclusiveType)
                knownExclusives.Add(input.GetType());

            // Add to list of inputs
            _inputs.Add(input);
            TriggerOnInputAdded(new InputEventArgs(input));
            return true;
        }

        /// <summary>Removes an input from list of inputs</summary>
        /// <param name="input">Input source to remove</param>
        /// <returns>Succesfully removed?</returns>
        public bool RemoveInput(IInputSource input) {
            // If exclusive, remove from exclusives list
            if (input.ExclusiveType)
                knownExclusives.Remove(input.GetType());

            // Remove from list of inputs
            if (_inputs.Remove(input)) {
                TriggerOnInputRemoved(new InputEventArgs(input));
                return true;
            }
            return false;
        }

        /// <summary>Returns inputs of same type</summary>
        /// <typeparam name="T">Type of input to return</typeparam>
        /// <returns>List<InputSource> of inputs</returns>
        public List<IInputSource> GetInputs<T>() {
            return Inputs.FindAll(x => x.GetType() == typeof(T));
        }

        /// <summary>Returns inputs of same type</summary>
        /// <param name="type">String of name of type of input to return</param>
        /// <returns>List<InputSource> of inputs</returns>
        public List<IInputSource> GetInputs(string type) {
            return Inputs.FindAll(x => x.GetType().ToString() == type);
        }

        /// <summary>Disables all inputs not in DisableExceptions List</summary>
        /// <param name="forceDisable">
        /// If true, will disable all inputs, regardless if inputs
        /// are in DisableExceptions List or not.
        /// </param>
        public void DisableInputs(bool forceDisable = false) {
            foreach (IInputSource i in Inputs) {
                if (!forceDisable) {
                    try {
                        if (DisableExceptions.Contains((InputSource)i)) continue;
                    } catch (InvalidCastException) {
                        // Disable since can't be in DisableExceptions by definition
                    }
                }
                i.InputEnabled = false;
            }
        }
        #endregion

        #region Methods
        /// <summary>Convenience method to trigger OnInputAdded</summary>
        protected virtual void TriggerOnInputAdded(InputEventArgs eventArgs) {
            OnInputAdded?.Invoke(this, eventArgs);
        }

        /// <summary>Convenience method to trigger OnOutputRemoved</summary>
        protected virtual void TriggerOnInputRemoved(InputEventArgs eventArgs) {
            OnInputRemoved?.Invoke(this, eventArgs);
        }

        protected void Log(string s) { if (PrintDebugMessages) Debug.Log("<b>[" + GetType() + "]</b>" + s); }
        protected void LogWarning(string s) { if (PrintDebugMessages) Debug.LogWarning("<b>[" + GetType() + "]</b>" + s); }
        protected void LogError(string s) { if (PrintDebugMessages) Debug.LogError("<b>[" + GetType() + "]</b>" + s); }
        #endregion

        #region EventTrigger Class
        public class InputEventArgs : EventArgs {
            public IInputSource InputSource;
            public InputEventArgs(IInputSource InputSource) {
                this.InputSource = InputSource ?? throw new ArgumentNullException();
            }
        }
        #endregion
    }
}
