using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMachine {

    #region Fields

    /// <summary></summary>
    List<Machine.Axis> Axes { get; set; }

    /// <summary>Max speed of machine</summary>
    float MaxSpeed { get; set; }

    /// <summary>Individual ID</summary>
    string Name { get; set; }

    /// <summary></summary>
    string UUID { get; set; }

    /// <summary></summary>
    string Manufacturer { get; set; }

    /// <summary></summary>
    string Model { get; set; }

    #endregion

    #region Required methods

    /// <summary>
    /// Activate a small delta of inverse kinematics for the target position.
    /// </summary>
    /// <param name="targetPosition">Vector3 of target position in world space</param>
    void InverseKinematics(Vector3 targetPosition);

    #endregion
}
