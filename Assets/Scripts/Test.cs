using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [Header("References")]
    public Kuka kuka;

    void Update()
    {
        kuka.InverseKinematics(transform.position);
    }
}
