// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NERVV.Menu.InputsListPanel {
    public class InputsList : MenuPanel {
        #region References
        /// <summary>Parent to spawn machine buttons underneath</summary>
        [Tooltip("Parent to spawn machine buttons underneath"),
        Header("References")]
        public Transform scrollViewParent;
        public GameObject inputToggleElementPrefab;

        [SerializeField, Tooltip("If null, will attempt to use global reference")]
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
        #endregion

        #region Unity Methods
        /// <summary>Check references and generate input button</summary>
        protected override void OnEnable() {
            if (inputToggleElementPrefab == null) throw new ArgumentNullException();
            if (InputManager == null) throw new ArgumentNullException();

            GenerateInputButtons();
            InputManager.OnInputAdded += AddInputToggleElement;
            InputManager.OnInputRemoved += RemoveInputToggleElement;

            base.OnEnable();
        }
        #endregion

        #region Methods
        protected void GenerateInputButtons() {
            // Delete old objects
            foreach (Transform t in scrollViewParent.transform)
                Destroy(t.gameObject);

            // Spawn buttons for each machine
            foreach (var i in InputManager.Inputs)
                AddInputToggleElement(this, new InputManager.InputEventArgs(i));
        }

        protected void AddInputToggleElement(object sender, InputManager.InputEventArgs args) {
            var gameObject = Instantiate(inputToggleElementPrefab, scrollViewParent);
            var toggleScript = gameObject.GetComponent<InputToggleElement>();
            Debug.Assert(toggleScript != null);

            // Set toggle button initial values
            toggleScript.Input = args.InputSource;  // Enables toggle button
        }

        protected void RemoveInputToggleElement(object sender, InputManager.InputEventArgs args) {
            foreach (var e in scrollViewParent.GetComponentsInChildren<InputToggleElement>())
                if (e.Input == args.InputSource) Destroy(e.gameObject);
        }
        #endregion
    }
}