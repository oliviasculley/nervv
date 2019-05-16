using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenHapticsConnect : MonoBehaviour
{
    [Header("References")]
    public HapticPlugin HapticDevice = null;

    void Start()
    {
        // Get ref to HapticPlugin
        HapticDevice = (HapticPlugin)FindObjectOfType(typeof(HapticPlugin));
        Debug.Assert(HapticDevice != null, "[OpenHapticsConnect] Could not find HapticPlugin script in scene!");
    }

    void Update()
    {
        // Send Haptic angles to Kuka
        if (MTConnect.mtc.machines.Count >= 2 && MTConnect.mtc.machines[1] != null)
            for (int i = 3; i < 9; i++)
                MTConnect.mtc.machines[1].SetAxisAngle("A" + (i - 2), 0);
    }
}
