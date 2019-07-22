using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MTConnectVR;

public class Shark : Machine {

    // Private Vars
    [Tooltip("Location of Shark components")]
    private Transform head, holder, base_y;

    // Constants
    private readonly float scaleFactor = 0.0254f; // inches to mm

    /// <summary>
    /// Initialize components
    /// </summary>
    protected void Awake() {
        head = transform.Find("A1/A2/A3");
        holder = transform.Find("A1/A2");
        base_y = transform.Find("A1");

        // Safety checks
        Debug.Assert(head != null, "Could not find head!");
        Debug.Assert(holder != null, "Could not find holder!");
        Debug.Assert(base_y != null, "Could not find base_y!");
        if (LerpSpeed == 0)
            Debug.LogWarning("Lerp speed is 0, will never go to final position!");
    }

    private void Update() {
        if (Interpolation) {
            // Continually lerp towards final position
            Vector3 vel = Vector3.zero;
            float clampedLerp = Mathf.Clamp(LerpSpeed * Time.deltaTime, 0, 1);

            head.localPosition = Vector3.Lerp(
                head.localPosition,
                Axes[0].AxisVector3,
                clampedLerp
            );
            holder.localPosition = Vector3.Lerp(
                holder.localPosition,
                Axes[1].AxisVector3,
                clampedLerp
            );
            base_y.localPosition = Vector3.Lerp(
                base_y.localPosition,
                Axes[2].AxisVector3,
                clampedLerp
            );
        } else {
            // Get latest correct axis angle
            head.localPosition = Axes[2].AxisVector3;
            holder.localPosition = Axes[1].AxisVector3;
            base_y.localPosition = Axes[0].AxisVector3;
        }
    }
}
