using System;
using System.Collections.Generic;
using System.Data.Common;
using ALinq.Mapping;
using ALinq.SqlClient.Implementation;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Collections;

namespace ALinq.SqlClient
{
    internal partial class ObjectReaderCompiler : IObjectReaderCompiler
    {
        // Fields
        private readonly FieldInfo argsField;
        private readonly FieldInfo bufferReaderField;
        private static readonly LocalDataStoreSlot cacheSlot = Thread.AllocateDataSlot();
        private readonly Type dataReaderType;
        private readonly FieldInfo globalsField;
        private const int maxReaderCacheSize = 10;
        private readonly MethodInfo miBRisDBNull;
        private readonly MethodInfo miDRisDBNull;
        private readonly FieldInfo ordinalsField;
        private readonly FieldInfo readerField;
        private readonly IDataServices services;
        
        // Methods
        public ObjectReaderCompiler(Type dataReaderType, IDataServices services)
        {
            this.dataReaderType = dataReaderType;
            this.services = services;
            miDRisDBNull = dataReaderType.GetMethod("IsDBNull",
                                                         BindingFlags.NonPublic | BindingFlags.Public |
                                                         BindingFlags.Instance);
            miBRisDBNull = typeof(DbDataReader).GetMethod("IsDBNull",
                                                                BindingFlags.NonPublic | BindingFlags.Public |
                                                                BindingFlags.Instance);
            Type type = typeof(ObjectMaterializer<>).MakeGenericType(new[] { this.dataReaderType });
            ordinalsField = type.GetField("Ordinals", BindingFlags.Public | BindingFlags.Instance);
            globalsField = type.GetField("Globals", BindingFlags.Public | BindingFlags.Instance);
            argsField = type.GetField("Arguments", BindingFlags.Public | BindingFlags.Instance);
            readerField = type.GetField("DataReader", BindingFlags.Public | BindingFlags.Instance);
            bufferReaderField = type.GetField("BufferReader", BindingFlags.Public | BindingFlags.Instance);
        }

        public IObjectReaderFactory Compile(SqlExpression expression, Type elementType)
        {
            //-------------------------SOURCE CODE--------------------------
            object identity = this.services.Context.Mapping.Identity;
            //--------------------------------------------------------------
            //var bf = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty;
            //object identity = typeof(MetaModel).InvokeMember("Identity", bf, null, services.Context.Mapping, null);
            //--------------------------------------------------------------
            DataLoadOptions loadOptions = services.Context.LoadOptions;
            IObjectReaderFactory factory = null;
            ReaderFactoryCache data = null;
            bool flag = SqlProjectionComparer.CanBeCompared(expression);
            if (flag)
            {
                data = (ReaderFactoryCache)Thread.GetData(cacheSlot);
                if (data == null)
                {
                    data = new ReaderFactoryCache(maxReaderCacheSize);
                    Thread.SetData(cacheSlot, data);
                }
                factory = data.GetFactory(elementType, dataReaderType, identity, loadOptions, expression);
            }
            if (factory == null)
            {
                var gen = new Generator(this, elementType);
                DynamicMethod method = CompileDynamicMethod(gen, expression, elementType);
                var t = typeof(ObjectMaterializer<>).MakeGenericType(new[] { dataReaderType });
                var delegateType = typeof(Func<,>).MakeGenericType(new[] { t, elementType });
                var delegate2 = method.CreateDelegate(delegateType);

                factory = (IObjectReaderFactory)
                    Activator.CreateInstance(typeof(ObjectReaderFactory<,>).MakeGenericType(new[] { dataReaderType, elementType }),
                                             BindingFlags.NonPublic | BindingFlags.Instance, null,
                                             new object[] { delegate2, gen.NamedColumns, gen.Globals, gen.Locals }, null);
                if (flag)
                {
                    expression = new SourceExpressionRemover().VisitExpression(expression);
                    data.AddFactory(elementType, dataReaderType, identity, loadOptions, expression, factory);
                }
            }
            return factory;
        }

        //生成读取实体的方法。
        private DynamicMethod CompileDynamicMethod(Generator gen, SqlExpression expression, Type elementType)
        {
            Type type = typeof(ObjectMaterializer<>).MakeGenericType(new[] { dataReaderType });
            var method = new DynamicMethod("Read_" + elementType.Name, elementType, new[] { type }, true);
            gen.GenerateBody(method.GetILGenerator(), expression);
            return method;
        }

