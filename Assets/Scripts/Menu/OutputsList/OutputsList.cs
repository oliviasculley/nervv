// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NERVV.Menu.OutputsListPanel {
    public class OutputsList : MenuPanel {
        #region References
        /// <summary>Parent to spawn machine buttons underneath</summary>
        [Tooltip("Parent to spawn machine buttons underneath"),
        Header("References")]
        public Transform scrollViewParent;
        public GameObject outputToggleElementPrefab;

        [SerializeField, Tooltip("If null, will attempt to use global reference")]
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
        /// <summary>Check references and generate buttons OnEnable</summary>
        protected override void OnEnable() {
            if (outputToggleElementPrefab == null) throw new ArgumentNullException();
            if (OutputManager == null) throw new ArgumentNullException();

            GenerateOutputButtons();
            OutputManager.OnOutputAdded += AddOutputToggleElement;
            OutputManager.OnOutputRemoved += RemoveOutputToggleElement;

            base.OnEnable();
        }

        /// <summary>Deletes old objects on destroy</summary>
        protected override void OnDisable() {
            base.OnDisable();

            // Delete old objects
            foreach (Transform t in scrollViewParent.transform)
                Destroy(t.gameObject);
        }
        #endregion

        #region Methods
        protected void GenerateOutputButtons() {
            // Delete old objects
            foreach (Transform t in scrollViewParent.transform)
                Destroy(t.gameObject);

            // Spawn buttons for each machine
            foreach (var o in OutputManager.Outputs)
                AddOutputToggleElement(this, new OutputManager.OutputEventArgs(o));
        }

        protected void AddOutputToggleElement(object sender, OutputManager.OutputEventArgs args) {
            var gameObject = Instantiate(outputToggleElementPrefab, scrollViewParent);
            var toggleScript = gameObject.GetComponent<OutputToggleElement>();
            Debug.Assert(toggleScript != null);

            // Set toggle button initial values
            toggleScript.Initialize(args.OutputSource, args.OutputSource.OutputEnabled);
        }

        protected void RemoveOutputToggleElement(object sender, OutputManager.OutputEventArgs args) {
            foreach (var e in scrollViewParent.GetComponentsInChildren<OutputToggleElement>())
                if (e != null && e.Output == args.OutputSource) Destroy(e.gameObject);
        }
        #endregion
    }
}