using System;
using System.Collections.Generic;
using System.Data.Common;
using ALinq.SqlClient.Implementation;

namespace ALinq.SqlClient
{
    internal partial class ObjectReaderCompiler
    {
        private class ObjectReader<TDataReader, TObject> : ObjectReaderBase<TDataReader>, 
                                                           IEnumerator<TObject>, IObjectReader where TDataReader : DbDataReader
        {
            // Fields
            private readonly bool disposeSession;
            private readonly Func<ObjectMaterializer<TDataReader>, TObject> fnMaterialize;

            // Methods
            internal ObjectReader(ObjectReaderSession<TDataReader> session, NamedColumn[] namedColumns,
                                  object[] globals, object[] arguments, int nLocals, bool disposeSession,
                                  Func<ObjectMaterializer<TDataReader>, TObject> fnMaterialize)
                : base(session, namedColumns, globals, arguments, nLocals)
            {
                this.disposeSession = disposeSession;
                this.fnMaterialize = fnMaterialize;
            }

            public void Dispose()
            {
                if (disposeSession)
                {
                    session.Dispose();
                }
            }

            public bool MoveNext()
            {
                if (Read())
                {
                    Current = fnMaterialize(this);
                    return true;
                }
                Current = default(TObject);
                Dispose();
                return false;
            }

            public void Reset()
            {
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            // Properties
            public TObject Current { get; private set; }

            public IObjectReaderSession Session
            {
                [System.Diagnostics.DebuggerStepThrough]
                get { return session; }
            }

        }
    }
}