        public IObjectReaderSession CreateSession(DbDataReader reader, IReaderProvider provider, object[] parentArgs,
                                                  object[] userArgs, ICompiledSubQuery[] subQueries)
        {
            return (IObjectReaderSession)
                    Activator.CreateInstance(typeof(ObjectReaderSession<>).MakeGenericType(new[] { dataReaderType }),
                                             BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null,
                                             new object[] { reader, provider, parentArgs, userArgs, subQueries }, null);
        }

        private class ObjectReaderFactory<TDataReader, TObject> : IObjectReaderFactory where TDataReader : DbDataReader
        {
            // Fields
            private readonly Func<ObjectMaterializer<TDataReader>, TObject> fnMaterialize;
            private readonly object[] globals;
            private readonly NamedColumn[] namedColumns;
            private readonly int nLocals;

            // Methods
            internal ObjectReaderFactory(Func<ObjectMaterializer<TDataReader>, TObject> fnMaterialize,
                                         NamedColumn[] namedColumns, object[] globals, int nLocals)
            {
                this.fnMaterialize = fnMaterialize;
                this.namedColumns = namedColumns;
                this.globals = globals;
                this.nLocals = nLocals;
            }

            public IObjectReader Create(DbDataReader dataReader, bool disposeDataReader, IReaderProvider provider,
                                        object[] parentArgs, object[] userArgs, ICompiledSubQuery[] subQueries)
            {
                var session = new ObjectReaderSession<TDataReader>((TDataReader)dataReader, provider,
                                                                    parentArgs, userArgs, subQueries);
                return session.CreateReader(fnMaterialize, namedColumns, globals, nLocals,
                                                     disposeDataReader);
            }

            public IObjectReader GetNextResult(IObjectReaderSession session, bool disposeDataReader)
            {
                var session2 = (ObjectReaderSession<TDataReader>)session;
                IObjectReader reader = session2.GetNextResult(fnMaterialize, namedColumns,
                                                                       globals, nLocals, disposeDataReader);
                if ((reader == null) && disposeDataReader)
                {
                    session2.Dispose();
                }
                return reader;
            }
        }



        internal class OrderedResults<T> : IOrderedEnumerable<T>
        {
            // Fields
            private readonly List<T> values;

            // Methods
            internal OrderedResults(IEnumerable<T> results)
            {
                values = results as List<T> ?? new List<T>(results);
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return values.GetEnumerator();
            }

            IOrderedEnumerable<T> IOrderedEnumerable<T>.CreateOrderedEnumerable<K>(Func<T, K> keySelector,
                                                                                   IComparer<K> comparer,
                                                                                   bool descending)
            {
                throw Error.NotSupported();
            }
        }





        private class SideEffectChecker : SqlVisitor
        {
            // Fields
            private bool hasSideEffect;

            // Methods
            internal bool HasSideEffect(SqlNode node)
            {
                hasSideEffect = false;
                Visit(node);
                return hasSideEffect;
            }

            internal override SqlExpression VisitClientQuery(SqlClientQuery cq)
            {
                return cq;
            }

            internal override SqlExpression VisitJoinedCollection(SqlJoinedCollection jc)
            {
                hasSideEffect = true;
                return jc;
            }
        }

        private class SourceExpressionRemover : SqlDuplicator.DuplicatingVisitor
        {
            // Methods
            internal SourceExpressionRemover()
                : base(true)
            {
            }

            internal override SqlNode Visit(SqlNode node)
            {
                node = base.Visit(node);
                if (node != null)
                {
                    node.ClearSourceExpression();
                }
                return node;
            }

            internal override SqlExpression VisitAliasRef(SqlAliasRef aref)
            {
                SqlExpression expression = base.VisitAliasRef(aref);
                if ((expression != null) && (expression == aref))
                {
                    SqlAlias alias = aref.Alias;
                    return new SqlAliasRef(new SqlAlias(new SqlNop(aref.ClrType, aref.SqlType, null)));
                }
                return expression;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                SqlExpression expression = base.VisitColumnRef(cref);
                if ((expression != null) && (expression == cref))
                {
                    SqlColumn column = cref.Column;
                    SqlColumn col;
                    //if (column is SqlDynamicColumn)
                    //{
                    //    col = new SqlDynamicColumn(column.ClrType, column.SqlType, column.Name, column.MetaMember, null, column.SourceExpression);
                    //}
                    //else
                    //{
                    col = new SqlColumn(column.ClrType, column.SqlType, column.Name, column.MetaMember, null, column.SourceExpression);
                    //}

                    col.Ordinal = column.Ordinal;
                    expression = new SqlColumnRef(col);
                    col.ClearSourceExpression();
                }
                return expression;
            }
        }

