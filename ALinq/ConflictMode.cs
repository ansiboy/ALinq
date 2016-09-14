namespace ALinq
{
    /// <summary>
    /// Specifies when concurrency conflicts should be reported.
    /// </summary>
    public enum ConflictMode
    {
        /// <summary>
        /// Specifies that attempts to update the database should stop immediately when the first concurrency conflict error is detected.
        /// </summary>
        FailOnFirstConflict,

        /// <summary>
        /// Specifies that all updates to the database should be tried, and that concurrency conflicts should be accumulated and returned at the end of the process.
        /// </summary>
        ContinueOnConflict
    }
}