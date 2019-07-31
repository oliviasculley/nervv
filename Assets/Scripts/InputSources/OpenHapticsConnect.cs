// System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

// MTConnectVR
using MTConnectVR;

public class OpenHapticsConnect : InputSource {

    #region References

    [Header("References")]
    public HapticPlugin HapticDevice = null;

    #endregion

    #region Unity Methods

    protected override void Start() {
        base.Start();

        // Get ref to HapticPlugin
        HapticDevice = (HapticPlugin)FindObjectOfType(typeof(HapticPlugin));
        Debug.Assert(HapticDevice != null, "[OpenHapticsConnect] Could not find HapticPlugin script in scene!");

        // Add self to InputManager
        Debug.Assert(InputManager.Instance != null, "[OpenHapticsConnect] Could not get ref to InputManager!");
        if (!InputManager.Instance.AddInput(this))
            Debug.LogError("[OpenHapticsConnect] Could not add self to InputManager!");
    }

    #endregion

}