        internal class SqlProjectionComparer
        {
            // Methods
            internal static bool AreSimilar(SqlExpression node1, SqlExpression node2)
            {
                if (node1 == node2)
                {
                    return true;
                }
                if ((node1 != null) && (node2 != null))
                {
                    if (((node1.NodeType != node2.NodeType) || (node1.ClrType != node2.ClrType)) ||
                        (node1.SqlType != node2.SqlType))
                    {
                        return false;
                    }
                    switch (node1.NodeType)
                    {
                        case SqlNodeType.ClientArray:
                            {
                                var array = (SqlClientArray)node1;
                                var array2 = (SqlClientArray)node2;
                                if (array.Expressions.Count == array2.Expressions.Count)
                                {
                                    int num7 = 0;
                                    int count = array.Expressions.Count;
                                    while (num7 < count)
                                    {
                                        if (!AreSimilar(array.Expressions[num7], array2.Expressions[num7]))
                                        {
                                            return false;
                                        }
                                        num7++;
                                    }
                                    return true;
                                }
                                return false;
                            }
                        case SqlNodeType.ClientCase:
                            {
                                var @case = (SqlClientCase)node1;
                                var case2 = (SqlClientCase)node2;
                                if (@case.Whens.Count == case2.Whens.Count)
                                {
                                    int num9 = 0;
                                    int num10 = @case.Whens.Count;
                                    while (num9 < num10)
                                    {
                                        if (!AreSimilar(@case.Whens[num9].Match, case2.Whens[num9].Match) ||
                                            !AreSimilar(@case.Whens[num9].Value, case2.Whens[num9].Value))
                                        {
                                            return false;
                                        }
                                        num9++;
                                    }
                                    return true;
                                }
                                return false;
                            }
                        case SqlNodeType.ClientQuery:
                            {
                                var query = (SqlClientQuery)node1;
                                var query2 = (SqlClientQuery)node2;
                                if (query.Arguments.Count == query2.Arguments.Count)
                                {
                                    int num15 = 0;
                                    int num16 = query.Arguments.Count;
                                    while (num15 < num16)
                                    {
                                        if (!AreSimilar(query.Arguments[num15], query2.Arguments[num15]))
                                        {
                                            return false;
                                        }
                                        num15++;
                                    }
                                    return true;
                                }
                                return false;
                            }
                        case SqlNodeType.ColumnRef:
                            {
                                var ref2 = (SqlColumnRef)node1;
                                var ref3 = (SqlColumnRef)node2;
                                return (ref2.Column.Ordinal == ref3.Column.Ordinal);
                            }
                        case SqlNodeType.DiscriminatedType:
                            {
                                var type = (SqlDiscriminatedType)node1;
                                var type2 = (SqlDiscriminatedType)node2;
                                return AreSimilar(type.Discriminator, type2.Discriminator);
                            }
                        case SqlNodeType.Lift:
                            return AreSimilar(((SqlLift)node1).Expression, ((SqlLift)node2).Expression);

                        case SqlNodeType.Link:
                            {
                                var link = (SqlLink)node1;
                                var link2 = (SqlLink)node2;
                                if (MetaPosition.AreSameMember(link.Member.Member, link2.Member.Member))
                                {
                                    if (link.KeyExpressions.Count != link2.KeyExpressions.Count)
                                    {
                                        return false;
                                    }
                                    int num5 = 0;
                                    int num6 = link.KeyExpressions.Count;
                                    while (num5 < num6)
                                    {
                                        if (!AreSimilar(link.KeyExpressions[num5], link2.KeyExpressions[num5]))
                                        {
                                            return false;
                                        }
                                        num5++;
                                    }
                                    return true;
                                }
                                return false;
                            }
                        case SqlNodeType.Grouping:
                            {
                                var grouping = (SqlGrouping)node1;
                                var grouping2 = (SqlGrouping)node2;
                                return (AreSimilar(grouping.Key, grouping2.Key) &&
                                        AreSimilar(grouping.Group, grouping2.Group));
                            }
                        case SqlNodeType.JoinedCollection:
                            {
                                var joineds = (SqlJoinedCollection)node1;
                                var joineds2 = (SqlJoinedCollection)node2;
                                if (!AreSimilar(joineds.Count, joineds2.Count))
                                {
                                    return false;
                                }
                                return AreSimilar(joineds.Expression, joineds2.Expression);
                            }
                        case SqlNodeType.MethodCall:
                            {
                                var call = (SqlMethodCall)node1;
                                var call2 = (SqlMethodCall)node2;
                                if ((call.Method == call2.Method) && AreSimilar(call.Object, call2.Object))
                                {
                                    if (call.Arguments.Count != call2.Arguments.Count)
                                    {
                                        return false;
                                    }
                                    int num17 = 0;
                                    int num18 = call.Arguments.Count;
                                    while (num17 < num18)
                                    {
                                        if (!AreSimilar(call.Arguments[num17], call2.Arguments[num17]))
                                        {
                                            return false;
                                        }
                                        num17++;
                                    }
                                    return true;
                                }
                                return false;
                            }
                        case SqlNodeType.Member:
                            {
                                var member = (SqlMember)node1;
                                var member2 = (SqlMember)node2;
                                if (member.Member != member2.Member)
                                {
                                    return false;
                                }
                                return AreSimilar(member.Expression, member2.Expression);
                            }
                        case SqlNodeType.OptionalValue:
                            {
                                var value2 = (SqlOptionalValue)node1;
                                var value3 = (SqlOptionalValue)node2;
                                return AreSimilar(value2.Value, value3.Value);
                            }
                        case SqlNodeType.OuterJoinedValue:
                        case SqlNodeType.ValueOf:
                            return AreSimilar(((SqlUnary)node1).Operand, ((SqlUnary)node2).Operand);

                        case SqlNodeType.SearchedCase:
                            {
                                var case3 = (SqlSearchedCase)node1;
                                var case4 = (SqlSearchedCase)node2;
                                if (case3.Whens.Count != case4.Whens.Count)
                                {
                                    return false;
                                }
                                int num11 = 0;
                                int num12 = case3.Whens.Count;
                                while (num11 < num12)
                                {
                                    if (!AreSimilar(case3.Whens[num11].Match, case4.Whens[num11].Match) ||
                                        !AreSimilar(case3.Whens[num11].Value, case4.Whens[num11].Value))
                                    {
                                        return false;
                                    }
                                    num11++;
                                }
                                return AreSimilar(case3.Else, case4.Else);
                            }
                        case SqlNodeType.New:
                            {
                                var new2 = (SqlNew)node1;
                                var new3 = (SqlNew)node2;
                                if ((new2.Args.Count != new3.Args.Count) || (new2.Members.Count != new3.Members.Count))
                                {
                                    return false;
                                }
                                int num = 0;
                                int num2 = new2.Args.Count;
                                while (num < num2)
                                {
                                    if (!AreSimilar(new2.Args[num], new3.Args[num]))
                                    {
                                        return false;
                                    }
                                    num++;
                                }
                                int num3 = 0;
                                int num4 = new2.Members.Count;
                                while (num3 < num4)
                                {
                                    if (
                                        !MetaPosition.AreSameMember(new2.Members[num3].Member, new3.Members[num3].Member) ||
                                        !AreSimilar(new2.Members[num3].Expression, new3.Members[num3].Expression))
                                    {
                                        return false;
                                    }
                                    num3++;
                                }
                                return true;
                            }
                        case SqlNodeType.Value:
                            return Equals(((SqlValue)node1).Value, ((SqlValue)node2).Value);

                        case SqlNodeType.UserColumn:
                            return (((SqlUserColumn)node1).Name == ((SqlUserColumn)node2).Name);

                        case SqlNodeType.TypeCase:
                            {
                                var case5 = (SqlTypeCase)node1;
                                var case6 = (SqlTypeCase)node2;
                                if (!AreSimilar(case5.Discriminator, case6.Discriminator))
                                {
                                    return false;
                                }
                                if (case5.Whens.Count != case6.Whens.Count)
                                {
                                    return false;
                                }
                                int num13 = 0;
                                int num14 = case5.Whens.Count;
                                while (num13 < num14)
                                {
                                    if (!AreSimilar(case5.Whens[num13].Match, case6.Whens[num13].Match))
                                    {
                                        return false;
                                    }
                                    if (!AreSimilar(case5.Whens[num13].TypeBinding, case6.Whens[num13].TypeBinding))
                                    {
                                        return false;
                                    }
                                    num13++;
                                }
                                return true;
                            }
                    }
                }
                return false;
            }

