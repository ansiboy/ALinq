using System;
using ALinq.SqlClient;

namespace ALinq.SqlClient
{
    internal interface IObjectReaderSession : IConnectionUser, IDisposable
    {
        // Methods
        void Buffer();

        // Properties
        bool IsBuffered { get; }
    }
}