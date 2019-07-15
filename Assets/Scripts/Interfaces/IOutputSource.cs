using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOutputSource {
    /// <summary>Is this input currently enabled?</summary>
    bool OutputEnabled { get; set; }

    /// <summary>Is input type exclusive (Only one input of this type allowed?)</summary>
    bool ExclusiveType { get; set; }

    string Name { get; set; }
}
