using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineManager : MonoBehaviour
{
    // MachineManager
    // This class handles adding and removing machines from the scene.
    // It also handles sending data to the machines

    /* Static Reference */
    public static MachineManager Instance;

    [Header("Properties")]
    public List<Machine> machines;  // List of machines in scene

    public void Awake() {
        if (Instance != null)
            Debug.LogWarning("[MachineManager] Static ref to self was not null!\nOverriding...");
        Instance = this;
    }

    /* Public Methods */

    /// <summary>
    /// Adds machine to machines List
    /// </summary>
    /// <param name="machine">Machine to add</param>
    /// <returns>Succesfully added?</returns>
    public bool AddMachine(Machine machine) {
        // Check for duplicate machines
        if (machines.Contains(machine))
            return false;

        machines.Add(machine);
        return true;
    }

    /// <summary>
    /// Removes machine from machines list
    /// </summary>
    /// <param name="machine">Machine to remove</param>
    /// <returns>Succesfully removed?</returns>
    public bool RemoveMachine(Machine machine) {
        return machines.Remove(machine);
    }

    /// <summary>
    /// Returns machines of same type
    /// </summary>
    /// <typeparam name="T">Type of machine to return</typeparam>
    /// <returns>List<Machine> of machines</returns>
    public List<Machine> GetMachines<T>() {
        List<Machine> foundMachines = new List<Machine>();

        foreach (Machine m in machines)
            if (m.GetType() == typeof(T))
                foundMachines.Add(m);

        return foundMachines;
    }

    /// <summary>
    /// Returns machines of same type
    /// </summary>
    /// <param name="type">Type of machine to return</param>
    /// <returns>List<Machine> of machines</returns>
    public List<Machine> GetMachines(System.Type type) {
        List<Machine> foundMachines = new List<Machine>();

        foreach (Machine m in machines)
            if (m.GetType() == type)
                foundMachines.Add(m);

        return foundMachines;
    }

    /// <summary>
    /// Returns machine with matching ID
    /// </summary>
    /// <param name="id">string with machine ID</param>
    /// <returns>Selected machine if found, otherwise returns null</returns>
    public Machine GetMachine(string id) {
        return machines.Find(x => x.id == id);
    }
}
