// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;
using UnityEngine.Events;
using TMPro;

using NERVV.Menu.MachineDetailPanel;

namespace NERVV.Menu.MachineListPanel {
    public class MachinesListElement : MenuComponent {
        #region Properties
        private IMachine _currentMachine = null;
        /// <summary>
        /// The current machine pointed to by the button element.
        /// If set, enables the button!
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if set to null</exception>
        public IMachine CurrentMachine {
            get => _currentMachine;
            set {
                _currentMachine = value ?? throw new ArgumentNullException();
                ElementTitle.text = CurrentMachine.Name;    // Set Title
                gameObject.SetActive(true);
            }
        }
        #endregion

        #region References
        [Header("References")]
        public TextMeshProUGUI ElementTitle;
        #endregion

        #region Unity Methods
        protected void Awake() {
            if (ElementTitle == null) throw new ArgumentNullException();
            gameObject.SetActive(false);
        }

        protected override void OnEnable() {
            base.OnEnable();
        }
        #endregion

        #region Public Methods
        public void OpenMachineDetail() {
            // Check for valid current machine
            if (CurrentMachine == null)
                throw new InvalidOperationException("Current machine is null!");

            // If no MachineDetail panel, throw dependency exception
            if (!Menu.MenuPanels.TryGetValue(
                typeof(MachineDetail),
                out MenuPanel panel))
                throw new DependencyException();

            // Switch to machine detail panel
            ((MachineDetail)panel).CurrMachine = CurrentMachine;
            Menu.UISwitcher.ChangeMenu(panel.gameObject);
        }
        #endregion
    }
}