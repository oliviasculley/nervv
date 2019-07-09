using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputSource : MonoBehaviour, IInputSource {
    // This is the base class for inputs. These are dynamically added to InputManager.

    [Header("Input Properties")]
    [Tooltip("Is this input currently enabled?")]
    [SerializeField] private bool _inputEnabled = true;

    [Header("Input Settings")]
    public new string name;
    [Tooltip("Is input type exclusive (Only one input of this type allowed?)")]
    [SerializeField] private bool _exclusiveType;

    /// <summary>
    /// Enable or disable input source without enabling/disabling source connection
    /// </summary>
    /// <param name="b"></param>
    public virtual void SetSourceActive(bool isActive) {
        _inputEnabled = isActive;
    }

    /// <summary>
    /// Returns true if input source is actively applying values
    /// </summary>
    /// <returns>If input source is actively applying values</returns>
    public virtual bool IsActive() {
        return _inputEnabled;
    }

    /// <summary>
    /// Returns if input is exclusive (None other of same type may exist)
    /// </summary>
    /// <returns>If input source is exclusive</returns>
    public virtual bool IsExclusive() {
        return _exclusiveType;
    }

    /// <summary>
    /// Returns the input's name
    /// </summary>
    /// <returns>String with input's name</returns>
    public virtual string GetName() {
        return name;
    }

    public InputSource(string name, bool exclusiveType) {
        this.name = name;
        _exclusiveType = exclusiveType;
    }
}