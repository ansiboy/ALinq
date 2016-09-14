using System.Collections.Generic;

namespace ALinq
{
    internal static class SourceState<T>
    {
        // Fields
        internal static readonly IEnumerable<T> Assigned;
        internal static readonly IEnumerable<T> Loaded;

        // Methods
        static SourceState()
        {
            SourceState<T>.Loaded = (IEnumerable<T>)new T[0];
            SourceState<T>.Assigned = (IEnumerable<T>)new T[0];
        }
    }
}