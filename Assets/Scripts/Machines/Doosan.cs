// System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

// NERVV
using NERVV;

public class Doosan : Machine {
    #region Unity Methods
    void Update() {
        if (Interpolation) {
            // Continually lerp towards final position
            for (int i = 0; i < Axes.Count; i++)
                Axes[i].AxisTransform.localRotation = Quaternion.Lerp(
                    Axes[i].AxisTransform.localRotation,
                    Quaternion.Euler(Axes[i].AxisVector3),
                    Mathf.Clamp(BlendSpeed * Time.deltaTime, 0, 1)
                );
        } else {
            // Get latest correct axis angle
            for (int i = 0; i < Axes.Count; i++)
                Axes[i].AxisTransform.localEulerAngles = Axes[i].AxisVector3;
        }

        // DEBUG: Draw forward kinematics every frame
        ForwardKinematics(Axes.ToArray());
    }
    #endregion
}
