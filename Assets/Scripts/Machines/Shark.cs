// System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

// NERVV
using NERVV;

public class Shark : Machine {
    #region Unity Methods
    /// <summary>Set component positions based on axes</summary>
    void Update() {
        if (Interpolation) {
            // Continually lerp towards final position
            float clampedLerp = Mathf.Clamp(BlendSpeed * Time.deltaTime, 0, 1);
            for (int i = 0; i < Axes.Count; i++)
                Axes[i].AxisTransform.localPosition = Vector3.Lerp(
                    Axes[i].AxisTransform.localPosition,
                    Axes[i].AxisVector3,
                    clampedLerp
                );
        } else {
            // Get latest correct axis angle
            for (int i = 0; i < Axes.Count && i < Axes.Count; i++)
                Axes[i].AxisTransform.localPosition = Axes[i].AxisVector3;
        }

        // DEBUG: Draw Position
        ForwardKinematics(Axes.ToArray());
    }
    #endregion
}
