using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class InputsList : MonoBehaviour
{
    [Header("References")]
    public Transform scrollViewParent;     // Parent to spawn machine buttons underneath
    public GameObject inputButtonPrefab;
    public UIPanelSwitcher switcher;

    private void OnEnable()
    {
        Debug.Assert(inputButtonPrefab != null,
            "[Menu: MachinesList] Could not get ref to input button prefab!");
        Debug.Assert(switcher != null,
            "[Menu: MachinesList] Could not get ref to switcher!");
        Debug.Assert(InputManager.Instance != null,
            "[Menu: MachinesList] Could not get ref to InputManager instance!");

        GenerateInputButtons();
    }

    /* Public Methods */

    public void ToggleInput(InputSource i) {
        i.inputEnabled = !i.inputEnabled;
        GenerateInputButtons();
    }

    /* Private Methods */

    private void GenerateInputButtons() {
        // Delete old objects
        foreach (Transform t in scrollViewParent.transform)
            Destroy(t.gameObject);

        // Spawn buttons for each machine
        GameObject g;
        foreach (InputSource i in InputManager.Instance.inputs) {
            g = Instantiate(inputButtonPrefab, scrollViewParent);

            g.transform.Find("InputName").GetComponent<TextMeshProUGUI>().text = "Name:\n" + i.name;
            g.transform.Find("ToggleEnabled").GetComponent<Toggle>().isOn = i.inputEnabled;

            // Push machine to stack so button works correctly
            InputSource buttonInput = i;
            g.GetComponent<Button>().onClick.AddListener(delegate { ToggleInput(buttonInput); });
        }
    }
}
