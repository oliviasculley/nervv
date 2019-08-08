// System
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>
    /// This class handles adding and removing machines from the scene.
    /// It also handles sending data to the machines. Static reference
    /// to self is set in Awake(), so any calls to Instance must happen
    /// in Start() or later.
    /// </summary>
    public class MachineManager : MonoBehaviour {
        #region Static
        public static MachineManager Instance;
        #endregion

        #region Properties
        [SerializeField,
        Tooltip("List of machines in scene"), Header("Properties")]
        protected List<IMachine> _machines;
        /// <summary>List of machines in scene</summary>
        public List<IMachine> Machines {
            get { return _machines; }
            set { _machines = value; }
        }
        #endregion

        #region Unity Methods
        /// <summary>Initialize and set static ref to self</summary>
        protected virtual void Awake() {
            _machines = new List<IMachine>();

            if (Instance != null)
                Debug.LogWarning("Static ref to self was not null!\nOverriding...");
            Instance = this;
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
            List<IMachine> foundMachines = new List<IMachine>();

            foreach (IMachine m in Machines)
                if (m.GetType() == typeof(T))
                    foundMachines.Add(m);

            return foundMachines;
        }

        /// <summary>Returns machines of same type</summary>
        /// <param name="type">Type of machine to return</param>
        /// <returns>List<Machine> of machines</returns>
        public virtual List<IMachine> GetMachines(System.Type type) {
            List<IMachine> foundMachines = new List<IMachine>();

            foreach (IMachine m in Machines)
                if (m.GetType() == type)
                    foundMachines.Add(m);

            return foundMachines;
        }
        #endregion
    }
}