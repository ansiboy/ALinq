using System;
using System.Collections.Generic;

namespace ALinq
{
    /// <summary>
    /// Represents the results of mapped functions or queries with variable return sequences.
    /// </summary>
    public interface IMultipleResults : IFunctionResult, IDisposable
    {
        /// <summary>
        /// Retrieves the next result as a sequence of a specified type.
        /// </summary>
        /// <typeparam name="TElement">The type of the sequence to be returned.</typeparam>
        /// <returns>An enumeration for iterating over the results.</returns>
        IEnumerable<TElement> GetResult<TElement>();
    }
}
