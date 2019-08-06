// System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

// NERVV
using NERVV;

/// <summary>
/// Testing script that calls InverseKinematics on a Machine
/// </summary>
/// <seealso cref="Machine"/>
public class IKHelp : MonoBehaviour {
    #region References
    [Header("References")]
    public Machine machine;
    #endregion

    #region Unity methods
    /// <summary>Call InverseKinematics if machine is not null</summary>
    public void Update() {
        if (machine != null)
            machine.InverseKinematics(transform.position);
    }
    #endregion
}
