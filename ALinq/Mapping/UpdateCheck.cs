namespace ALinq.Mapping
{
    /// <summary>
    /// Specifies when objects are to be tested for concurrency conflicts.
    /// </summary>
    public enum UpdateCheck
    {
        /// <summary>
        /// Always check.
        /// </summary>
        Always,

        /// <summary>
        /// Never check.
        /// </summary>
        Never,

        /// <summary>
        /// Check only when the object has been changed.
        /// </summary>
        WhenChanged
    }
}