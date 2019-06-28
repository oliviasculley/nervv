using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

using TMPro;

public class Menu_MachineDetail : MonoBehaviour
{
    [Header("Properties")]
    public Machine currMachine;

    [Header("Settings")]
    public string[] stringFieldNamesToGenerate; // String fields to generate handlers for
    public string[] floatFieldNamesToGenerate;  // Float fields to generate handlers for
    public bool generateAngles;                 // Generate angle modifiers
    public float floatDelta;                    // Delta to multiply by float

    [Header("References")]
    public TextMeshProUGUI machineTitle;
    public Transform machineElementParent;
    public GameObject machineElementStringPrefab, machineElementFloatPrefab; // Changeable machine element 

    private void Awake() {
        // Get references
        Debug.Assert(machineTitle != null,
            "[Menu: Machine Detail] Could not get ref to machine title!");
        Debug.Assert(machineElementStringPrefab != null && machineElementFloatPrefab != null,
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
            foreach (Machine.Axis a in currMachine.axes)
                GenerateAxisElement(a);
    }

    public void ChangeStringElementValue(string fieldToSet, string newValue) {
        typeof(Machine).GetField(fieldToSet).SetValue(currMachine, newValue);
    }

    public void ChangeFloatElementValue(string fieldToSet, float newValue)
    {
        typeof(Machine).GetField(fieldToSet).SetValue(currMachine, newValue);
    }

    /* Private methods */

    /// <summary>
    /// Generates handler that allows for modification of the corresponding axis field
    /// </summary>
    /// <param name="axisName">axis to create handler for</param>
    private void GenerateAxisElement(Machine.Axis a) {
        GameObject g = Instantiate(machineElementStringPrefab, machineElementParent);
        g.transform.SetAsLastSibling();

        // Get Button components
        TextMeshProUGUI TMP = g.transform.Find("ElementName").GetComponent<TextMeshProUGUI>();
        TMP_InputField i = g.transform.Find("InputField (TMP)").GetComponent<TMP_InputField>();
        Debug.Assert(TMP != null && i != null);

        // Set name
        TMP.text = a.GetName() + ": ";

        // Add onClick listener to button
        Machine machine = currMachine;    // Push info to stack so button will be correct
        Machine.Axis axis = a;
        i.onValueChanged.AddListener(delegate { ChangeStringAxisValue(a, i.text); });
    }

    /// <summary>
    /// Generates handler that allows for modification of the corresponding float field
    /// </summary>
    /// <param name="fieldName">Name of field to modify</param>
    private void GenerateFloatElement(string fieldName) {
        GameObject g = Instantiate(machineElementStringPrefab, machineElementParent);
        g.transform.SetAsLastSibling();

        // Get Button components
        TextMeshProUGUI TMP = g.transform.Find("ElementName").GetComponent<TextMeshProUGUI>();
        TMP_InputField i = g.transform.Find("InputField (TMP)").GetComponent<TMP_InputField>();
        Debug.Assert(TMP != null && i != null);

        // Capitalize name
        TMP.text = char.ToUpper(fieldElement[0]).ToString();
        if (fieldElement.Length >= 2)
            TMP.text += fieldElement.Substring(1);
        TMP.text += ": ";
        i.text = fieldElement;

        // Add onClick listener to button
        Machine machine = currMachine;    // Push info to stack so button will be correct
        string s = fieldElement;
        i.onValueChanged.AddListener(delegate { ChangeStringElementValue(fieldElement, i.text); });
    }

    /// <summary>
    /// Generates handler that allows for modification of the corresponding string field 
    /// </summary>
    /// <param name="fieldName">Name of field to modify</param>
    private void GenerateStringElement(string fieldName) {
        GameObject g = Instantiate(machineElementStringPrefab, machineElementParent);
        g.transform.SetAsLastSibling();
        
        // Get Button components
        TextMeshProUGUI TMP = g.transform.Find("ElementName").GetComponent<TextMeshProUGUI>();
        TMP_InputField i = g.transform.Find("InputField (TMP)").GetComponent<TMP_InputField>();
        Debug.Assert(TMP != null && i != null);

        // Capitalize name
        TMP.text = char.ToUpper(fieldName[0]).ToString();
        if (fieldName.Length >= 2)
            TMP.text += fieldName.Substring(1);
        TMP.text += ": ";
        i.text = fieldName;

        // Add onClick listener to button
        Machine machine = currMachine;    // Push info to stack so button will be correct
        string s = fieldName;
        i.onValueChanged.AddListener(delegate { ChangeStringElementValue(fieldName, i.text); });
    }

    /// <summary>
    /// Returns string with capitalized first letter
    /// </summary>
    /// <param name="input">string to be capitalized</param>
    /// <returns>capitalized string</returns>
    private string CapitalizeFirstLetter(string input) {

    }
}
