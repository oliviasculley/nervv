using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this script to any trigger colliders to automatically disable
/// all outputs if a machine collider enters the trigger
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class OutputSafety : MonoBehaviour {

    /// <summary> Shut down outputs if collider enters the trigger </summary>
    public void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Machines")) {
            Debug.LogError(
                "[SAFETY TRIGGERED] " + name +
                " triggered by " + other.name + "\nShutting down ALL OUTPUTS!");
            foreach (IOutputSource output in OutputManager.Instance.outputs)
                output.OutputEnabled = false;
        }
    }

    /// <summary> Ensure that outputs are shut down while machine is in trigger </summary>
    public void OnTriggerStay(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Machines"))
            foreach (IOutputSource output in OutputManager.Instance.outputs)
                output.OutputEnabled = false;
    }
}
