using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doosan : Machine {

    [Header("Doosan Settings")]
    [Tooltip("Speed of lerping to correct position")]
    public float LerpSpeed = 10f;
    [Tooltip("Toggles lerping to correct position")]
    public bool Interpolation = true;

    [Tooltip("Sampling distance used to calculate gradient")]
    public float MaxDeltaTimeDelta;
    [Tooltip("Learning rate of gradient descent")]
    public float SpeedFactor;

    // Private Vars
    private Transform[] components;

    private void Awake() {
        // Init arrays
        components = new Transform[Axes.Count];

        // Recursively populate components with Transform references
        Transform t = transform;
        for (int i = 0; i < Axes.Count; i++) {                   // For each component
            for (int j = 0; j < t.childCount; j++) {            // Go through current transform
                if (t.GetChild(j).name == ("A" + (i + 1))) {    // If name matches
                    components[i] = (t = t.GetChild(j));        // Set new child and components
                    break;
                }
            }
        }

        // Safety checks
        for (int i = 0; i < Axes.Count; i++)
            Debug.Assert(components[i] != null,
                "Could not find component " + i + "!");
        if (LerpSpeed == 0)
            Debug.LogWarning("LerpSpeed is 0, will never move!");
        if (MaxSpeed == 0)
            Debug.LogWarning("MaxSpeed set to 0, will not be able to move!");
        if (SpeedFactor == 0 || MaxDeltaTimeDelta == 0)
            Debug.LogWarning("IK Learning rate or sampling distance invalid, will behave unpredictably!");
    }

    private void Start() {
        // Link to MachineManager
        if (MachineManager.Instance == null) {
            Debug.LogWarning("[Doosan] Could not find MachineManager!");
        } else {
            MachineManager.Instance.AddMachine(this);
        }
    }

    private void Update() {
        if (Interpolation) {
            // Continually lerp towards final position
            for (int i = 0; i < Axes.Count; i++)
                components[i].localRotation = Quaternion.Lerp(components[i].localRotation,
                                                                Quaternion.Euler(Axes[i].AxisVector3),
                                                                Mathf.Clamp(LerpSpeed * Time.deltaTime, 0, 1));
        } else {
            // Get latest correct axis angle
            for (int i = 0; i < Axes.Count; i++)
                components[i].localEulerAngles = Axes[i].AxisVector3;
        }

        // Debug
        ForwardKinematics(Axes);
    }

    #region Public Methods

    /// <summary>
    /// Returns the final location of the robotic arm using forward kinematics
    /// </summary>
    /// <param name="anglesToCalculate">Array of floats with angles to calculate</param>
    /// <returns>Vector3 of final position in world space</returns>
    public Vector3 ForwardKinematics(List<Axis> anglesToCalculate) {
        if (components.Length == 0)
            return Vector3.zero;

        Vector3 prevPoint = components[0].position;
        Quaternion rotation = Quaternion.identity;

        for (int i = 0; i < anglesToCalculate.Count - 1; i++) {
            rotation *= Quaternion.AngleAxis(Mathf.Repeat(anglesToCalculate[i].Value, 360), Axes[i].AxisVector3);
            Vector3 nextPoint = prevPoint + (rotation * components[i + 1].localPosition);
            Debug.DrawRay(prevPoint, rotation * components[i + 1].localPosition, Color.red);
            prevPoint = nextPoint;
        }

        return prevPoint;
    }

    /// <summary>
    /// When called, performs IK toward the target position
    /// </summary>
    /// <param name="target">Vector3 target position in worldspace</param>
    public override void InverseKinematics(Vector3 target) {
        for (int i = 0; i < Axes.Count; i++) {
            if (i == 10000 || i == 100000)
                continue;

            Debug.Log("Axis " + i + ": " + PartialGradient(target, Axes, i));
            Axes[i].Value -= Mathf.Clamp(
                SpeedFactor * PartialGradient(target, Axes, i),
                -MaxDeltaTimeDelta,
                MaxDeltaTimeDelta
            );
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Returns the gradient for a specific angleID
    /// </summary>
    /// <param name="target">Vector3 target location in worldspace</param>
    /// <param name="anglesToCalculate">Angles to calculate from</param>
    /// <param name="axisToCalculate">Angle to return gradient</param>
    /// <returns>Partial gradient as float</returns>
    private float PartialGradient(Vector3 target, List<Axis> anglesToCalculate, int axisToCalculate) {

        // Safety checks
        Debug.Assert(axisToCalculate >= 0 && axisToCalculate < anglesToCalculate.Count,
            "[Doosan] Invalid axisID: " + axisToCalculate);
        Debug.Assert(anglesToCalculate.Count == Axes.Count,
            "Invalid number of angles passed!");

        
        // Gradient : [F(x+Time per frame) - F(axisToCalculate)] / h
        float f_x = Vector3.SqrMagnitude(target - ForwardKinematics(anglesToCalculate));

        List<Axis> tempAngles = anglesToCalculate;
        tempAngles[axisToCalculate].Value += Time.deltaTime;
        float f_x_plus_d = Vector3.SqrMagnitude(target - ForwardKinematics(tempAngles));

        return f_x_plus_d - f_x;
    }

    #endregion
}
