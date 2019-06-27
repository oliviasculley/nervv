using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class Menu_MachinesList : MonoBehaviour
{
    [Header("References")]
    public Transform scrollViewParent;     // Parent to spawn machine buttons underneath
    public GameObject machineButtonPrefab;
    public Menu_MachineDetail detailPanel;

    private void Start() {
        Debug.Assert(detailPanel != null,
            "[Menu: MachinesList] Could not get ref to machine detail panel!");
        Debug.Assert(machineButtonPrefab != null,
            "[Menu: MachinesList] Could not get ref to machine button prefab!");
        Debug.Assert(MachineManager.Instance != null,
            "[Menu: MachinesList] Could not get ref to MachineManager!");
    }

    /* Public Methods */

    public void MachineClick(Machine m) {
        detailPanel.DisplayMachine(m);
    }

    /* Private Methods */

    private void GenerateMachineButtons() {
        GameObject g;
        foreach (Machine m in MachineManager.Instance.machines) {
            g = Instantiate(machineButtonPrefab, scrollViewParent);
            g.transform.Find("MachineName").GetComponent<TextMeshProUGUI>().text = m.name;

            // Push machine to stack so button works correctly
            Machine buttonM = m;
            g.GetComponent<Button>().onClick.AddListener(() => MachineClick(buttonM));
        }
    }
}
