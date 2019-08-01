// Unity Engine
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MTConnectVR.Menu {
    /// <summary>Displays a list of machines in the menu</summary>
    public class MachinesList : MonoBehaviour {
        #region References
        /// <summary>Parent to spawn machine buttons underneath</summary>
        [Tooltip("Parent to spawn machine buttons underneath"), Header("References")]
        public Transform scrollViewParent;
        public GameObject machineButtonPrefab;
        public MachineDetail detailPanel;
        public UIPanelSwitcher switcher;
        #endregion

        #region Unity Methods
        /// <summary>Check references and generate buttons</summary>
        void OnEnable() {
            Debug.Assert(detailPanel != null,
                "Could not get ref to machine detail panel!");
            Debug.Assert(machineButtonPrefab != null,
                "Could not get ref to machine button prefab!");
            Debug.Assert(switcher != null,
                "Could not get ref to switcher!");
            Debug.Assert(MachineManager.Instance != null,
                "[Menu: MachinesList] Could not get ref to MachineManager!");

            GenerateMachineButtons();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// UI Wrapper to select a machine to inspect
        /// </summary>
        /// <param name="m">Machine to inspect</param>
        public void MachineClick(Machine m) {
            detailPanel.DisplayMachine(m);
            switcher.ChangeMenu(detailPanel.gameObject);
        }
        #endregion

        #region Methods
        void GenerateMachineButtons() {
            // Delete old objects
            foreach (Transform t in scrollViewParent.transform)
                Destroy(t.gameObject);

            // Spawn buttons for each machine
            GameObject g;
            foreach (Machine m in MachineManager.Instance.machines) {
                g = Instantiate(machineButtonPrefab, scrollViewParent);
                g.transform.Find("MachineName").GetComponent<TextMeshProUGUI>().text = "Name: " + m.name;

                // Push machine to stack so button works correctly
                Machine buttonM = m;
                g.GetComponent<Button>().onClick.AddListener(delegate { MachineClick(buttonM); });
            }
        }
        #endregion
    }
}