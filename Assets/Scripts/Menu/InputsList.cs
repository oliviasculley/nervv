//System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputsList : MonoBehaviour {
    #region References
    /// <summary>Parent to spawn machine buttons underneath</summary>
    [Tooltip("Parent to spawn machine buttons underneath"),
    Header("References")]
    public Transform scrollViewParent;
    public GameObject inputButtonPrefab;
    #endregion

    #region Unity Methods
    /// <summary>Check references and generate input button</summary>
    void OnEnable() {
        Debug.Assert(inputButtonPrefab != null,
            "[Menu: MachinesList] Could not get ref to input button prefab!");
        Debug.Assert(InputManager.Instance != null,
            "[Menu: MachinesList] Could not get ref to InputManager instance!");

        GenerateInputButtons();
    }
    #endregion

    #region Public Methods
    public void ToggleInput(IInputSource i) {
        i.InputEnabled = !i.InputEnabled;
        GenerateInputButtons();
    }
    #endregion

    #region Methods
    void GenerateInputButtons() {
        // Delete old objects
        foreach (Transform t in scrollViewParent.transform)
            Destroy(t.gameObject);

        // Spawn buttons for each machine
        GameObject g;
        foreach (IInputSource i in InputManager.Instance.Inputs) {
            g = Instantiate(inputButtonPrefab, scrollViewParent);

            g.transform.Find("InputName").GetComponent<TextMeshProUGUI>().text = i.Name;
            g.transform.Find("ToggleEnabled").GetComponent<Toggle>().isOn = i.InputEnabled;

            // Push machine to stack so button works correctly
            IInputSource buttonInput = i;
            g.GetComponent<Button>().onClick.AddListener(delegate { ToggleInput(buttonInput); });
        }
    }
    #endregion
}
