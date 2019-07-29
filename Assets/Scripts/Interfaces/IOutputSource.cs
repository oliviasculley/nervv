/// <summary>
/// Interface that must be fulfilled to be considered as an output source
/// </summary>
public interface IOutputSource {

    #region Required Fields

    /// <summary> Is this input currently enabled? </summary>
    bool OutputEnabled { get; set; }

    /// <summary> Is input type exclusive (Only one input of this type allowed?) </summary>
    bool ExclusiveType { get; set; }

    /// <summary> </summary>
    string Name { get; set; }

    #endregion
}
