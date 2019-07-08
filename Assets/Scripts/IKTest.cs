using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Valve.VR;

public class IKTest : MonoBehaviour
{
    [Header("Properties")]
    public SteamVR_Action_Boolean activateIK;

    [Header("References")]
    public GameObject sphere;

    private void Start() {
        Debug.Assert(sphere != null,
            "Could not get reference to sphere!");
    }

    void Update()
    {
        // If activated, perform IK on all robots
        if (activateIK.state)
            foreach (Machine m in MachineManager.Instance.machines)
                m.InverseKinematics(transform.position);

        // Set sphere visualizer visibility
        sphere.SetActive(activateIK.state);
    }
}
