// System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

public class UIPanelSwitcher : MonoBehaviour {
    #region Settings
    /// <summary>Panel to first be set initially</summary>
    [Tooltip("Panel to first be set initially"), Header("Settings")]
    public GameObject initialPanel;

    /// <summary>If true, set all children as inactive on enable</summary>
    [Tooltip("If true, set all children as inactive on enable")]
    public bool setInactive;
    #endregion

    #region References
    /// <summary>UI Panels to set inactive on load</summary>
    [Tooltip("UI Panels to set inactive on load")]
    public List<GameObject> UIPanels;
    #endregion

    #region Vars
    GameObject activePanel;
    #endregion

    #region Unity Methods
    /// <summary>Check references</summary>
    void Awake() {
        Debug.Assert(initialPanel != null,
            "Initial panel was null!");
    }

    /// <summary>Potentially disable children and display initial panel</summary>
    void OnEnable() {
        // Disable all children
        if (setInactive)
            foreach (GameObject g in UIPanels)
                g.SetActive(false);

        // Display initialPanel
        ChangeMenu(initialPanel);
    }
    #endregion

    #region Public Methods
    /// <summary>Changes UI panel to new panel</summary>
    /// <param name="panel">New UI panel to display</param>
    public void ChangeMenu(GameObject panel) {
        if (activePanel != null)
            activePanel.SetActive(false);

        (activePanel = panel).SetActive(true);
    }
    #endregion
}