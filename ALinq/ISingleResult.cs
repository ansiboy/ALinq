using System;
using System.Collections;
using System.Collections.Generic;

namespace ALinq
{
    /// <summary>
    /// Represents the result of a mapped function that has a single return sequence.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the return sequence.</typeparam>
    public interface ISingleResult<T> : IEnumerable<T>, IEnumerable, IFunctionResult, IDisposable
    {
    }
}