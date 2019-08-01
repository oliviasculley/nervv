// System
using System.Collections;
using System.Collections.Generic;

// Unity Engine
using UnityEngine;

/// <summary>
/// The InputManager handles different input sources and sends
/// the data to the machines through the MachineManager. There
/// should only be one instance of InputManager running at any
/// time, referenced by InputManager.Instance. Static reference
/// to self is set in Awake(), so any calls to Instance must
/// happen in Start() or later.
/// </summary>
public class InputManager : MonoBehaviour {
    #region Static
    public static InputManager Instance;
    #endregion

    #region Properties
    [SerializeField, Tooltip("List of input sources in scene"), Header("Properties")]
    List<IInputSource> _inputs;
    /// <summary>List of input sources in scene</summary>
    public List<IInputSource> Inputs {
        get { return _inputs; }
    }
    #endregion

    #region Vars
    /// <summary>Keeps track of exclusive types in inputs</summary>
    List<System.Type> knownExclusives;
    #endregion

    #region Unity Methods
    /// <summary>Set static reference to self and initialize variables</summary>
    void Awake() {
        // Initialize vars
        knownExclusives = new List<System.Type>();
        _inputs = new List<IInputSource>();

        // Add static reference to self
        if (Instance != null)
            Debug.LogWarning("[InputManager] Static ref to self was not null!\nOverriding...");
        Instance = this;
    }
    #endregion

    #region Public Methods
    /// <summary>Adds an input to list of inputs</summary>
    /// <param name="input">Input source to add</param>
    /// <returns>Succesfully added?</returns>
    public bool AddInput(IInputSource input) {
        // Check for duplicate inputs
        if (_inputs.Contains(input))
            return false;

        // Check for same type for exclusive types
        if (knownExclusives.Contains(input.GetType()))
            return false;

        // Add to knownExclusives if exclusive
        if (input.ExclusiveType)
            knownExclusives.Add(input.GetType());

        // Add to list of inputs
        _inputs.Add(input);
        return true;
    }

    /// <summary>Removes an input from list of inputs</summary>
    /// <param name="input">Input source to remove</param>
    /// <returns>Succesfully removed?</returns>
    public bool RemoveInput(IInputSource input) {
        // If exclusive, remove from exclusives list
        if (input.ExclusiveType)
            knownExclusives.Remove(input.GetType());

        // Remove from list of inputs
        return _inputs.Remove(input);
    }

    /// <summary>Returns inputs of same type</summary>
    /// <typeparam name="T">Type of input to return</typeparam>
    /// <returns>List<InputSource> of inputs</returns>
    public List<IInputSource> GetInputs<T>() {
        List<IInputSource> foundInputs = new List<IInputSource>();

        foreach (IInputSource i in _inputs)
            if (i.GetType() == typeof(T))
                foundInputs.Add(i);

        return foundInputs;
    }

    /// <summary>Returns inputs of same type</summary>
    /// <param name="type">String of name of type of input to return</param>
    /// <returns>List<InputSource> of inputs</returns>
    public List<IInputSource> GetInputs(string type) {
        List<IInputSource> foundInputs = new List<IInputSource>();

        foreach (IInputSource i in _inputs)
            if (i.GetType().ToString() == type)
                foundInputs.Add(i);

        return foundInputs;
    }
    #endregion
}
