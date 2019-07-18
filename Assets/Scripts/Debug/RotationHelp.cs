using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationHelp : MonoBehaviour
{
    [Header("Properties")]
    public Vector3 offset;

    [RuntimeInitializeOnLoadMethod]
    void Update()
    {
        transform.localEulerAngles += offset;
    }
}
