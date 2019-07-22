
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MTConnectVR.Menu {
    public class MachinesList : MonoBehaviour {
        [Header("References")]
        public Transform scrollViewParent;     // Parent to spawn machine buttons underneath
        public GameObject machineButtonPrefab;
        public MachineDetail detailPanel;
        public UIPanelSwitcher switcher;

        private void OnEnable() {
            Debug.Assert(detailPanel != null,
                "[Menu: MachinesList] Could not get ref to machine detail panel!");
            Debug.Assert(machineButtonPrefab != null,
                "[Menu: MachinesList] Could not get ref to machine button prefab!");
            Debug.Assert(switcher != null,
                "[Menu: MachinesList] Could not get ref to switcher!");
        }

        private void Start() {
            Debug.Assert(MachineManager.Instance != null,
                "[Menu: MachinesList] Could not get ref to MachineManager!");

            GenerateMachineButtons();
        }

        #region Public Methods

        public void MachineClick(Machine m) {
            detailPanel.DisplayMachine(m);
            switcher.ChangeMenu(detailPanel.gameObject);
        }

        #endregion

        #region Private Methods

        private void GenerateMachineButtons() {
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