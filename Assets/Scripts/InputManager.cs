using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    // InputManager
    // This class handles different input sources and sends the data
    // to the machines through the MachineManager.

    // Static reference to self
    public static InputManager Instance;

    [Header("Properties")]
    public List<InputSource> inputs; // List of input sources in scene

    // Private vars
    List<System.Type> knownExclusives;  // Keeps track of exclusive types in inputs

    private void Awake() {
        // Add static reference to self
        if (Instance != null)
            Debug.LogWarning("[InputManager] Static ref to self was not null!\nOverriding...");
        Instance = this;
    }

    /* Public Methods */

    /// <summary>
    /// Adds an input to list of inputs
    /// </summary>
    /// <param name="input">Input source to add</param>
    /// <returns>Succesfully added?</returns>
    public bool AddInput(InputSource input) {
        // Check for duplicate inputs
        if (inputs.Contains(input))
            return false;

        // Check for same type for exclusive types
        if (knownExclusives.Contains(input.GetType()))
            return false;

        // Add to knownExclusives if exclusive
        if (input.exclusiveType)
            knownExclusives.Add(input.GetType());

        // Add to list of inputs
        inputs.Add(input);
        return true;
    }

    /// <summary>
    /// Removes an input from list of inputs
    /// </summary>
    /// <param name="input">Input source to remove</param>
    /// <returns>Succesfully removed?</returns>
    public bool RemoveInput(InputSource input) {
        // If exclusive, remove from exclusives list
        if (input.exclusiveType)
            knownExclusives.Remove(input.GetType());

        // Remove from list of inputs
        return inputs.Remove(input);
    }

    /// <summary>
    /// Returns inputs of same type
    /// </summary>
    /// <typeparam name="T">Type of input to return</typeparam>
    /// <returns>List<InputSource> of inputs</returns>
    public List<InputSource> GetInputs<T>() {
        List<InputSource> foundInputs = new List<InputSource>();

        foreach (InputSource i in inputs)
            if (i.GetType() == typeof(T))
                foundInputs.Add(i);

        return foundInputs;
    }

    /// <summary>
    /// Returns inputs of same type
    /// </summary>
    /// <param name="type">String of name of type of input to return</param>
    /// <returns>List<InputSource> of inputs</returns>
    public List<InputSource> GetInputs(string type) {
        List<InputSource> foundInputs = new List<InputSource>();

        foreach (InputSource i in inputs)
            if (i.GetType().ToString() == type)
                foundInputs.Add(i);

        return foundInputs;
    }
}
