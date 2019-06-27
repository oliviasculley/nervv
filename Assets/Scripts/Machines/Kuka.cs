using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kuka : Machine
{
    [Header("Kuka Settings")]
    public float lerpSpeed = 10f;           // Speed of lerping to correct position
    public bool interpolation = true;       // Toggles lerping to correct position
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
        axes = new List<Axis>();
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
        for (int i = 0; i < axisCount; i++)
            Debug.Assert(components[i] != null, "Could not find component " + i + "!");
        if (lerpSpeed == 0)
            Debug.LogWarning("LerpSpeed is 0, will never move!");
        if (maxSpeed == 0)
            Debug.LogWarning("MaxSpeed set to 0, will not be able to move!");
    }

    private void Start() {
        // Link to MachineManager
        if (MachineManager.Instance == null) {
            Debug.LogWarning("[Kuka] Could not find MachineManager!");
        } else {
            MachineManager.Instance.AddMachine(this);
        }

        // Initialize Axes
        for (int i = 1; i <= 6; i++)
            axes.Add(new Axis("a" + i.ToString(), "A" + i.ToString(), 0, AxisType.Rotary));
        
        // DEBUG: Random colors
        randomColors = new Color[axisCount];
        for (int i = 0; i < axisCount; i++)
            randomColors[i] = Random.ColorHSV();
    }

    private void Update() {
        if (interpolation) {
            // Continually lerp towards final position
            for (int i = 0; i < axes.Count; i++)
                components[i].localRotation = Quaternion.Lerp(components[i].localRotation,
                                                                Quaternion.Euler(GetAxisVector3(axes[i])),
                                                                Mathf.Clamp(lerpSpeed * Time.deltaTime, 0, 1));
        } else {
            // Get latest correct axis angle
            for (int i = 0; i < axisCount; i++)
                components[i].localEulerAngles = GetAxisVector3(axes[i]);
        }

        // Debug
        ForwardKinematics(axes);
    }

    /* Public Methods */

    /// <summary>
    /// Sets the value of a certain axis by axis' ID
    /// </summary>
    /// <param name="axisID">Axis ID (MTConnect string identifier) to set</param>
    /// <param name="value">Value of axis to set</param>
    public override void SetAxisValue(string axisID, float value) {

        // Get Axis with axisID
        Axis found;
        if ((found = axes.Find(x => x.GetID() == axisID)) == null)
            return;

        // Axis convention conversions
        switch (found.GetID()) {

            // X rotation
            case "a2":
                found.SetValue(NormalizeAngle(-(value + 90f)));
                break;
            case "a3":
                found.SetValue(NormalizeAngle(-(value - 90f)));
                break;
            case "a5":
                found.SetValue(NormalizeAngle(-value));
                break;

            // Y rotation
            case "a1":
                found.SetValue(NormalizeAngle(value));
                break;

            // Z rotation
            case "a4":
                found.SetValue(NormalizeAngle(-value));
                break;
            case "a6":
                found.SetValue(NormalizeAngle(value));
                break;

            default:
                Debug.LogWarning("[Kuka] Could not find axis with ID: " + found.GetID());
                break;
        }
    }

    /// <summary>
    /// Returns the Vector3 for the associated axis in local space
    /// </summary>
    /// <param name="axis">Axis to return Vector3</param>
    /// <returns>Vector3 of rotation for selected axis in local space</returns>
    public override Vector3 GetAxisVector3(Axis axis) {
        // Switch based on axisID
        switch (axis.GetID()) {

            // X rotation
            case "a2":
                return new Vector3(axis.GetValue(), 0, 0);
            case "a3":
            case "a5":
                return new Vector3(axis.GetValue(), 0, 0);

            // Y rotation
            case "a1":
                return new Vector3(0, axis.GetValue(), 0);

            // Z rotation
            case "a4":
            case "a6":
                return new Vector3(0, 0, axis.GetValue());

            default:
                Debug.LogWarning("[Kuka] Could not find axis for ID: " + axis.GetID());
                return Vector3.zero;
        }
    }

    /// <summary>
    /// Returns the final location of the robotic arm using forward kinematics
    /// </summary>
    /// <param name="anglesToCalculate">Array of floats with angles to calculate</param>
    /// <returns>Vector3 of final position in world space</returns>
    public Vector3 ForwardKinematics(List<Axis> anglesToCalculate) {
        Vector3 prevPoint = components[0].position;
        Quaternion rotation = Quaternion.identity;

        for (int i = 0; i < anglesToCalculate.Count - 1; i++) {
            rotation *= Quaternion.AngleAxis(NormalizeAngle(anglesToCalculate[i].GetValue()), GetAxisVector3(axes[i]));
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
        for (int i = 0; i < axes.Count; i++) {
            if (i == 3 || i == 5)
                continue;

            axes[i].SetValue(NormalizeAngle( axes[i].GetValue() - learningRate
                                        * PartialGradient(target, axes, i)
                                        * Time.deltaTime));
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
    /// <param name="axisToCalculate">Angle to return gradient</param>
    /// <returns></returns>
    private float PartialGradient(Vector3 target, List<Axis> anglesToCalculate, int axisToCalculate) {

        // Safety checks
        Debug.Assert(axisToCalculate >= 0 && axisToCalculate < anglesToCalculate.Count,
            "[Kuka] Invalid axisID: " + axisToCalculate);
        Debug.Assert(anglesToCalculate.Count == axisCount,
            "Invalid number of angles passed!");

        float cachedAngle = anglesToCalculate[axisToCalculate].GetValue();
        // Gradient : [F(x+SamplingDistance) - F(axisToCalculate)] / h
        float f_x = Vector3.SqrMagnitude(target - ForwardKinematics(anglesToCalculate));

        anglesToCalculate[axisToCalculate].SetValue(anglesToCalculate[axisToCalculate].GetValue() + samplingDistance);
        float f_x_plus_d = Vector3.SqrMagnitude(target - ForwardKinematics(anglesToCalculate));
        anglesToCalculate[axisToCalculate].SetValue(cachedAngle);

        return (f_x_plus_d - f_x) / samplingDistance;
    }
}
