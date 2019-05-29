using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSource : MonoBehaviour
{
    // Input
    // This is the base class for all inputs. These are added to InputManager.

    [Header("Settings")]
    public bool exclusiveType;  // Is input type exclusive (Only one input of this type allowed?)
}