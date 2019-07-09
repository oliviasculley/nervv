﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMachine
{
    /// <summary>
    /// Returns the Vector3 for the associated axis
    /// </summary>
    /// <param name="axis">Axis to get Vector3</param>
    /// <returns>Vector3 of axis value in local space</returns>
    Vector3 GetAxisVector3(Machine.Axis axis);

    /// <summary>
    /// Sets the value of a certain axis by axis' ID. Use this function to apply offsets or mirrors to incoming values!
    /// </summary>
    /// <param name="axisID">Axis ID (MTConnect string identifier) to set</param>
    /// <param name="value">Value of axis to set</param>
    void SetAxisValue(string axisID, float value);

    /// <summary>
    /// Activate a small delta of inverse kinematics for the target position.
    /// </summary>
    /// <param name="targetPosition">Vector3 of target position in world space</param>
    void InverseKinematics(Vector3 targetPosition);
}