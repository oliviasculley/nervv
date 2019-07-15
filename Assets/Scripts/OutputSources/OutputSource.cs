using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OutputSource : MonoBehaviour, IOutputSource {
    // This is the base class for outputs. These are dynamically added to OutputManager.

    [Header("Output Properties")]
        [Tooltip("Is this output currently enabled?")]
        [SerializeField] protected bool _outputEnabled = true;
        public bool OutputEnabled {
            get { return _outputEnabled; }
            set { _outputEnabled = value; }
        }

    [Header("Output Settings")]
        protected string _name;
        public string Name {
            get { return _name; }
            set { _name = value; }
        }
        [Tooltip("Is output type exclusive (Only one output of this type allowed?)")]
        [SerializeField] protected bool _exclusiveType = false;
        public bool ExclusiveType {
            get { return _exclusiveType; }
            set { _exclusiveType = value; }
        }
}