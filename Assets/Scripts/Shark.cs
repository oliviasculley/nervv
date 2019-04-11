using UnityEngine;

public class Shark : Machine
{
    // Private Vars
    [SerializeField] private Transform head;  // Location of Shark Head

    private void Awake() {
        // Init arrays
        angles = new double[AxisCount];
        head = transform.Find("Head");

        Debug.Assert(head != null, "Could not find head!");
    }

    private void Start() {
        // Link to MTConnect updates
        MTConnect.mtc.AddMachine(this);
    }

    private void Update() {
        // Continually lerp towards final position
        Vector3 vel = Vector3.zero;
        head.localPosition  = Vector3.SmoothDamp(   head.localPosition,
                                                    GetAxis(0),
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
            Debug.LogWarning("[Shark] Invalid axisName: \"" + axisName + "\"");
            return;
        }

        // Parse 2nd char to axis ID
        if (!int.TryParse(axisName[1].ToString(), out int axis))
            Debug.LogWarning("[Shark] Could not parse axisName: \"" + axisName + "\"");

        // Safety check axisID
        if (axis < 0 || axis > AxisCount) {
            Debug.LogWarning("[Shark] Invalid axisID to set: " + axis);
            return;
        }

        angles[axis - 1] = angle;
    }

    /// <summary>
    /// Returns the Vector3 for the associated axis
    /// </summary>
    /// <param name="axisID">ID of the axis to return Vector3</param>
    /// <returns></returns>
    public override Vector3 GetAxis(int axisID) {
        if (axisID != 0) {
            Debug.LogError("[Shark] Invalid axisID: " + axisID);
            return Vector3.zero;
        }

        return new Vector3((float) angles[0], (float) angles[1], (float) angles[2]);
    }
}
