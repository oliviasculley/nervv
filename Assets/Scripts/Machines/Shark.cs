using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shark : Machine
{
    [Header("Shark Settings")]
    public float LerpSpeed = 10f;           // Speed to lerp to correct position
    public bool Interpolation = true;       // Toggles lerping to final position

    // Private Vars
    private Transform head,holder,base_y;  // Location of Shark Head

    // Constants
    private readonly float scaleFactor = 0.0254f; // inches to mm

    private void Awake() {
        // Init arrays
        head = transform.Find("A1/A2/A3");
	    holder = transform.Find("A1/A2");
	    base_y = transform.Find("A1");

        // Safety checks
        Debug.Assert(head != null, "Could not find head!");
        Debug.Assert(holder != null, "Could not find holder!");
        Debug.Assert(base_y != null, "Could not find base_y!");

        if (LerpSpeed == 0)
            Debug.LogWarning("Lerp speed is 0, will never go to final position!");
        if (MaxSpeed == 0)
            Debug.LogWarning("MaxSpeed set to 0, will not be able to move!");
    }

    private void Start() {
        // Add to MachineManager
        if (MachineManager.Instance == null) {
            Debug.LogWarning("[Shark] Could not find MachineManager!");
        } else {
            MachineManager.Instance.AddMachine(this);
        }
    }

    private void Update() {
        if (Interpolation) {
            // Continually lerp towards final position
            Vector3 vel = Vector3.zero;

            head.localPosition = Vector3.Lerp(
                head.localPosition,
                Axes[0].AxisVector3,
                LerpSpeed * Time.deltaTime
            );
            holder.localPosition = Vector3.Lerp(
                holder.localPosition,
                Axes[1].AxisVector3,
                LerpSpeed * Time.deltaTime
            );
            base_y.localPosition = Vector3.Lerp(
                base_y.localPosition,
                Axes[2].AxisVector3,
                LerpSpeed * Time.deltaTime
            );
        } else {
            // Get latest correct axis angle
            head.localPosition = Axes[2].AxisVector3;
            holder.localPosition = Axes[1].AxisVector3;
            base_y.localPosition = Axes[0].AxisVector3;
        }
    }

    #region Public Methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetPosition"></param>
    public override void InverseKinematics(Vector3 targetPosition) {
        Debug.LogWarning("Not implemented yet!");
    }

    #endregion
}
