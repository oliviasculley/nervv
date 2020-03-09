// System
using System;
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

        /// <summary>Called when any Machine field is modified, except Axes!</summary>
        EventHandler OnMachineUpdated { get; set; }

        /// <summary>Called when Machine should stop moving for any reason</summary>
        EventHandler<BaseMachine.SafetyEventArgs> OnSafetyTriggered { get; set; }

        /// <summary>Used to activate safety and invokes OnSafetyTriggered</summary>
        void TriggerSafety(object sender, BaseMachine.SafetyEventArgs args);
    }
}