// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

// NERVV
using NERVV;

/// <summary>
/// Attach this script to any trigger colliders to automatically disable
/// all outputs if a machine collider enters the trigger
/// </summary>
[RequireComponent(typeof(Collider)),
RequireComponent(typeof(Rigidbody))]
public class OutputSafety : MonoBehaviour {
    #region References
    [SerializeField,
    Tooltip("If null, will attempt to use global reference"), Header("References")]
    protected OutputManager _outputManager;
    public OutputManager OutputManager {
        get {
            if (_outputManager == null) {
                if (OutputManager.Instances.Count > 0)
                    _outputManager = OutputManager.Instances[0];
                else
                    throw new ArgumentNullException("Could not get a ref to an OutputManager!");
            }
            return _outputManager;
        }
        set => _outputManager = value;
    }
    #endregion

    #region Unity Methods
    /// <summary>Shut down outputs if collider enters the trigger</summary>
    public void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Machines")) {
            Debug.LogError(
                "[SAFETY TRIGGERED] " + name +
                " triggered by " + other.name + "\nShutting down ALL OUTPUTS!");
            foreach (IOutputSource output in OutputManager.Outputs)
                output.OutputEnabled = false;
        }
    }

    /// <summary>Ensure that outputs are shut down while machine is in trigger</summary>
    public void OnTriggerStay(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Machines"))
            foreach (IOutputSource output in OutputManager.Outputs)
                output.OutputEnabled = false;
    }
    #endregion
}
