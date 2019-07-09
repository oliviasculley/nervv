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
            head.localPosition = Vector3.Lerp(  head.localPosition,
                                                GetAxisVector3(Axes[0]),
                                                LerpSpeed * Time.deltaTime);
			//Debug.Log("Shark  dsfbkjasb"+ GetAxisVector3(Axes[2]));
            holder.localPosition = Vector3.Lerp(holder.localPosition,
                                                GetAxisVector3(Axes[1]),
                                                LerpSpeed * Time.deltaTime);
            base_y.localPosition = Vector3.Lerp(base_y.localPosition,
                                                GetAxisVector3(Axes[2]),
                                                LerpSpeed * Time.deltaTime);
        } else {
            // Get latest correct axis angle
            head.localPosition = GetAxisVector3(Axes[2]);
            holder.localPosition = GetAxisVector3(Axes[1]);
            base_y.localPosition = GetAxisVector3(Axes[0]);
        }
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
        if ((found = Axes.Find(x => x.ID == axisID)) == null)
            return;

        found.Value = value;
    }

    /// <summary>
    /// Returns the Vector3 for the associated axis in local space
    /// </summary>
    /// <param name="axis">Axis to return Vector3</param>
    /// <returns>Vector3 of rotation for selected axis in local space</returns>
    public override Vector3 GetAxisVector3(Axis axis) {
        switch (axis.ID) {
            case "x_axis":
                return new Vector3(0, axis.Value, 0) * scaleFactor;

            case "y_axis":
                return new Vector3(axis.Value, 0, 0) * scaleFactor;

            case "z_axis":
                return new Vector3(0, 0, axis.Value) * scaleFactor;

            default:
                Debug.LogError("[Shark] Could not find axis with id: " + axis.ID);
                return new Vector3(0, 0, 0);
        }
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetPosition"></param>
    public override void InverseKinematics(Vector3 targetPosition) {
        Debug.LogWarning("Not implemented yet!");
    }
}
