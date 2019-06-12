using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class rotationhelper : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.localEulerAngles += new Vector3(0, Time.deltaTime * 10000, 0);
    }
}
