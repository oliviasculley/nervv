using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kuka : Machine
{
    [Header("Kuka Settings")]
        [Tooltip("Speed of lerping to correct position")]
        public float LerpSpeed = 10f;
        [Tooltip("Toggles lerping to correct position")]
        public bool Interpolation = true;
        [Range(0.00001f, 1)]
        [Tooltip("Sampling distance used to calculate gradient")]
        public float SamplingDistance;
        [Range(0, 50000)]
        [Tooltip("Learning rate of gradient descent")]
        public float LearningRate;

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
            Debug.Assert(components[i] != null, "Could not find component " + i + "!");
        if (LerpSpeed == 0)
            Debug.LogWarning("LerpSpeed is 0, will never move!");
        if (MaxSpeed == 0)
            Debug.LogWarning("MaxSpeed set to 0, will not be able to move!");
    }

    private void Start() {
        // Link to MachineManager
        if (MachineManager.Instance == null) {
            Debug.LogWarning("[Kuka] Could not find MachineManager!");
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
            if (i == 3 || i == 5)
                continue;

            Axes[i].Value = (
                Axes[i].Value - LearningRate
                * PartialGradient(target, Axes, i)
                * Time.deltaTime
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
    /// <returns>Float with partial gradient</returns>
    private float PartialGradient(Vector3 target, List<Axis> anglesToCalculate, int axisToCalculate) {

        // Safety checks
        Debug.Assert(axisToCalculate >= 0 && axisToCalculate < anglesToCalculate.Count,
            "[Kuka] Invalid axisID: " + axisToCalculate);
        Debug.Assert(anglesToCalculate.Count == Axes.Count,
            "Invalid number of angles passed!");

        float cachedAngle = anglesToCalculate[axisToCalculate].Value;
        // Gradient : [F(x+SamplingDistance) - F(axisToCalculate)] / h
        float f_x = Vector3.SqrMagnitude(target - ForwardKinematics(anglesToCalculate));

        anglesToCalculate[axisToCalculate].Value = (anglesToCalculate[axisToCalculate].Value + SamplingDistance);
        float f_x_plus_d = Vector3.SqrMagnitude(target - ForwardKinematics(anglesToCalculate));
        anglesToCalculate[axisToCalculate].Value = (cachedAngle);

        return (f_x_plus_d - f_x) / SamplingDistance;
    }

    #endregion
}
