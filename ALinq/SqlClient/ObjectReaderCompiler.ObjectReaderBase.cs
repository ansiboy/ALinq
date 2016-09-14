using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using ALinq.Mapping;
using ALinq.SqlClient.Implementation;

namespace ALinq.SqlClient
{
    internal partial class ObjectReaderCompiler
    {
        private abstract class ObjectReaderBase<TDataReader> : ObjectMaterializer<TDataReader> where TDataReader : DbDataReader
        {
            // Fields
            private bool hasCurrentRow;
            private bool hasRead;
            private bool isFinished;
            private readonly IDataServices services;
            protected readonly ObjectReaderSession<TDataReader> session;

            // Methods
            protected ObjectReaderBase(ObjectReaderSession<TDataReader> session,
                                      NamedColumn[] namedColumns, object[] globals,
                                      object[] arguments, int nLocals)
            {
                this.session = session;
                services = session.Provider.Services;
                DataReader = session.DataReader;
                Globals = globals;
                Arguments = arguments;
                if (nLocals > 0)
                {
                    Locals = new object[nLocals];
                }
                if (this.session.IsBuffered)
                {
                    Buffer();
                }
                Ordinals = GetColumnOrdinals(namedColumns);
            }

            internal void Buffer()
            {
                if ((BufferReader == null) && (hasCurrentRow || !hasRead))
                {
                    if (session.IsBuffered)
                    {
                        BufferReader = session.GetNextBufferedReader();
                    }
                    else
                    {
                        var set = new DataSet { EnforceConstraints = false };
                        var table = new DataTable();
                        set.Tables.Add(table);
                        string[] activeNames = session.GetActiveNames();
                        table.Load(new Rereader(DataReader, hasCurrentRow, null), LoadOption.OverwriteChanges);
                        BufferReader = new Rereader(table.CreateDataReader(), false, activeNames);
                    }
                    if (hasCurrentRow)
                    {
                        Read();
                    }
                }
            }

            public override IEnumerable ExecuteSubQuery(int iSubQuery, object[] parentArgs)
            {
                if (session.ParentArguments != null)
                {
                    int length = session.ParentArguments.Length;
                    var destinationArray = new object[length + parentArgs.Length];
                    Array.Copy(session.ParentArguments, destinationArray, length);
                    Array.Copy(parentArgs, 0, destinationArray, length, parentArgs.Length);
                    parentArgs = destinationArray;
                }
                ICompiledSubQuery query = session.SubQueries[iSubQuery];
                return (IEnumerable)query.Execute(session.Provider, parentArgs, session.UserArguments).ReturnValue;
            }

            private int[] GetColumnOrdinals(NamedColumn[] namedColumns)
            {
                DbDataReader bufferReader = BufferReader ?? DataReader;
                if ((namedColumns == null) || (namedColumns.Length == 0))
                {
                    return null;
                }
                var numArray = new int[namedColumns.Length];
                var dictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                int ordinal = 0;
                int fieldCount = bufferReader.FieldCount;
                while (ordinal < fieldCount)
                {
                    dictionary[SqlIdentifier.QuoteCompoundIdentifier(bufferReader.GetName(ordinal))] = ordinal;
                    ordinal++;
                }
                int index = 0;
                int length = namedColumns.Length;
                while (index < length)
                {
                    int num5;
                    if (dictionary.TryGetValue(SqlIdentifier.QuoteCompoundIdentifier(namedColumns[index].Name), out num5))
                    {
                        numArray[index] = num5;
                    }
                    else
                    {
                        if (namedColumns[index].IsRequired)
                        {
                            throw Error.RequiredColumnDoesNotExist(namedColumns[index].Name);
                        }
                        numArray[index] = -1;
                    }
                    index++;
                }
                return numArray;
            }

            public override IEnumerable<T> GetLinkSource<T>(int iGlobalLink, int iLocalFactory, object[] keyValues)
            {
                var deferredSourceFactory = (IDeferredSourceFactory)Locals[iLocalFactory];
                if (deferredSourceFactory == null)
                {
                    var member = (MetaDataMember)Globals[iGlobalLink];
                    deferredSourceFactory = services.GetDeferredSourceFactory(member);
                    Locals[iLocalFactory] = deferredSourceFactory;
                }
                return (IEnumerable<T>)deferredSourceFactory.CreateDeferredSource(keyValues);
            }

            public override IEnumerable<T> GetNestedLinkSource<T>(int iGlobalLink, int iLocalFactory, object instance)
            {
                var deferredSourceFactory = (IDeferredSourceFactory)Locals[iLocalFactory];
                if (deferredSourceFactory == null)
                {
                    var member = (MetaDataMember)Globals[iGlobalLink];
                    deferredSourceFactory = services.GetDeferredSourceFactory(member);
                    Locals[iLocalFactory] = deferredSourceFactory;
                }
                return (IEnumerable<T>)deferredSourceFactory.CreateDeferredSource(instance);
            }

            public override object InsertLookup(int iMetaType, object instance)
            {
                var type = (MetaType)Globals[iMetaType];
                return services.InsertLookupCachedObject(type, instance);
            }

            public override bool Read()
            {
                if (isFinished)
                {
                    return false;
                }
                hasCurrentRow = BufferReader != null ? BufferReader.Read() : DataReader.Read();
                if (!hasCurrentRow)
                {
                    isFinished = true;
                    session.Finish(this);
                }
                hasRead = true;
                return hasCurrentRow;
            }

            public override void SendEntityMaterialized(int iMetaType, object instance)
            {
                var type = (MetaType)Globals[iMetaType];
                services.OnEntityMaterialized(type, instance);
            }

            // Properties
            public override bool CanDeferLoad
            {
                get { return services.Context.DeferredLoadingEnabled; }
            }

            internal bool IsBuffered
            {
                get { return (BufferReader != null); }
            }

            private SqlIdentifier SqlIdentifier
            {
                get
                {
                    return ((SqlProvider) services.Context.Provider).SqlIdentifier;
                }
            }
        }
    }
}
