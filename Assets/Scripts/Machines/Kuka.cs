// Unity Engine
using UnityEngine;
using MTConnectVR;

public class Kuka : Machine {

    #region Unity Methods

    private void Awake() {
        // Init arrays
        components = new Transform[Axes.Count];

        // Recursively populate components with Transform references
        Transform t = transform;
        for (int i = 0; i < Axes.Count; i++)                    // For each component
            for (int j = 0; j < t.childCount; j++)              // Go through current transform
                if (t.GetChild(j).name == ("A" + (i + 1))) {    // If name matches
                    components[i] = (t = t.GetChild(j));        // Set new child and components
                    break;
                }
    }

    protected override void Start() {
        base.Start();
        if (LerpSpeed == 0)
            Debug.LogWarning("LerpSpeed is 0, will never move!");
    }

    private void Update() {
        if (Interpolation) {
            // Continually lerp towards final position
            for (int i = 0; i < Axes.Count; i++)
                components[i].localRotation = Quaternion.Lerp(
                    components[i].localRotation,
                    Quaternion.Euler(Axes[i].AxisVector3),
                    Mathf.Clamp(LerpSpeed * Time.deltaTime, 0, 1)
                );
        } else {
            // Get latest correct axis angle
            for (int i = 0; i < Axes.Count; i++)
                components[i].localEulerAngles = Axes[i].AxisVector3;
        }

        // DEBUG: Draw forward kinematics every frame
        ForwardKinematics(Axes.ToArray());
    }

    #endregion

}
