namespace ALinq
{
    /// <summary>
    /// Describes the type of change the entity will undergo when changes are submitted to the database.
    /// </summary>
    public enum ChangeAction
    {
        /// <summary>
        /// The entity will not be submitted.
        /// </summary>
        None,

        /// <summary>
        /// The entity will be deleted.
        /// </summary>
        Delete,

        /// <summary>
        /// The entity will be inserted.
        /// </summary>
        Insert,

        /// <summary>
        /// The entity will be updated.
        /// </summary>
        Update
    }
}