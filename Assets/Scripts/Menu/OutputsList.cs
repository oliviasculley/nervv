using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class OutputsList : MonoBehaviour {
    [Header("References")]

    [Tooltip("Parent to spawn machine buttons underneath")]
    public Transform scrollViewParent;
    public GameObject inputButtonPrefab;

    private void OnEnable() {
        Debug.Assert(inputButtonPrefab != null,
            "Could not get ref to input button prefab!");
        Debug.Assert(OutputManager.Instance != null,
            "Could not get ref to OutputManager instance!");

        GenerateOutputButtons();
    }

    #region Public Methods

    public void ToggleOutputs(IOutputSource o) {
        o.OutputEnabled = !o.OutputEnabled;
        GenerateOutputButtons();
    }

    #endregion

    #region Private Methods

    private void GenerateOutputButtons() {
        // Delete old objects
        foreach (Transform t in scrollViewParent.transform)
            Destroy(t.gameObject);

        // Spawn buttons for each machine
        GameObject g;
        foreach (IOutputSource o in OutputManager.Instance.outputs) {
            g = Instantiate(inputButtonPrefab, scrollViewParent);

            g.transform.Find("InputName").GetComponent<TextMeshProUGUI>().text = o.Name;
            g.transform.Find("ToggleEnabled").GetComponent<Toggle>().isOn = o.OutputEnabled;

            // Push machine to stack so button works correctly
            IOutputSource buttonOutput = o;
            g.GetComponent<Button>().onClick.AddListener(delegate { ToggleOutputs(buttonOutput); });
        }
    }

    #endregion
}
