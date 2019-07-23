using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class OutputSafety : MonoBehaviour {
    public void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Machines")) {
            Debug.LogError("[SAFETY TRIGGERED] " + name + " triggered by " + other.name + "\nShutting down ALL OUTPUTS!");
            foreach (IOutputSource output in OutputManager.Instance.outputs)
                output.OutputEnabled = false;
        }
    }

    public void OnTriggerStay(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Machines"))
            foreach (IOutputSource output in OutputManager.Instance.outputs)
                output.OutputEnabled = false;
    }
}
