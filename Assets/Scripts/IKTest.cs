using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKTest : MonoBehaviour
{
    [Header("References")]
    public Doosan IKMachine;

    void Update()
    {
        IKMachine.InverseKinematics(transform.position);
    }
}
