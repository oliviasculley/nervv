// System
using System;

// Unity Engine
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// NERVV
using NERVV.Menu.MachineDetailPanel;

namespace NERVV.Menu.MachineListPanel {
    /// <summary>Displays a list of machines in the menu</summary>
    public class MachinesList : MenuPanel {
        #region References
        /// <summary>Parent to spawn machine buttons underneath</summary>
        [Tooltip("Parent to spawn machine buttons underneath"), Header("References")]
        public Transform ScrollViewParent;
        public GameObject MachineListElementPrefab;

        [SerializeField, Tooltip("If null, will attempt to use global reference")]
        protected MachineManager _machineManager;
        public MachineManager MachineManager {
            get {
                if (_machineManager == null) {
                    if (MachineManager.Instances.Count > 0)
                        _machineManager = MachineManager.Instances[0];
                    else
                        throw new ArgumentNullException("Could not get a ref to a MachineManager!");
                }
                return _machineManager;
            }
            set => _machineManager = value;
        }
        #endregion

        #region Unity Methods
        /// <summary>Check references, generate initial buttons and register callbacks</summary>
        protected override void OnEnable() {
            if (MachineManager == null) throw new ArgumentNullException();
            if (MachineListElementPrefab == null) throw new ArgumentNullException();

            GenerateMachineButtons();
            MachineManager.OnMachineAdded += AddMachineElement;
            MachineManager.OnMachineRemoved += RemoveMachineElement;

            base.OnEnable();
        }

        /// <summary>Removes callbacks</summary>
        protected override void OnDisable() {
            base.OnDisable();

            MachineManager.OnMachineAdded -= AddMachineElement;
            MachineManager.OnMachineRemoved -= RemoveMachineElement;

            // Delete old objects
            foreach (Transform t in ScrollViewParent.transform)
                Destroy(t.gameObject);
        }
        #endregion

        #region Methods
        protected void GenerateMachineButtons() {
            // Delete old objects
            foreach (Transform t in ScrollViewParent.transform)
                Destroy(t.gameObject);

            // Spawn buttons for each machine
            foreach (IMachine m in MachineManager.Machines)
                AddMachineElement(this, new MachineManager.MachineEventArgs(m));
        }

        /// <summary>Called when machine is added, spawns new machine element</summary>
        protected void AddMachineElement(object sender, MachineManager.MachineEventArgs eventArgs) {
            GameObject g = Instantiate(MachineListElementPrefab, ScrollViewParent);
            var e = g.GetComponent<MachinesListElement>();
            Debug.Assert(e != null);

            // Set button initial values
            e.CurrentMachine = eventArgs.Machine;   // Enables toggle button
        }

        /// <summary>Called when machine is removed, removes all matching machine elements</summary>
        protected void RemoveMachineElement(object sender, MachineManager.MachineEventArgs eventArgs) {
            foreach (var e in ScrollViewParent.GetComponentsInChildren<MachinesListElement>())
                if ((IMachine) e.CurrentMachine == eventArgs.Machine) Destroy(e.gameObject);
        }
        #endregion
    }
}