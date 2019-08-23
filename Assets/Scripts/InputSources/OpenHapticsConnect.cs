// System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

// NERVV
using NERVV;

public class OpenHapticsConnect : InputSource {
    #region References
    [Header("References")]
    public HapticPlugin HapticDevice = null;
    #endregion

    #region Unity Methods
    protected override void Start() {
        // Get ref to HapticPlugin
        HapticDevice = (HapticPlugin)FindObjectOfType(typeof(HapticPlugin));
        Debug.Assert(HapticDevice != null);

        // Initial InputSource fields
        Name = "OpenHaptics Device: " + HapticDevice.name;
        ExclusiveType = true;
        base.Start();
    }
    #endregion
}
