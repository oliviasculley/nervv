using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kuka : Machine
{
    [Header("Kuka Settings")]
    public float lerpSpeed = 10f;           // Speed of lerping to correct position
    public bool interpolation = true;       // Toggles lerping to correct position
    public float[] minAngles, maxAngles;    // Min and max angles for each axis
    [Range(0.00001f, 1)]
    public float samplingDistance;   // Sampling distance used to calculate gradient
    [Range(0, 50000)]
    public float learningRate;          // Learning rate of gradient descent

    // Private Vars
    private Transform[] components;

    // DEBUG
    private Color[] randomColors;

    private void Awake() {
        // Init arrays
        angles = new float[axisCount];
        components = new Transform[axisCount];

        // Recursively populate components with Transform references
        Transform t = this.transform;
        for (int i = 0; i < axisCount; i++) {                   // For each component
            for (int j = 0; j < t.childCount; j++) {            // Go through current transform
                if (t.GetChild(j).name == ("A" + (i + 1))) {    // If name matches
                    components[i] = (t = t.GetChild(j));        // Set new child and components
                    break;
                }
            }
        }

        // Safety checks
        Debug.Assert(minAngles.Length == axisCount, "MinAngles count does not equal number of axis!");
        Debug.Assert(maxAngles.Length == axisCount, "MaxAngles count does not equal number of axis!");
        for (int i = 0; i < axisCount; i++)
            Debug.Assert(components[i] != null, "Could not find component " + i + "!");
        if (lerpSpeed == 0)
            Debug.LogWarning("LerpSpeed is 0, will never move!");
        if (maxSpeed == 0)
            Debug.LogWarning("MaxSpeed set to 0, will not be able to move!");
    }

    private void Start() {
        // Link to MTConnect updates
        if (MTConnect.mtc == null) {
            Debug.LogWarning("[Kuka] Could not find MTConnect!");
        } else {
            MTConnect.mtc.AddMachine(this);
        }
        
        // DEBUG: Random colors
        randomColors = new Color[axisCount];
        for (int i = 0; i < axisCount; i++)
            randomColors[i] = Random.ColorHSV();
    }

    private void Update() {
        if (interpolation) {
            // Continually lerp towards final position
            for (int i = 0; i < axisCount; i++)
                components[i].localRotation = Quaternion.Lerp(components[i].localRotation,
                                                                Quaternion.Euler(GetAxis(i)),
                                                                Mathf.Clamp(lerpSpeed * Time.deltaTime, 0, 1));
        } else {
            // Get latest correct axis angle
            for (int i = 0; i < axisCount; i++)
                components[i].localEulerAngles = GetAxis(i);
        }

        // Debug
        ForwardKinematics(angles);
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
            Debug.LogWarning("[Kuka] Invalid axisName: \"" + axisName + "\"");
            return;
        }

        // Parse 2nd char to axis ID
        if (!int.TryParse(axisName[1].ToString(), out int axisID))
            Debug.LogWarning("[Kuka] Could not parse axisName: \"" + axisName + "\"");

        // Safety check axisID
        // Decrement to 0-indexed axisID
        axisID -= 1;
        if (axisID < 0 || axisID >= axisCount) {
            Debug.LogWarning("[Kuka] Invalid axisID to set: " + axisID);
            return;
        }

        // Set axis angle
        if (minAngles[axisID] == 0 && maxAngles[axisID] == 0) {
            // No min/max angle restriction
            angles[axisID] = angle;
        } else {
            angles[axisID] = Mathf.Clamp(angle, minAngles[axisID], maxAngles[axisID]);
        }

        // Axis convention conversions
        switch (axisID) {

            // X rotation
            case 1:
                angles[axisID] = NormalizeAngle(-(angles[axisID] + 90f));
                break;
            case 2:
                angles[axisID] = NormalizeAngle(-(angles[axisID] - 90));
                break;
            case 4:
                angles[axisID] = NormalizeAngle(angles[axisID]);
                break;

            // Y rotation
            case 0:
                angles[axisID] = NormalizeAngle(angles[axisID]);
                break;

            // Z rotation
            case 3:
            case 5:
                angles[axisID] = NormalizeAngle(-angles[axisID]);
                break;

            default:
                Debug.LogWarning("[Kuka] Could not find axisID: " + axisID);
                break;
        }
    }

    /// <summary>
    /// Returns the Vector3 for the associated axis in local space
    /// </summary>
    /// <param name="axisID">ID of the axis to return Vector3</param>
    /// <returns>Vector3 of rotation for selected axis in local space</returns>
    public override Vector3 GetAxis(int axisID) {
        // Switch based on axisID
        switch (axisID) {

            // X rotation
            case 1:
            case 2:
            case 4:
                return new Vector3(angles[axisID], 0, 0);

            // Y rotation
            case 0:
                return new Vector3(0, angles[axisID], 0);

            // Z rotation
            case 3:
            case 5:
                return new Vector3(0, 0, angles[axisID]);

            default:
                Debug.LogWarning("[Kuka] Could not find axisID: " + axisID);
                return Vector3.zero;
        }
    }

    /// <summary>
    /// Returns the final location of the robotic arm using forward kinematics
    /// </summary>
    /// <param name="anglesToCalculate">Array of floats with angles to calculate</param>
    /// <returns>Vector3 of final position in world space</returns>
    public Vector3 ForwardKinematics(float[] anglesToCalculate) {
        Vector3 prevPoint = components[0].position;
        Quaternion rotation = Quaternion.identity;

        for (int i = 0; i < axisCount - 1; i++) {
            rotation *= Quaternion.AngleAxis(NormalizeAngle(anglesToCalculate[i]), GetAxis(i));
            Vector3 nextPoint = prevPoint + (rotation * components[i + 1].localPosition);
            Debug.DrawRay(prevPoint, rotation * components[i + 1].localPosition, randomColors[i]);
            prevPoint = nextPoint;
        }

        return prevPoint;
    }

    /// <summary>
    /// When called, performs IK toward the target position
    /// </summary>
    /// <param name="target">Vector3 target position in worldspace</param>
    public void InverseKinematics(Vector3 target) {
        for (int i = 0; i < axisCount; i++) {
            if (i == 3 || i == 5)
                continue;

            angles[i] = NormalizeAngle( angles[i] - learningRate
                                        * PartialGradient(transform.InverseTransformPoint(target), angles, i)
                                        * Time.deltaTime);
        }
    }

    /* Private Methods */

    /// <summary>
    /// Ensures that angle will always be between 0-360
    /// </summary>
    /// <param name="angle">Angle in degrees</param>
    /// <returns>Equivalent angle in degrees between 0-360</returns>
    private float NormalizeAngle(float angle) {
        return ((angle %= 360) < 0) ? angle + 360 : angle;
    }

    /// <summary>
    /// Returns the gradient for a specific angleID
    /// </summary>
    /// <param name="target">Vector3 target location in worldspace</param>
    /// <param name="anglesToCalculate">Angles to calculate from</param>
    /// <param name="angleID">Angle to return gradient</param>
    /// <returns></returns>
    private float PartialGradient(Vector3 target, float[] anglesToCalculate, int axisID) {

        // Safety checks
        Debug.Assert(axisID >= 0 && axisID < axisCount, "[Kuka] Invalid axisID: " + axisID);
        Debug.Assert(anglesToCalculate.Length == axisCount, "Invalid number of angles passed!");

        float cachedAngle = anglesToCalculate[axisID];
        // Gradient : [F(x+SamplingDistance) - F(x)] / h
        float f_x = Vector3.SqrMagnitude(target - ForwardKinematics(anglesToCalculate));

        anglesToCalculate[axisID] += samplingDistance;
        float f_x_plus_d = Vector3.SqrMagnitude(target - ForwardKinematics(anglesToCalculate));
        anglesToCalculate[axisID] = cachedAngle;

        return (f_x_plus_d - f_x) / samplingDistance;
    }
}
