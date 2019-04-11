using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Machine : MonoBehaviour
{
    [Header("Properties")]
    public double[] angles; // Angles for each axes

    [Header("Settings")]
    public int AxisCount;   // Number of axes on machine
    public float maxSpeed;  // Max speed of machine
    public string type;     // String with Model/Company
    public string id;       // Individual ID

    /* Public Methods */

    /// <summary>
    /// Returns the Vector3 for the associated axis
    /// </summary>
    /// <param name="axisID">ID of the axis to return Vector3</param>
    /// <returns></returns>
    public abstract Vector3 GetAxis(int axisID);

    /// <summary>
    /// Sets the angle of a certain axis
    /// </summary>
    /// <param name="s">Name of the axis to set</param>
    public abstract void SetAxisAngle(string axisName, double angle);
}