// System
using System;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>
    /// This class handles different outputs sources. Static references
    /// to self are added in Awake(), so any calls to Instance must
    /// happen in Start() or later.
    /// </summary>
    public class OutputManager : MonoBehaviour {
        #region Static
        public static List<OutputManager> Instances = new List<OutputManager>();
        #endregion

        #region Properties
        [SerializeField,
        Tooltip("List of output sources in scene"), Header("Properties")]
        protected List<IOutputSource> _outputs = new List<IOutputSource>();
        /// <summary>List of output sources in scene</summary>
        public List<IOutputSource> Outputs => _outputs;
        
        public event EventHandler<OutputEventArgs> OnOutputAdded;
        public event EventHandler<OutputEventArgs> OnOutputRemoved;
        #endregion

        #region Settings
        /// <summary>
        /// Outputs that won't get disabled by default when an output source
        /// runs. Can still get disabled if DisableInputs(true) is called!
        /// </summary>
        [Tooltip("Outputs that won't get disabled by default" +
            " when an input source initializes. Can still get disabled if " +
            "DisableOutputs(true) is called!"), Header("Settings")]
        public List<OutputSource> DisableExceptions = new List<OutputSource>();

        public bool PrintDebugMessages = false;
        #endregion

        #region Vars
        /// <summary>Keeps track of exclusive types in outputs</summary>
        protected List<Type> knownExclusives = new List<Type>();
        #endregion

        #region Unity Methods
        /// <summary>Init vars and add static ref to self</summary>
        protected virtual void OnEnable() {
            // Init vars
            Instances = Instances ?? new List<OutputManager>() ??
                throw new ArgumentNullException();
            knownExclusives = knownExclusives ?? new List<Type>() ??
                throw new ArgumentNullException();
            _outputs = _outputs ?? new List<IOutputSource>() ??
                throw new ArgumentNullException();
            DisableExceptions = DisableExceptions ?? new List<OutputSource>() ??
                throw new ArgumentNullException();

            // Add static ref to self
            if (Instances.Contains(this))
                LogWarning("Reference already exists in Instances list!");
            Instances.Add(this);
        }

        /// <summary>Remove static ref to self</summary>
        protected virtual void OnDisable() {
            Debug.Assert(Instances.Remove(this));
        }
        #endregion

        #region Public Methods
        /// <summary>Adds an output to list of outputs</summary>
        /// <param name="output">Output source to add</param>
        /// <returns>Succesfully added?</returns>
        public virtual bool AddOutput(IOutputSource output) {
            // Check for duplicate outputs
            if (Outputs.Contains(output)) return false;

            // Check for same type for exclusive types
            if (knownExclusives.Contains(output.GetType())) return false;

            // Add to knownExclusives if exclusive
            if (output.ExclusiveType) knownExclusives.Add(output.GetType());

            // Add to list of outputs
            Outputs.Add(output);
            TriggerOnOutputAdded(new OutputEventArgs(output));
            return true;
        }

        /// <summary>Removes an output from list of outputs</summary>
        /// <param name="output">Output source to remove</param>
        /// <returns>Succesfully removed?</returns>
        public virtual bool RemoveOutput(IOutputSource output) {
            // If exclusive, remove from exclusives list
            if (output.ExclusiveType) knownExclusives.Remove(output.GetType());

            // Remove from list of outputs
            if (Outputs.Remove(output)) {
                TriggerOnOutputRemoved(new OutputEventArgs(output));
                return true;
            }
            return false;
        }

        /// <summary>Returns outputs of same type</summary>
        /// <typeparam name="T">Type of output to return</typeparam>
        /// <returns>List<OutputSource> of outputs</returns>
        public virtual List<IOutputSource> GetOutputs<T>() {
            return Outputs.FindAll(x => x.GetType() == typeof(T));
        }

        /// <summary>Returns outputs of same type</summary>
        /// <param name="type">String of name of type of output to return</param>
        /// <returns>List<OutputSource> of outputs</returns>
        public virtual List<IOutputSource> GetOutputs(string type) {
            return Outputs.FindAll(x => x.GetType().ToString() == type);
        }

        /// <summary>Disables all outputs not in DisableExceptions List</summary>
        /// <param name="forceDisable">
        /// If true, will disable all outputs, regardless if outputs
        /// are in DisableExceptions List or not.
        /// </param>
        public void DisableOutputs(bool forceDisable = false) {
            foreach (IOutputSource o in Outputs) {
                if (!forceDisable) {
                    try {
                        if (DisableExceptions.Contains((OutputSource)o)) continue;
                    } catch (InvalidCastException) {
                        // Disable since can't be in DisableExceptions by definition
                    }
                }
                o.OutputEnabled = false;
            }
        }
        #endregion

        #region Methods
        /// <summary>Convenience method to trigger OnOutputAdded</summary>
        protected virtual void TriggerOnOutputAdded(OutputEventArgs eventArgs) {
            OnOutputAdded?.Invoke(this, eventArgs);
        }

        /// <summary>Convenience method to trigger OnOutputRemoved</summary>
        protected virtual void TriggerOnOutputRemoved(OutputEventArgs eventArgs) {
            OnOutputRemoved?.Invoke(this, eventArgs);
        }
        #endregion

        #region EventTrigger Class
        public class OutputEventArgs : EventArgs {
            public IOutputSource OutputSource;
            public OutputEventArgs(IOutputSource outputSource) {
                OutputSource = outputSource ?? throw new ArgumentNullException();
            }
        }

        protected void Log(string s) { if (PrintDebugMessages) Debug.Log($"<b>[{GetType()}]</b> " + s); }
        protected void LogWarning(string s) { if (PrintDebugMessages) Debug.LogWarning($"<b>[{GetType()}]</b> " + s); }
        protected void LogError(string s) { if (PrintDebugMessages) Debug.LogError($"<b>[{GetType()}]</b> " + s); }
        #endregion
    }
}
