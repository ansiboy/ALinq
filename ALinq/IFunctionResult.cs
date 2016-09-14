namespace ALinq
{
    /// <summary>
    /// Provides access to the return value of a function.
    /// </summary>
    public interface IFunctionResult
    {
        /// <summary>
        /// Gets the return value of a function.
        /// </summary>
        /// <returns>
        /// The value returned by the function.
        /// </returns>
        object ReturnValue { get; }
    }
}