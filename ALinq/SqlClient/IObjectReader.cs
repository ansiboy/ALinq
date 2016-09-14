using System;
using System.Collections;

namespace ALinq.SqlClient
{
    internal interface IObjectReader : IEnumerator, IDisposable
    {
        // Properties
        IObjectReaderSession Session { get; }
    }
}