namespace NERVV {
    /// <summary>
    /// Interface that must be fulfilled to be considered as an output source
    /// </summary>
    public interface IOutputSource {
        /// <summary>Is this input currently enabled?</summary>
        bool OutputEnabled { get; set; }

        /// <summary>Is output type exclusive (Only one output of this type allowed?)</summary>
        bool ExclusiveType { get; set; }

        /// <summary></summary>
        string Name { get; set; }
    }
}