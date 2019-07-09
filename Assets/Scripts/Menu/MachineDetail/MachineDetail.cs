using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class MachineDetail : MonoBehaviour
{
    [Header("Properties")]
    public Machine currMachine;

    [Header("Settings")]
    public string[] stringFieldNamesToGenerate; // String fields to generate handlers for
    public string[] floatFieldNamesToGenerate;  // Float fields to generate handlers for
    public bool generateAngles = true;          // Generate angle modifiers

    [Header("References")]
    public TextMeshProUGUI machineTitle;
    public Transform machineElementParent;

    [Header("Prefabs")]
    public GameObject machineElementStringPrefab;   // Changeable machine element
    public GameObject machineElementFloatPrefab, machineElementAxisPrefab;

    private void OnEnable() {
        // Get references
        Debug.Assert(machineTitle != null,
            "[Menu: Machine Detail] Could not get ref to machine title!");
        Debug.Assert(machineElementStringPrefab != null &&
            machineElementFloatPrefab != null &&
            machineElementAxisPrefab != null,
            "[Menu: Machine Detail] Could not get machine element prefabs!");
        Debug.Assert(machineElementParent != null,
            "[Menu: Machine Detail] Could not get machine element parent!");

        // Safety checks
        foreach (string s in stringFieldNamesToGenerate)
            Debug.Assert(!string.IsNullOrEmpty(s),
                "[Menu: Machine Detail] Invalid string field name!");
    }

    /* Public methods */

    public void DisplayMachine(Machine m) {
        // Safety checks
        if ((currMachine = m) == null) {
            Debug.LogWarning("[Menu: Machine Detail] Invalid machine! Skipping...");
            return;
        }

        // Delete all previous elements
        foreach (Transform t in machineElementParent)
            Destroy(t.gameObject);

        // Generate string fields
        foreach (string s in stringFieldNamesToGenerate)
            GenerateStringElement(s);

        // Generate float fields
        foreach (string s in floatFieldNamesToGenerate)
            GenerateFloatElement(s);

        // Generate angles
        if (generateAngles)
            foreach (Machine.Axis a in currMachine.Axes)
                GenerateAxisElement(a);
    }

    /* Private methods */

    #region Element Generators

    /// <summary>
    /// Generates handler that allows for modification of the corresponding axis field
    /// </summary>
    /// <param name="axisName">axis to create handler for</param>
    private void GenerateAxisElement(Machine.Axis a) {
        GameObject g = Instantiate(machineElementAxisPrefab, machineElementParent);
        g.transform.SetAsLastSibling();

        MachineAxisElement e = g.GetComponent<MachineAxisElement>();
        Debug.Assert(e != null, "Could not get MachineAxisElement from prefab!");

        e.InitializeElement(a);
    }

    /// <summary>
    /// Generates handler that allows for modification of the corresponding float field
    /// </summary>
    /// <param name="fieldName">Name of field to modify</param>
    private void GenerateFloatElement(string fieldName) {
        GameObject g = Instantiate(machineElementFloatPrefab, machineElementParent);
        g.transform.SetAsLastSibling();

        MachineFloatElement e = g.GetComponent<MachineFloatElement>();
        Debug.Assert(e != null, "Could not get MachineFloatElement from prefab!");

        e.InitializeElement(fieldName, currMachine);
    }

    /// <summary>
    /// Generates handler that allows for modification of the corresponding string field 
    /// </summary>
    /// <param name="fieldName">Name of field to modify</param>
    private void GenerateStringElement(string fieldName) {
        GameObject g = Instantiate(machineElementStringPrefab, machineElementParent);
        g.transform.SetAsLastSibling();

        MachineStringElement e = g.GetComponent<MachineStringElement>();
        Debug.Assert(e != null, "Could not get MachineStringElement from prefab!");

        e.InitializeElement(fieldName, currMachine);
    }

    #endregion
}
