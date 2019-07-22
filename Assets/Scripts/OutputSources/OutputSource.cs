using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MTConnectVR {
    public abstract class OutputSource : MonoBehaviour, IOutputSource {
        // This is the base class for outputs. These are dynamically added to OutputManager.

        [Header("Output Properties")]

        [Tooltip("Is this output currently enabled?")]
        [SerializeField] protected bool _outputEnabled = true;
        /// <summary>Is this output currently enabled?</summary>
        public virtual bool OutputEnabled {
            get { return _outputEnabled; }
            set { _outputEnabled = value; }
        }

        [Header("Output Settings")]

        [SerializeField] protected string _name;
        /// <summary></summary>
        public virtual string Name {
            get { return _name; }
            set { _name = value; }
        }

        [Tooltip("Is output type exclusive (Only one output of this type allowed?)")]
        [SerializeField] protected bool _exclusiveType = false;
        /// <summary>Is output type exclusive (Only one output of this type allowed?)</summary>
        public virtual bool ExclusiveType {
            get { return _exclusiveType; }
            set { _exclusiveType = value; }
        }

        protected virtual void Start() {
            // Add self to InputManager, disabling self if failure
            Debug.Assert(OutputManager.Instance != null, "Could not get ref to OutputManager!");
            if (OutputEnabled &= OutputManager.Instance.AddOutput(this))
                Debug.LogError("Could not add self to OutputManager!");
        }
    }
}