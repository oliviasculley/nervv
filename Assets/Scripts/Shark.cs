using UnityEngine;

public class Shark : Machine
{
    [Header("Shark Settings")]
    public float lerpSpeed;             // Speed to lerp to correct position
    public bool interpolation = true;   // Toggles lerping to final position

    // Private Vars
    [SerializeField] private Transform head;  // Location of Shark Head

    // Constants
    private readonly float scale = 0.0254f; // inches to mm

    private void Awake() {
        // Init arrays
        angles = new double[axisCount];
        head = transform.Find("Head");

        // Safety checks
        Debug.Assert(head != null, "Could not find head!");
        if (lerpSpeed == 0)
            Debug.LogWarning("Lerp speed is 0, will never go to final position!");
        if (maxSpeed == 0)
            Debug.LogWarning("MaxSpeed set to 0, will not be able to move!");
    }

    private void Start() {
        // Link to MTConnect updates
        MTConnect.mtc.AddMachine(this);
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
        if (axis < 0 || axis > axisCount) {
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

        return new Vector3(-(float)angles[1], (float) angles[2], (float)angles[0]) * scale;
    }
}
