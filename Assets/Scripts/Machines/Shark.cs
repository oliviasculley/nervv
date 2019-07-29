using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MTConnectVR;

public class Shark : Machine {

    #region Unity Methods

    /// <summary> Initialize components </summary>
    protected void Awake() {
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

    private void Update() {
        if (Interpolation) {
            // Continually lerp towards final position
            float clampedLerp = Mathf.Clamp(LerpSpeed * Time.deltaTime, 0, 1);
            for (int i = 0; i < components.Length && i < Axes.Count; i++)
                components[i].localPosition = Vector3.Lerp(
                    components[i].localPosition,
                    Axes[i].AxisVector3,
                    clampedLerp
                );
        } else {
            // Get latest correct axis angle
            for (int i = 0; i < components.Length && i < Axes.Count; i++)
                components[i].localPosition = Axes[i].AxisVector3;
        }

        // DEBUG: Draw Position
        ForwardKinematics(Axes.ToArray());
    }

    #endregion
}
