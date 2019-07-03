using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPanelSwitcher : MonoBehaviour {
    public GameObject initialPanel;
    public bool setInactive;
    public List<GameObject> UIPanels;

    private GameObject activePanel;

    private void Awake() {
        Debug.Assert(initialPanel != null);
    }

    private void OnEnable() {
        // Disable all children
        if (setInactive)
            foreach (GameObject g in UIPanels)
                g.SetActive(false);

        // Display initialPanel
        ChangeMenu(initialPanel);
    }

    /// <summary>
    /// Changes UI panel to new panel
    /// </summary>
    /// <param name="panel">New UI panel to display</param>
    public void ChangeMenu(GameObject panel) {
        if (activePanel != null)
            activePanel.SetActive(false);

        (activePanel = panel).SetActive(true);
    }
}
