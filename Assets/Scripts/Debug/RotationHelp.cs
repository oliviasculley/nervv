// System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

public class RotationHelp : MonoBehaviour {

    #region Properties

    [Header("Properties")]
    public Vector3 offset;

    #endregion

    #region Unity Methods

    [RuntimeInitializeOnLoadMethod]
    void Update() {
        transform.localEulerAngles += offset;
    }

    #endregion

}
