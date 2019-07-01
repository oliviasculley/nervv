using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSource : MonoBehaviour
{
    // Input
    // This is the base class for all inputs. These are added to InputManager.

    [Header("Input Properties")]
    public bool inputEnabled = true;   // Is this input currently enabled?

    [Header("Input Settings")]
    public new string name;
    public bool exclusiveType;  // Is input type exclusive (Only one input of this type allowed?)
}