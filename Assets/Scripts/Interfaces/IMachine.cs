// System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

namespace NERVV {
    /// <summary>
    /// The base machine interface that must be implemented to
    /// be considered as a machine.
    /// </summary>
    public interface IMachine {
        #region Required Fields
        /// <summary>List of axes of movement</summary>
        List<Machine.Axis> Axes { get; set; }

        /// <summary>Individual ID</summary>
        string Name { get; set; }

        /// <summary></summary>
        string UUID { get; set; }

        /// <summary></summary>
        string Manufacturer { get; set; }

        /// <summary></summary>
        string Model { get; set; }
        #endregion
    }
}