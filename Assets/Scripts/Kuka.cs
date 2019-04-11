using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kuka : Machine
{
    // Private Vars
    [SerializeField] private Transform[] components;

    private void Awake() {
        // Init arrays
        angles = new double[AxisCount];
        components = new Transform[AxisCount];

        // Recursively populate components with Transform references
        Transform t = this.transform;
        for (int i = 0; i < AxisCount; i++) {                   // For each component
            for (int j = 0; j < t.childCount; j++) {            // Go through current transform
                if (t.GetChild(j).name == ("A" + (i + 1))) {    // If name matches
                    components[i] = (t = t.GetChild(j));        // Set new child and components
                    break;
                }
            }
        }

        // Safety check all components
        for (int i = 0; i < AxisCount; i++)
            Debug.Assert(components[i] != null, "Could not find component " + i + "!");
    }

    private void Start() {
        // Link to MTConnect updates
        MTConnect.mtc.AddMachine(this);
    }

    private void Update() {
        // Continually lerp towards final position
        Vector3 vel = Vector3.zero;
        for (int i = 0; i < AxisCount; i++)
            components[i].localEulerAngles = Vector3.SmoothDamp(components[i].localEulerAngles,
                                                                GetAxis(i),
                                                                ref vel,
                                                                maxSpeed * Time.deltaTime);
    }

    /* Public Methods */

    /// <summary>
    /// Sets the angle of a certain axis
    /// </summary>
    /// <param name="s">Name of the axis to set</param>
    public override void SetAxisAngle(string axisName, double angle) {
        // String Err checking
        if (string.IsNullOrEmpty(axisName) ||               // Axis name cannot be null, empty
            axisName.Length < 2 ||                          // Must be at least 2 chars
            (axisName[0] != 'a' && axisName[0] != 'A')) {   // Kuka must start with [Aa]
            Debug.LogWarning("[Kuka] Invalid axisName: \"" + axisName + "\"");
            return;
        }

        // Parse 2nd char to axis ID
        if (!int.TryParse(axisName[1].ToString(), out int axis))
            Debug.LogWarning("[Kuka] Could not parse axisName: \"" + axisName + "\"");

        // Safety check axisID
        if (axis < 0 || axis > AxisCount) {
            Debug.LogWarning("[Kuka] Invalid axisID to set: " + axis);
            return;
        }

        // Set axis angle
        angles[axis - 1] = angle;
    }

    /// <summary>
    /// Returns the Vector3 for the associated axis
    /// </summary>
    /// <param name="axisID">ID of the axis to return Vector3</param>
    /// <returns></returns>
    public override Vector3 GetAxis(int axisID) {
        // Switch based on axisID
        switch (axisID) {

            // Y rotation
            case 0:
            case 3:
            case 5:
                return new Vector3(0, (float) angles[axisID], 0);

            // Z rotation
            case 1:
                return new Vector3(0, 0, (float) angles[axisID] + 90f);
            case 2:
                return new Vector3(0, 0, (float) angles[axisID]);

            // X rotation
            case 4:
                return new Vector3((float) angles[axisID], -90f, 0);

            default:
                Debug.LogWarning("[Kuka] Could not find axisID: " + axisID);
                return Vector3.zero;
        }
    }

    /* Private Methods */
}
