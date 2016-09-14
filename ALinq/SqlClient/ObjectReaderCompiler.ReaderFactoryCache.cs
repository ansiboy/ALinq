using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ALinq.SqlClient
{
    internal partial class ObjectReaderCompiler
    {
        private class ReaderFactoryCache
        {
            // Fields
            private readonly LinkedList<CacheInfo> list;
            private readonly int maxCacheSize;

            // Methods
            internal ReaderFactoryCache(int maxCacheSize)
            {
                this.maxCacheSize = maxCacheSize;
                list = new LinkedList<CacheInfo>();
            }

            internal void AddFactory(Type elementType, Type dataReaderType, object mapping, DataLoadOptions options,
                                     SqlExpression projection, IObjectReaderFactory factory)
            {
                list.AddFirst(new LinkedListNode<CacheInfo>(new CacheInfo(elementType, dataReaderType, mapping, options,
                                                                projection, factory)));
                if (list.Count > maxCacheSize)
                {
                    list.RemoveLast();
                }
            }

            internal IObjectReaderFactory GetFactory(Type elementType, Type dataReaderType, object mapping,
                                                     DataLoadOptions options, SqlExpression projection)
            {
                for (LinkedListNode<CacheInfo> node = list.First; node != null; node = node.Next)
                {
                    if ((((elementType == node.Value.elementType) && (dataReaderType == node.Value.dataReaderType)) &&
                         ((mapping == node.Value.mapping) && ShapesAreSimilar(options, node.Value.options))) &&
                           SqlProjectionComparer.AreSimilar(projection, node.Value.projection))
                    {
                        list.Remove(node);
                        list.AddFirst(node);
                        return node.Value.factory;
                    }
                }
                return null;
            }

            private static bool ShapesAreSimilar(DataLoadOptions ds1, DataLoadOptions ds2)
            {
                if (ds1 != ds2)
                {
                    if ((ds1 != null) && !ds1.IsEmpty)
                    {
                        return false;
                    }
                    if (ds2 != null)
                    {
                        return ds2.IsEmpty;
                    }
                }
                return true;
            }


            // Nested Types
            private class CacheInfo
            {
                // Fields
                internal readonly Type dataReaderType;
                internal readonly Type elementType;
                internal readonly IObjectReaderFactory factory;
                internal readonly object mapping;
                internal readonly DataLoadOptions options;
                internal readonly SqlExpression projection;

                // Methods
                public CacheInfo(Type elementType, Type dataReaderType, object mapping, DataLoadOptions options,
                                 SqlExpression projection, IObjectReaderFactory factory)
                {
                    this.elementType = elementType;
                    this.dataReaderType = dataReaderType;
                    this.options = options;
                    this.mapping = mapping;
                    this.projection = projection;
                    this.factory = factory;
                }
            }
        }
    }
}
