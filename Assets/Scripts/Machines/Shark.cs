using UnityEngine;

public class Shark : Machine
{
    [Header("Shark Settings")]
    public float lerpSpeed = 10f;           // Speed to lerp to correct position
    public bool interpolation = true;       // Toggles lerping to final position
    public float[] minAngles, maxAngles;    // Min and max angles for each axis

    // Private Vars
    private Transform head;  // Location of Shark Head

    // Constants
    private readonly float scaleFactor = 0.0254f; // inches to mm

    private void Awake() {
        // Init arrays
        angles = new float[axisCount];
        head = transform.Find("Head");

        // Safety checks
        Debug.Assert(head != null, "Could not find head!");
        Debug.Assert(minAngles.Length == axisCount, "MinAngles count does not equal number of axis!");
        Debug.Assert(maxAngles.Length == axisCount, "MaxAngles count does not equal number of axis!");
        if (lerpSpeed == 0)
            Debug.LogWarning("Lerp speed is 0, will never go to final position!");
        if (maxSpeed == 0)
            Debug.LogWarning("MaxSpeed set to 0, will not be able to move!");
    }

    private void Start() {
        // Add to MachineManager
        if (MachineManager.Instance == null) {
            Debug.LogWarning("[Shark] Could not find MachineManager!");
        } else {
            MachineManager.Instance.AddMachine(this);
        }
    }

    private void Update() {
        if (interpolation) {
            // Continually lerp towards final position
            Vector3 vel = Vector3.zero;
            head.localPosition = Vector3.Lerp(  head.localPosition,
                                                GetAxis(0),
                                                lerpSpeed * Time.deltaTime);
        } else {
            // Get latest correct axis angle
            head.localPosition = GetAxis(0);
        }
    }

    /* Public Methods */

    /// <summary>
    /// Sets the angle of a certain axis
    /// </summary>
    /// <param name="s">Name of the axis to set (1 indexed)</param>
    public override void SetAxisAngle(string axisName, float angle) {
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
        if (axis < 0 || axis > axisCount) {
            Debug.LogWarning("[Shark] Invalid axisID to set: " + axis);
            return;
        } else {
            // Decrement to 0 index axisID
            axis -= 1;
        }

        // Set angle with safety min and max angles
        if (minAngles[axis] == 0 && maxAngles[axis] == 0) {
            // No min/max angle restriction
            angles[axis] = angle % 360f;
        } else {
            angles[axis] = Mathf.Clamp(angle, minAngles[axis], maxAngles[axis]);
        }
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

        return new Vector3(-angles[1], angles[2], angles[0]) * scaleFactor;
    }
}
