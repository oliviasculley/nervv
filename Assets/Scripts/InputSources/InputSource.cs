using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputSource : MonoBehaviour, IInputSource {
    // This is the base class for inputs. These are dynamically added to InputManager.

    [Header("Input Properties")]
        [Tooltip("Is this input currently enabled?")]
        [SerializeField] protected bool _inputEnabled = true;
        /// <summary>Is this input currently enabled?</summary>
        public virtual bool InputEnabled {
            get { return _inputEnabled; }
            set { _inputEnabled = value; }
        }

    [Header("Input Settings")]
        [SerializeField] protected string _name;
        public virtual string Name {
            get { return _name; }
            set { _name = value; }
        }
        [Tooltip("Is input type exclusive (Only one input of this type allowed?)")]
        [SerializeField] private bool _exclusiveType;
        /// <summary>Is input type exclusive (Only one input of this type allowed?)</summary>
        public virtual bool ExclusiveType {
            get { return _exclusiveType; }
            set { _exclusiveType = value; }
        }
}