using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using ALinq.SqlClient.Implementation;

namespace ALinq.SqlClient
{
    internal partial class ObjectReaderCompiler
    {
        private class ObjectReaderSession<TDataReader> : IObjectReaderSession where TDataReader : DbDataReader
        {
            // Fields
            private List<DbDataReader> buffer;
            private ObjectReaderBase<TDataReader> currentReader;
            private readonly TDataReader dataReader;
            private bool hasResults;
            private int iNextBufferedReader;
            private bool isDataReaderDisposed;
            private bool isDisposed;
            private readonly object[] parentArgs;
            private readonly IReaderProvider provider;
            private readonly ICompiledSubQuery[] subQueries;
            private readonly object[] userArgs;

            // Methods
            internal ObjectReaderSession(TDataReader dataReader, IReaderProvider provider, object[] parentArgs,
                                         object[] userArgs, ICompiledSubQuery[] subQueries)
            {
                this.dataReader = dataReader;
                this.provider = provider;
                this.parentArgs = parentArgs;
                this.userArgs = userArgs;
                this.subQueries = subQueries;
                hasResults = true;
            }

            public void Buffer()
            {
                if (buffer == null)
                {
                    if ((currentReader != null) && !currentReader.IsBuffered)
                    {
                        currentReader.Buffer();
                        CheckNextResults();
                    }
                    buffer = new List<DbDataReader>();
                    while (hasResults)
                    {
                        var set = new DataSet { EnforceConstraints = false };
                        var table = new DataTable();
                        set.Tables.Add(table);
                        string[] activeNames = GetActiveNames();
                        table.Load(new Rereader(dataReader, false, null),
                                   LoadOption.OverwriteChanges);
                        buffer.Add(new Rereader(table.CreateDataReader(), false, activeNames));
                        CheckNextResults();
                    }
                }
            }

            private void CheckNextResults()
            {
                hasResults = !dataReader.IsClosed && dataReader.NextResult();
                currentReader = null;
                if (!hasResults)
                {
                    Dispose();
                }
            }

            public void CompleteUse()
            {
                Buffer();
            }

            internal ObjectReader<TDataReader, TObject> CreateReader<TObject>(
                Func<ObjectMaterializer<TDataReader>, TObject> fnMaterialize,
                NamedColumn[] namedColumns, object[] globals, int nLocals, bool disposeDataReader)
            {
                var reader = new ObjectReader<TDataReader, TObject>(this, namedColumns, globals,
                                                                    userArgs, nLocals, disposeDataReader, fnMaterialize);
                currentReader = reader;
                return reader;
            }

            public void Dispose()
            {
                if (!isDisposed)
                {   
                    isDisposed = true;
                    if (!isDataReaderDisposed)
                    {
                        isDataReaderDisposed = true;
                        dataReader.Dispose();
                    }
                    provider.ConnectionManager.ReleaseConnection(this);
                }
            }

            internal void Finish(ObjectReaderBase<TDataReader> finishedReader)
            {
                if (currentReader == finishedReader)
                {
                    CheckNextResults();
                }
            }

            internal string[] GetActiveNames()
            {
                var strArray = new string[DataReader.FieldCount];
                int index = 0;
                int fieldCount = DataReader.FieldCount;
                while (index < fieldCount)
                {
                    strArray[index] = DataReader.GetName(index);
                    index++;
                }
                return strArray;
            }

            internal DbDataReader GetNextBufferedReader()
            {
                if (iNextBufferedReader < buffer.Count)
                {
                    return buffer[iNextBufferedReader++];
                }
                return null;
            }

            internal ObjectReader<TDataReader, TObject> GetNextResult<TObject>(
                Func<ObjectMaterializer<TDataReader>, TObject> fnMaterialize,
                NamedColumn[] namedColumns, object[] globals, int nLocals, bool disposeDataReader)
            {
                if (buffer != null)
                {
                    if (iNextBufferedReader >= buffer.Count)
                    {
                        return null;
                    }
                }
                else
                {
                    if (currentReader != null)
                    {
                        currentReader.Buffer();
                        CheckNextResults();
                    }
                    if (!hasResults)
                    {
                        return null;
                    }
                }
                var reader = new ObjectReader<TDataReader, TObject>(this, namedColumns, globals,
                                                                    userArgs, nLocals, disposeDataReader, fnMaterialize);
                currentReader = reader;
                return reader;
            }

            // Properties
            internal ObjectReaderBase<TDataReader> CurrentReader
            {
                get { return currentReader; }
            }

            internal TDataReader DataReader
            {
                get { return dataReader; }
            }

            public bool IsBuffered
            {
                get { return (buffer != null); }
            }

            internal object[] ParentArguments
            {
                get { return parentArgs; }
            }

            internal IReaderProvider Provider
            {
                get { return provider; }
            }

            internal ICompiledSubQuery[] SubQueries
            {
                get { return subQueries; }
            }

            internal object[] UserArguments
            {
                get { return userArgs; }
            }
        }
    }
}
