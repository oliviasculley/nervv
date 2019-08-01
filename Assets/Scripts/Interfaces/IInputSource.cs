/// <summary>
/// This is the base interface required to implement
/// in order to be a functional input source. Generally,
/// inputs are initialized when enabled and uninitialized
/// when disabled. However, utilize the field InputEnabled
/// to enable or disable the input sources without completely
/// uninitializing the input source; i.e. Stopping a ROS
/// publisher from publishing every second without closing
/// the websocket connection.
/// </summary>
public interface IInputSource {
    #region Required Fields
    /// <summary>
    /// If the input source is actively publishing to machines
    /// or not. Note that the input source may still be inactive
    /// even when this is false, just not actively publishing.
    /// </summary>
    bool InputEnabled { get; set; }

    /// <summary>
    /// Are multiple instantiations of this script allowed?
    /// InputManager will reject multiple types of this script
    /// if ExclusiveType is true when added to InputManager.
    /// </summary>
    bool ExclusiveType { get; set; }

    string Name { get; set; }
    #endregion
}