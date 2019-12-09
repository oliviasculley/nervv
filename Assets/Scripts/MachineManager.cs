// System
using System;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>
    /// This class handles adding and removing machines from the scene.
    /// It also handles sending data to the machines. Static references
    /// to self are added in Awake(), so any calls to Instance must
    /// happen in Start() or later.
    /// </summary>
    public class MachineManager : MonoBehaviour {
        #region Static
        public static List<MachineManager> Instances = new List<MachineManager>();
        #endregion

        #region Properties
        [Tooltip("List of machines in scene"), Header("Properties")]
        protected List<IMachine> _machines = new List<IMachine>();
        /// <summary>List of machines in scene</summary>
        public List<IMachine> Machines => _machines;

        public event EventHandler<MachineEventArgs> OnMachineAdded;
        public event EventHandler<MachineEventArgs> OnMachineRemoved;
        #endregion

        #region Settings
        [Header("Settings")]
        public bool PrintDebugMessages = false;
        #endregion

        #region Unity Methods
        /// <summary>Add static ref to self and init vars</summary>
        protected virtual void Awake() {
            // Init vars
            Instances = Instances ?? new List<MachineManager>() ??
                throw new ArgumentNullException();
            _machines = _machines ?? new List<IMachine>() ??
                throw new ArgumentNullException();

            // Add static ref to self
            if (PrintDebugMessages && Instances.Contains(this))
                Debug.LogWarning("Reference already exists in Instances list!");
            Instances.Add(this);
        }

        /// <summary>Remove static ref to self</summary>
        protected virtual void OnDestroy() {
            Instances.Remove(this);
        }
        #endregion

        #region Public Methods
        /// <summary>Adds machine to machines List</summary>
        /// <param name="machine">Machine to add</param>
        /// <returns>Succesfully added?</returns>
        public virtual bool AddMachine(IMachine machine) {
            // Check for duplicate machines
            if (Machines.Contains(machine)) {
                return false;
            }
                
            Machines.Add(machine);
            return true;
        }

        /// <summary>Removes machine from machines list</summary>
        /// <param name="machine">Machine to remove</param>
        /// <returns>Succesfully removed?</returns>
        public virtual bool RemoveMachine(IMachine machine) {
            return Machines.Remove(machine);
        }

        /// <summary>Returns machines of same type</summary>
        /// <typeparam name="T">Type of machine to return</typeparam>
        /// <returns>List<Machine> of machines</returns>
        public virtual List<IMachine> GetMachines<T>() {
            return Machines.FindAll(x => x.GetType() == typeof(T));;
        }

        /// <summary>Returns machines of same type</summary>
        /// <param name="type">Type of machine to return</param>
        /// <returns>List<Machine> of machines</returns>
        public virtual List<IMachine> GetMachines(string type) {
            return Machines.FindAll(x => x.GetType().ToString() == type);
        }
        #endregion

        #region Methods
        /// <summary>Convenience method to trigger OnOutputAdded</summary>
        protected virtual void TriggerOnMachineAdded(MachineEventArgs eventArgs) {
            OnMachineAdded?.Invoke(this, eventArgs);
        }

        /// <summary>Convenience method to trigger OnOutputRemoved</summary>
        protected virtual void TriggerOnMachineRemoved(MachineEventArgs eventArgs) {
            OnMachineRemoved?.Invoke(this, eventArgs);
        }
        #endregion

        #region EventTrigger Class
        public class MachineEventArgs : EventArgs {
            public IMachine Machine;

            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="machine"/> is null
            /// </exception>
            public MachineEventArgs(IMachine machine) {
                Machine = machine ?? throw new ArgumentNullException();
            }
        }
        #endregion
    }
}