            internal static bool CanBeCompared(SqlExpression node)
            {
                if (node == null)
                {
                    return true;
                }
                switch (node.NodeType)
                {
                    case SqlNodeType.ClientArray:
                        {
                            var array = (SqlClientArray)node;
                            int num7 = 0;
                            int count = array.Expressions.Count;
                            while (num7 < count)
                            {
                                if (!CanBeCompared(array.Expressions[num7]))
                                {
                                    return false;
                                }
                                num7++;
                            }
                            return true;
                        }
                    case SqlNodeType.ClientCase:
                        {
                            var @case = (SqlClientCase)node;
                            int num9 = 0;
                            int num10 = @case.Whens.Count;
                            while (num9 < num10)
                            {
                                if (!CanBeCompared(@case.Whens[num9].Match) || !CanBeCompared(@case.Whens[num9].Value))
                                {
                                    return false;
                                }
                                num9++;
                            }
                            return true;
                        }
                    case SqlNodeType.ClientQuery:
                        return true;

                    case SqlNodeType.ColumnRef:
                    case SqlNodeType.Value:
                    case SqlNodeType.UserColumn:
                        return true;

                    case SqlNodeType.DiscriminatedType:
                        return CanBeCompared(((SqlDiscriminatedType)node).Discriminator);

                    case SqlNodeType.Lift:
                        return CanBeCompared(((SqlLift)node).Expression);

                    case SqlNodeType.Link:
                        {
                            var link = (SqlLink)node;
                            int num5 = 0;
                            int num6 = link.KeyExpressions.Count;
                            while (num5 < num6)
                            {
                                if (!CanBeCompared(link.KeyExpressions[num5]))
                                {
                                    return false;
                                }
                                num5++;
                            }
                            return true;
                        }
                    case SqlNodeType.Grouping:
                        {
                            var grouping = (SqlGrouping)node;
                            return (CanBeCompared(grouping.Key) && CanBeCompared(grouping.Group));
                        }
                    case SqlNodeType.JoinedCollection:
                        {
                            var joineds = (SqlJoinedCollection)node;
                            if (!CanBeCompared(joineds.Count))
                            {
                                return false;
                            }
                            return CanBeCompared(joineds.Expression);
                        }
                    case SqlNodeType.MethodCall:
                        {
                            var call = (SqlMethodCall)node;
                            if ((call.Object == null) || CanBeCompared(call.Object))
                            {
                                int num15 = 0;
                                int num16 = call.Arguments.Count;
                                while (num15 < num16)
                                {
                                    if (!CanBeCompared(call.Arguments[0]))
                                    {
                                        return false;
                                    }
                                    num15++;
                                }
                                return true;
                            }
                            return false;
                        }
                    case SqlNodeType.Member:
                        return CanBeCompared(((SqlMember)node).Expression);

                    case SqlNodeType.OptionalValue:
                        return CanBeCompared(((SqlOptionalValue)node).Value);

                    case SqlNodeType.OuterJoinedValue:
                    case SqlNodeType.ValueOf:
                        return CanBeCompared(((SqlUnary)node).Operand);

                    case SqlNodeType.SearchedCase:
                        {
                            var case2 = (SqlSearchedCase)node;
                            int num11 = 0;
                            int num12 = case2.Whens.Count;
                            while (num11 < num12)
                            {
                                if (!CanBeCompared(case2.Whens[num11].Match) || !CanBeCompared(case2.Whens[num11].Value))
                                {
                                    return false;
                                }
                                num11++;
                            }
                            return CanBeCompared(case2.Else);
                        }
                    case SqlNodeType.New:
                        {
                            var new2 = (SqlNew)node;
                            int num = 0;
                            int num2 = new2.Args.Count;
                            while (num < num2)
                            {
                                if (!CanBeCompared(new2.Args[num]))
                                {
                                    return false;
                                }
                                num++;
                            }
                            int num3 = 0;
                            int num4 = new2.Members.Count;
                            while (num3 < num4)
                            {
                                if (!CanBeCompared(new2.Members[num3].Expression))
                                {
                                    return false;
                                }
                                num3++;
                            }
                            return true;
                        }
                    case SqlNodeType.TypeCase:
                        {
                            var case3 = (SqlTypeCase)node;
                            if (!CanBeCompared(case3.Discriminator))
                            {
                                return false;
                            }
                            int num13 = 0;
                            int num14 = case3.Whens.Count;
                            while (num13 < num14)
                            {
                                if (!CanBeCompared(case3.Whens[num13].Match))
                                {
                                    return false;
                                }
                                if (!CanBeCompared(case3.Whens[num13].TypeBinding))
                                {
                                    return false;
                                }
                                num13++;
                            }
                            return true;
                        }
                }
                return false;
            }
        }
    }
}