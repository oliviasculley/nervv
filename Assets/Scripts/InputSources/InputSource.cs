using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSource : MonoBehaviour
{
    // Input
    // This is the base class for all inputs. These are added to InputManager.

    [Header("Settings")]
    public new string name;
    public string source;
    public bool exclusiveType;  // Is input type exclusive (Only one input of this type allowed?)

    public InputSource(string name) {
        this.name = name; 
    }

    public InputSource(string name, string source) : this(name) {
        this.source = source;
    }
}