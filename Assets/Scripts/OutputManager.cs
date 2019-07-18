﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutputManager : MonoBehaviour
{
    // OutputManager
    // This class handles different output sources.

    // Static reference to self
    public static OutputManager Instance;

    [Header("Properties")]
    public List<IOutputSource> outputs; // List of output sources in scene

    // Private vars
    List<System.Type> knownExclusives;  // Keeps track of exclusive types in outputs

    private void Awake() {
        // Add static reference to self
        if (Instance != null)
            Debug.LogWarning("[OutputManager] Static ref to self was not null!\nOverriding...");
        Instance = this;

        // Initialize vars
        knownExclusives = new List<System.Type>();
        outputs = new List<IOutputSource>();
    }

    #region Public Methods

    /// <summary>
    /// Adds an output to list of outputs
    /// </summary>
    /// <param name="output">Output source to add</param>
    /// <returns>Succesfully added?</returns>
    public bool AddOutput(IOutputSource output) {
        // Check for duplicate outputs
        if (outputs.Contains(output))
            return false;

        // Check for same type for exclusive types
        if (knownExclusives.Contains(output.GetType()))
            return false;

        // Add to knownExclusives if exclusive
        if (output.ExclusiveType)
            knownExclusives.Add(output.GetType());

        // Add to list of outputs
        outputs.Add(output);
        return true;
    }

    /// <summary>
    /// Removes an output from list of outputs
    /// </summary>
    /// <param name="output">Output source to remove</param>
    /// <returns>Succesfully removed?</returns>
    public bool RemoveOutput(IOutputSource output) {
        // If exclusive, remove from exclusives list
        if (output.ExclusiveType)
            knownExclusives.Remove(output.GetType());

        // Remove from list of outputs
        return outputs.Remove(output);
    }

    /// <summary>
    /// Returns outputs of same type
    /// </summary>
    /// <typeparam name="T">Type of output to return</typeparam>
    /// <returns>List<OutputSource> of outputs</returns>
    public List<IOutputSource> GetOutputs<T>() {
        List<IOutputSource> foundOutputs = new List<IOutputSource>();

        foreach (IOutputSource i in outputs)
            if (i.GetType() == typeof(T))
                foundOutputs.Add(i);

        return foundOutputs;
    }

    /// <summary>
    /// Returns outputs of same type
    /// </summary>
    /// <param name="type">String of name of type of output to return</param>
    /// <returns>List<OutputSource> of outputs</returns>
    public List<IOutputSource> GetOutputs(string type) {
        List<IOutputSource> foundOutputs = new List<IOutputSource>();

        foreach (IOutputSource i in outputs)
            if (i.GetType().ToString() == type)
                foundOutputs.Add(i);

        return foundOutputs;
    }

    #endregion
}