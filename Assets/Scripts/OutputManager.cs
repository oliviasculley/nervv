// System
using System;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>
    /// This class handles different outputs sources. Static reference
    /// to self is set in Awake(), so any calls to Instance must happen
    /// in Start() or later.
    /// </summary>
    public class OutputManager : MonoBehaviour {
        #region Static
        public static OutputManager Instance;
        #endregion

        #region Properties
        [SerializeField,
        Tooltip("List of output sources in scene"), Header("Properties")]
        protected List<IOutputSource> _outputs;
        /// <summary>List of output sources in scene</summary>
        public List<IOutputSource> Outputs {
            get { return _outputs; }
            set { _outputs = value; }
        }
        #endregion

        #region Settings
        /// <summary>
        /// Outputs that won't get disabled by default when an output source
        /// runs. Can still get disabled if DisableInputs(true) is called!
        /// </summary>
        [Tooltip("Outputs that won't get disabled by default" +
            " when an input source initializes. Can still get disabled if " +
            "DisableOutputs(true) is called!"), Header("Settings")]
        public List<OutputSource> DisableExceptions;
        #endregion

        #region Vars
        /// <summary>Keeps track of exclusive types in outputs</summary>
        protected List<System.Type> knownExclusives;
        #endregion

        #region Unity Methods
        /// <summary>Set static ref to self and initialize vars</summary>
        protected virtual void Awake() {
            // Initialize vars
            knownExclusives = new List<System.Type>();
            _outputs = new List<IOutputSource>();

            // Add static reference to self
            if (Instance != null)
                Debug.LogWarning("[OutputManager] Static ref to self was not null!\nOverriding...");
            Instance = this;
        }
        #endregion

        #region Public Methods
        /// <summary>Adds an output to list of outputs</summary>
        /// <param name="output">Output source to add</param>
        /// <returns>Succesfully added?</returns>
        public virtual bool AddOutput(IOutputSource output) {
            // Check for duplicate outputs
            if (Outputs.Contains(output))
                return false;

            // Check for same type for exclusive types
            if (knownExclusives.Contains(output.GetType()))
                return false;

            // Add to knownExclusives if exclusive
            if (output.ExclusiveType)
                knownExclusives.Add(output.GetType());

            // Add to list of outputs
            Outputs.Add(output);
            return true;
        }

        /// <summary>Removes an output from list of outputs</summary>
        /// <param name="output">Output source to remove</param>
        /// <returns>Succesfully removed?</returns>
        public virtual bool RemoveOutput(IOutputSource output) {
            // If exclusive, remove from exclusives list
            if (output.ExclusiveType)
                knownExclusives.Remove(output.GetType());

            // Remove from list of outputs
            return Outputs.Remove(output);
        }

        /// <summary>Returns outputs of same type</summary>
        /// <typeparam name="T">Type of output to return</typeparam>
        /// <returns>List<OutputSource> of outputs</returns>
        public virtual List<IOutputSource> GetOutputs<T>() {
            List<IOutputSource> foundOutputs = new List<IOutputSource>();

            foreach (IOutputSource i in Outputs)
                if (i.GetType() == typeof(T))
                    foundOutputs.Add(i);

            return foundOutputs;
        }

        /// <summary>Returns outputs of same type</summary>
        /// <param name="type">String of name of type of output to return</param>
        /// <returns>List<OutputSource> of outputs</returns>
        public virtual List<IOutputSource> GetOutputs(string type) {
            List<IOutputSource> foundOutputs = new List<IOutputSource>();

            foreach (IOutputSource i in Outputs)
                if (i.GetType().ToString() == type)
                    foundOutputs.Add(i);

            return foundOutputs;
        }

        /// <summary>Disables all outputs not in DisableExceptions List</summary>
        /// <param name="forceDisable">
        /// If true, will disable all outputs, regardless if outputs
        /// are in DisableExceptions List or not.
        /// </param>
        public void DisableOutputs(bool forceDisable = false) {
            foreach (IOutputSource o in Outputs) {
                try {
                    if (!forceDisable && DisableExceptions.Contains((OutputSource)o)) continue;
                } catch (InvalidCastException) {
                    // continue since can't be in DisableExceptions by definition
                    continue;
                }
                o.OutputEnabled = false;
            }
        }
        #endregion
    }
}
