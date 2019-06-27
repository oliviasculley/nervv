using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

using TMPro;

public class Menu_MachineDetail : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI machineTitle;
    public Transform machineElementParent;
    public GameObject machineElementPrefab; // Changeable machine element 

    private void Awake() {
        Debug.Assert(machineTitle != null,
            "[Menu: Machine Detail] Could not get ref to machine title!");
        Debug.Assert(machineElementPrefab != null,
            "[Menu: Machine Detail] Could not get machine element prefab!");
        Debug.Assert(machineElementParent != null,
            "[Menu: Machine Detail] Could not get machine element parent!");
    }

    /* Public methods */

    public void DisplayMachine(Machine m) {
        GenerateElement(m, "name");
        GenerateElement(m, "uuid");
        GenerateElement(m, "model");
        GenerateElement(m, "manufacturer");
    }

    public void ChangeElementValue(Machine m, string fieldToSet, string newValue) {
        typeof(Machine).GetField(fieldToSet).SetValue(m, newValue);
    }

    private void GenerateElement(Machine m, string elementName) {
        GameObject g = Instantiate(machineElementPrefab, machineElementParent);
        g.transform.SetAsLastSibling();
        
        // Get Button components
        TextMeshProUGUI TMP = g.transform.Find("ElementName").GetComponent<TextMeshProUGUI>();
        TMP_InputField i = g.transform.Find("InputField (TMP)").GetComponent<TMP_InputField>();
        Debug.Assert(TMP != null && i != null);

        // Capitalize name
        TMP.text = char.ToUpper(elementName[0]).ToString();
        if (elementName.Length >= 2)
            TMP.text += elementName.Substring(1);
        TMP.text += ": ";
        i.text = elementName;

        // Add onClick listener to button
        Machine machine = m;    // Push info to stack so button will be correct
        string s = elementName;
        i.onValueChanged.AddListener(delegate { ChangeElementValue(machine, elementName, i.text); });
    }
}
