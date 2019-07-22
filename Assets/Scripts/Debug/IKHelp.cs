﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MTConnectVR;

public class IKHelp : MonoBehaviour {
    public Machine machine;

    public void Update() {
        if (machine != null)
            machine.InverseKinematics(transform.position);
    }
}
