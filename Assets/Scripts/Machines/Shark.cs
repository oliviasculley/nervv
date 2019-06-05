using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shark : Machine
{
    [Header("Shark Settings")]
    public float lerpSpeed = 10f;           // Speed to lerp to correct position
    public bool interpolation = true;       // Toggles lerping to final position

    // Private Vars
    private Transform head,holder,base_y;  // Location of Shark Head

    // Constants
    private readonly float scaleFactor = 0.0254f; // inches to mm

    private void Awake() {
        // Init arrays
        axes = new List<Axis>();
        head = transform.Find("A1/A2/A3");
	    holder = transform.Find("A1/A2");
	    base_y = transform.Find("A1");

        // Safety checks
        Debug.Assert(head != null, "Could not find head!");
        Debug.Assert(holder != null, "Could not find holder!");
        Debug.Assert(base_y != null, "Could not find base_y!");


        if (lerpSpeed == 0)
            Debug.LogWarning("Lerp speed is 0, will never go to final position!");
        if (maxSpeed == 0)
            Debug.LogWarning("MaxSpeed set to 0, will not be able to move!");
    }

    private void Start() {
        // Add to MachineManager
        if (MachineManager.Instance == null) {
            Debug.LogWarning("[Shark] Could not find MachineManager!");
        } else {
            MachineManager.Instance.AddMachine(this);
        }

        // Initialize Axes
        for (int i = 1; i <= 3; i++)
            axes.Add(new Axis("a" + i.ToString(), "A" + i.ToString(), 0, AxisType.Linear));
    }

    private void Update() {
        if (interpolation) {
            // Continually lerp towards final position
            Vector3 vel = Vector3.zero;
            head.localPosition = Vector3.Lerp(  head.localPosition,
                                                GetAxisVector3(axes[0]),
                                                lerpSpeed * Time.deltaTime);
			Debug.Log("Shark  dsfbkjasb"+ GetAxisVector3(axes[2]));
            holder.localPosition = Vector3.Lerp(holder.localPosition,
                                                GetAxisVector3(axes[1]),
                                                lerpSpeed * Time.deltaTime);
            base_y.localPosition = Vector3.Lerp(base_y.localPosition,
                                                GetAxisVector3(axes[0]),
                                                lerpSpeed * Time.deltaTime);
        } else {
            // Get latest correct axis angle
            head.localPosition = GetAxisVector3(axes[2]);
            holder.localPosition = GetAxisVector3(axes[1]);
            base_y.localPosition = GetAxisVector3(axes[0]);
        }
    }

    /* Public Methods */

    /// <summary>
    /// Sets the angle of a certain axis
    /// </summary>
    /// <param name="s">Name of the axis to set (1 indexed)</param>
    public void SetAxisValue(string axisID, float value) {

        // Get Axis with axisID
        Axis found;
        if ((found = axes.Find(x => x.GetID() == axisID)) == null)
            return;

        found.SetValue(value);
    }

    /// <summary>
    /// Returns the Vector3 for the associated axis
    /// </summary>
    /// <param name="axisID">ID of the axis to return Vector3</param>
    /// <returns></returns>
    public override Vector3 GetAxisVector3(Axis axis) {
        switch (axis.GetID()) {
            case "a1":
                return new Vector3(0, axis.GetValue(), 0) * scaleFactor;

            case "a2":
                return new Vector3(axis.GetValue(), 0, 0) * scaleFactor;

            case "a3":
                return new Vector3(0, 0, axis.GetValue()) * scaleFactor;

            default:
                Debug.LogError("[Shark] Could not find axis with id: " + axis.GetID());
                return new Vector3(0, 0, 0);
        }
	}
}
