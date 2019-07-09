using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputSource
{
    /// <summary>
    /// Enable or disable input source without enabling/disabling source connection
    /// </summary>
    /// <param name="newActiveState">New active state to set</param>
    void SetSourceActive(bool newActiveState);

    /// <summary>
    /// Returns true if input source is actively applying values
    /// </summary>
    /// <returns>If input source is actively applying values</returns>
    bool IsActive();

    /// <summary>
    /// Returns if input is exclusive (None other of same type may exist)
    /// </summary>
    /// <returns>If input source is exclusive</returns>
    bool IsExclusive();

    /// <summary>
    /// Returns the input's name
    /// </summary>
    /// <returns>String with input's name</returns>
    string GetName();
}
