using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ALinq;
using ALinq.SqlClient;
using System.ComponentModel;
using ALinq.Mapping;
using ALinq.Provider;

namespace ALinq.SqlClient
{
    ///<summary>
    /// Contains functionality to connect to and communicate with a SQL Server.
    ///</summary>
    public abstract partial class SqlProvider : IReaderProvider, IConnectionUser, IProviderExtend
    {
        internal IDataServices services;
        internal Translator translator;
        internal SqlFactory sqlFactory;

        internal ISqlConnectionManager conManager;
        internal IObjectReaderCompiler readerCompiler;

        internal ITypeSystemProvider typeProvider;
        private bool disposed;
        private bool deleted;
        private string dbName;
        private bool checkQueries;
        private OptimizationFlags optimizationFlags;
        protected int queryCount;
        private TextWriter log;
        private bool enableCacheLookup;

        internal SqlProvider()
            : this(ProviderMode.NotYetDecided)
        {
        }

        internal SqlProvider(ProviderMode mode)
        {
            optimizationFlags = OptimizationFlags.All;
            enableCacheLookup = true;
            this.Mode = mode;
        }

        protected static int GetTabesCount(DbConnection conn)
        {
            string tableType;
            string typeColumn = "TABLE_TYPE";
            string connTypeName = conn.GetType().Name;
            //注意：区分大小写
            switch (connTypeName)
            {
                case "FbConnection":
                case "OleDbConnection":
                case "SQLiteConnection":
                case "DB2Connection":
                    tableType = "TABLE";
                    break;

                case "SqlConnection":
                case "MySqlConnection":
                case "NpgsqlConnection":
                    tableType = "BASE TABLE";
                    break;

                case "OracleConnection":
                    tableType = "User";
                    typeColumn = "TYPE";
                    break;
                default:
                    return 10000;
            }
            var dt = conn.GetSchema("Tables");
            IEnumerable<DataRow> rows;
            if (connTypeName == "NpgsqlConnection")
            {
                rows = dt.Rows.Cast<DataRow>().Where(o => string.Compare(o["TABLE_TYPE"] as string, "BASE TABLE", true) == 0 &&
                                                          string.Compare(o["TABLE_SCHEMA"] as string, "public") == 0);
            }
            else if (connTypeName == "SqlConnection")
            {
                rows = dt.Rows.Cast<DataRow>().Where(o => string.Compare(o[typeColumn] as string, tableType, true) == 0 &&
                                                          o["TABLE_NAME"] as string != "sysdiagrams");
            }
            else
            {
                rows = dt.Rows.Cast<DataRow>().Where(o => string.Compare(o[typeColumn] as string, tableType, true) == 0);
            }
            return rows.Count();
        }

        private static bool validateTablesNumbers = false;
        internal virtual IExecuteResult Execute(Expression query, QueryInfo queryInfo, IObjectReaderFactory factory,
                                                 object[] parentArgs, object[] userArgs, ICompiledSubQuery[] subQueries,
                                                 object lastResult)
        {
            IExecuteResult result3;
            InitializeProviderMode();
            DbConnection connection = conManager.UseConnection(this);
            try
            {
                //Debug.Assert(License != null);

                ////注：Free版每次都要验证表的数量
                //if (this.License.LicenseType == LicenseType.Free)
                //{
                //    var count = GetTabesCount(connection);
                //    Debug.Assert(count > 0);

                //    if (count > Constants.FreeEdition_LimitedTablesCount)
                //        throw Error.TablesLimited(this.license.LicenseType, Constants.FreeEdition_LimitedTablesCount);
                //}
                //else
                //{

                //    if (!validateTablesNumbers)
                //    {
                //        if (License.Culture != null && License.Culture.Name == "zh-CN")
                //        {
                //            int count = 0;
                //            // Trial & Site License 是不需要验证的
                //            if (license.LicenseType != LicenseType.Trial && license.LicenseType != LicenseType.Site)
                //                count = GetTabesCount(connection);

                //            int expected = 0;

                //            switch (license.LicenseType)
                //            {
                //                case LicenseType.Experience:
                //                    expected = Constants.ExperienceEdition_LimitedTablesCount;
                //                    break;
                //                //=============================================================
                //                // 注：这段可以不要，但是加了也没有错。
                //                case LicenseType.Free:
                //                    expected = Constants.FreeEdition_LimitedTablesCount;
                //                    break;
                //                //=============================================================
                //                case LicenseType.Single:
                //                    expected = Constants.SingleEdition_LimitedTablesCount;
                //                    break;
                //                //=============================================================
                //                // 注：Site License 是不需要验证的
                //                //case LicenseType.Site:
                //                //    break;
                //                //=============================================================
                //                case LicenseType.Standard:
                //                    expected = Constants.StandardEdition_LimitedTablesCount;
                //                    break;
                //                case LicenseType.Team:
                //                    expected = Constants.TeamEdition_LimitedTablesCount;
                //                    break;
                //                //=============================================================
                //                // 注：Trial License 是不需要验证的
                //                //case LicenseType.Trial:
                //                //    break;
                //                //=============================================================
                //            }
                //            if (count > expected)
                //                throw Error.TablesLimited(this.license.LicenseType, expected);

                //        }

                //        validateTablesNumbers = true;
                //    }
                //}


                DbCommand cmd = connection.CreateCommand();
                cmd.CommandText = queryInfo.CommandText;

                if (Mode == ProviderMode.Firebird || Mode == ProviderMode.Oracle)
                {
                    cmd.CommandType = (queryInfo.Query is SqlStoredProcedureCall)
                                             ? CommandType.StoredProcedure
                                             : CommandType.Text;
                }
                cmd.Transaction = conManager.Transaction;
                cmd.CommandTimeout = CommandTimeout;
                AssignParameters(cmd, queryInfo.Parameters, userArgs, lastResult);

                LogCommand(Log, cmd);
                queryCount++;
                switch (queryInfo.ResultShape)
                {
                    case ResultShape.Singleton:
                        {
                            DbDataReader reader = CreateRereader(cmd.ExecuteReader());
                            IObjectReader reader2 = factory.Create(reader, true, this, parentArgs, userArgs, subQueries);
                            conManager.UseConnection(reader2.Session);
                            try
                            {
                                var objType = typeof(OneTimeEnumerable<>).MakeGenericType(new[] { queryInfo.ResultType });
                                var bf1 = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                                var args = new object[] { reader2 };
                                var sequence =
                                    (IEnumerable)Activator.CreateInstance(objType, bf1, null, args, null);
                                object obj2 = null;
                                var expression = query as MethodCallExpression;
                                MethodInfo info;
                                if ((expression != null) && ((expression.Method.DeclaringType == typeof(Queryable)) ||
                                                             (expression.Method.DeclaringType == typeof(Enumerable))))
                                {
                                    string name = expression.Method.Name;
                                    switch (name)
                                    {
                                        case "SingleOrDefault":
                                        case "FirstOrDefault":
                                        case "First":
                                            info = TypeSystem.FindSequenceMethod(expression.Method.Name, sequence);
                                            break;
                                        case "Single":
                                        default:
                                            //Debug.Assert(name == "Single");
                                            info = TypeSystem.FindSequenceMethod("Single", sequence);
                                            break;
                                    }

                                }
                                else
                                {
                                    info = TypeSystem.FindSequenceMethod("SingleOrDefault", sequence);
                                }
                                if (info != null)
                                {
                                    try
                                    {
                                        obj2 = info.Invoke(null, new object[] { sequence });
                                    }
                                    catch (TargetInvocationException exception)
                                    {
                                        if (exception.InnerException != null)
                                        {
                                            throw exception.InnerException;
                                        }
                                        throw;
                                    }
                                }
                                return CreateExecuteResult(cmd, queryInfo.Parameters, reader2.Session, obj2);
                            }
                            finally
                            {
                                reader2.Dispose();
                            }
                        }
                    case ResultShape.Sequence:
                        break;

                    case ResultShape.MultipleResults:
                        {
                            DbDataReader reader5 = CreateRereader(cmd.ExecuteReader());
                            IObjectReaderSession user = readerCompiler.CreateSession(reader5, this, parentArgs,
                                                                                          userArgs, subQueries);
                            conManager.UseConnection(user);
                            MetaFunction function2 = GetFunction(query);
                            var executeResult = CreateExecuteResult(cmd, queryInfo.Parameters, user);
                            executeResult.ReturnValue = new MultipleResults(this, function2, user, executeResult);
                            return executeResult;
                        }
                    default:
                        if (Mode == ProviderMode.Access && queryInfo.Query is SqlStoredProcedureCall)
                        {
                            cmd.CommandType = CommandType.Text;
                            return CreateExecuteResult(cmd, queryInfo.Parameters, null, cmd.ExecuteScalar(), false);
                        }
                        return CreateExecuteResult(cmd, queryInfo.Parameters, null, cmd.ExecuteNonQuery(), true);
                }

                DbDataReader reader3 = CreateRereader(cmd.ExecuteReader());
                IObjectReader reader4 = factory.Create(reader3, true, this, parentArgs, userArgs, subQueries);
                conManager.UseConnection(reader4.Session);
                var t = typeof(OneTimeEnumerable<>).MakeGenericType(new[]{
                                                                             TypeSystem.GetElementType(queryInfo.ResultType)
                                                                         });
                var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                var objs = new object[] { reader4 };
                var source = (IEnumerable)Activator.CreateInstance(t, flags, null, objs, null);
                if (typeof(IQueryable).IsAssignableFrom(queryInfo.ResultType))
                    source = source.AsQueryable();

                var result = CreateExecuteResult(cmd, queryInfo.Parameters, reader4.Session);
                MetaFunction function = GetFunction(query);
                if ((function != null) && !function.IsComposable)
                {
                    t = typeof(SingleResult<>).MakeGenericType(new[]{
                                                                        TypeSystem.GetElementType(queryInfo.ResultType)
                                                                    });
                    flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                    objs = new object[] { source, result, services.Context };
                    source = (IEnumerable)Activator.CreateInstance(t, flags, null, objs, null);
                }
                result.ReturnValue = source;
                result3 = result;
            }
            finally
            {
                conManager.ReleaseConnection(this);
            }
            return result3;
        }

        protected virtual DbDataReader CreateRereader(DbDataReader reader)
        {
            return reader;
        }


        protected MetaFunction GetFunction(Expression query)
        {
            var expression = query as LambdaExpression;
            if (expression != null)
                query = expression.Body;

            var expression2 = query as MethodCallExpression;
            if ((expression2 != null) && typeof(DataContext).IsAssignableFrom(expression2.Method.DeclaringType))
            {
                return services.Model.GetFunction(expression2.Method);
            }
            return null;
        }

        protected int QueryCount
        {
            get
            {
                CheckDispose();
                return queryCount;
            }
            set { queryCount = value; }
        }


        internal OptimizationFlags OptimizationFlags
        {
            get
            {
                CheckDispose();
                return optimizationFlags;
            }
            set
            {
                CheckDispose();
                optimizationFlags = value;
            }
        }

        internal virtual void AssignParameters(DbCommand cmd, ReadOnlyCollection<SqlParameterInfo> parms,
                                                object[] userArguments, object lastResult)
        {
            if (parms != null)
            {
                //TODO:Hack Code
                List<string> list = null;
                if (cmd.CommandText.StartsWith("DELETE"))
                {
                    list = new List<string>();
                    var match = Regex.Match(cmd.CommandText, string.Format("[{0}][a-z][0-9]+", SqlIdentifier.ParameterPrefix));
                    while (match.Success)
                    {
                        list.Add(match.Value);
                        match = match.NextMatch();
                    }
                    if (list.Count < parms.Count)
                        parms = new ReadOnlyCollection<SqlParameterInfo>(parms.Where(o => list.Contains(o.Parameter.Name)).ToArray());
                }
                //
                foreach (SqlParameterInfo info in parms)
                {
                    DbParameter parameter = cmd.CreateParameter();
                    parameter.ParameterName = info.Parameter.Name;
                    parameter.Direction = info.Parameter.Direction;
                    if ((info.Parameter.Direction != ParameterDirection.Input) &&
                        (info.Parameter.Direction != ParameterDirection.InputOutput))
                    {
                        goto Label_00C4;
                    }
                    object obj2 = info.Value;
                    switch (info.Type)
                    {
                        case SqlParameterType.UserArgument:
                            try
                            {
                                obj2 = info.Accessor.DynamicInvoke(new object[] { userArguments });
                                goto Label_00AA;
                            }
                            catch (TargetInvocationException exception)
                            {
                                throw exception.InnerException;
                            }
                        case SqlParameterType.PreviousResult:
                            break;

                        default:
                            goto Label_00AA;
                    }
                    obj2 = lastResult;
                Label_00AA:
                    if (info.Parameter.SqlType.IsRuntimeOnlyType && (obj2 == null))
                        parameter.Value = DBNull.Value;
                    else
                        typeProvider.InitializeParameter(info.Parameter.SqlType, parameter, obj2);
                    goto Label_00DC;
                Label_00C4:
                    typeProvider.InitializeParameter(info.Parameter.SqlType, parameter, null);
                Label_00DC:
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        internal IExecuteResult ExecuteAll(Expression query, QueryInfo[] queryInfos, IObjectReaderFactory factory,
                                            object[] userArguments, ICompiledSubQuery[] subQueries)
        {
            IExecuteResult result = null;
            object lastResult = null;
            int index = 0;
            int length = queryInfos.Length;
            while (index < length)
            {
                result = index < (length - 1) ? Execute(query, queryInfos[index], null, null, userArguments, subQueries, lastResult) :
                                                Execute(query, queryInfos[index], factory, null, userArguments, subQueries, lastResult);
                var resultShape = queryInfos[index].ResultShape;
                //QueryInfo.GetResultShape(arrQueryInfos.GetValue(index));
                if (resultShape == ResultShape.Return)
                {
                    lastResult = result.ReturnValue;
                }
                index++;
            }
            return result;
        }

        internal IList<QueryInfo> BuildQuery(Expression query)
        {
            var annotations = new SqlNodeAnnotations();
            var querys = BuildQuery(query, annotations);
            return querys;
        }

        internal virtual QueryInfo[] BuildQuery(Expression query, SqlNodeAnnotations annotations)
        {
            CheckDispose();
            query = Funcletizer.Funcletize(query);
            var converter = CreateQueryConverter(sqlFactory);
            switch (Mode)
            {
                case ProviderMode.Sql2000:
                    converter.ConverterStrategy = ConverterStrategy.CanUseJoinOn | ConverterStrategy.CanUseRowStatus |
                                                  ConverterStrategy.CanUseScopeIdentity;
                    break;
                case ProviderMode.Sql2005:
                    converter.ConverterStrategy = ConverterStrategy.CanOutputFromInsert | ConverterStrategy.CanUseJoinOn |
                                                  ConverterStrategy.CanUseRowStatus | ConverterStrategy.CanUseOuterApply |
                                                  ConverterStrategy.CanUseScopeIdentity |
                                                  ConverterStrategy.SkipWithRowNumber;
                    break;
                case ProviderMode.SqlCE:
                    converter.ConverterStrategy = ConverterStrategy.CanUseOuterApply;
                    break;
            }

            //var model = services.Model as DynamicModel;
            //if (model != null)
            //{
            //    model.Update(query);
            //}

            SqlNode node = converter.ConvertOuter(query);
            return BuildQuery(GetResultShape(query), GetResultType(query), node, null,
                              annotations);
        }

        internal abstract QueryConverter CreateQueryConverter(SqlFactory sql);


        internal virtual QueryInfo[] BuildQuery(ResultShape resultShape, Type resultType, SqlNode node, ReadOnlyCollection<SqlParameter> parentParameters, SqlNodeAnnotations annotations)
        {
            var model = services.Model;
            var validator = new SqlSupersetValidator();
            checkQueries = true;
            if (checkQueries)
            {
                validator.AddValidator(new ColumnTypeValidator());
                validator.AddValidator(new LiteralValidator());
            }
            validator.Validate(node);
            var columnizer = new SqlColumnizer(CanBeColumn);
            var binder = new SqlBinder(translator, sqlFactory, model,
                                             services.Context.LoadOptions, columnizer)
                             {
                                 OptimizeLinkExpansions =
                                     ((optimizationFlags & OptimizationFlags.OptimizeLinkExpansions) !=
                                      OptimizationFlags.None),
                                 SimplifyCaseStatements =
                                     ((optimizationFlags & OptimizationFlags.SimplifyCaseStatements) !=
                                      OptimizationFlags.None),
                                 PreBinder =
                                     (n => PreBindDotNetConverter.Convert(n, sqlFactory, model))
                             };
            node = binder.Bind(node);
            if (checkQueries)
            {
                validator.AddValidator(new ExpectNoAliasRefs());
                validator.AddValidator(new ExpectNoSharedExpressions());
            }
            validator.Validate(node);

            node = PostBindDotNetConverter.Convert(node, sqlFactory, this);
            var retyper = new SqlRetyper(typeProvider, model);
            node = retyper.Retype(node);
            validator.Validate(node);
            node = new SqlTypeConverter(sqlFactory).Visit(node);
            validator.Validate(node);
            node = new SqlMethodTransformer(sqlFactory).Visit(node);
            validator.Validate(node);
            SqlMultiplexer.Options options;

            if (Mode == ProviderMode.Sql2000)
                options = SqlMultiplexer.Options.None;
            else
                options = SqlMultiplexer.Options.EnableBigJoin;

            node = new SqlMultiplexer(options, parentParameters, sqlFactory, SqlIdentifier).Multiplex(node);
            validator.Validate(node);
            node = new SqlFlattener(sqlFactory, columnizer).Flatten(node);
            validator.Validate(node);
            if (Mode == ProviderMode.SqlCE)
            {
                node = new SqlRewriteScalarSubqueries(sqlFactory).Rewrite(node);
            }
            node = SqlCaseSimplifier.Simplify(node, sqlFactory);
            node = new SqlReorderer(typeProvider, sqlFactory).Reorder(node);
            validator.Validate(node);
            node = SqlBooleanizer.Rationalize(this, node, typeProvider, model);
            if (checkQueries)
            {
                validator.AddValidator(new ExpectRationalizedBooleans());
            }
            validator.Validate(node);
            if (checkQueries)
            {
                validator.AddValidator(new ExpectNoFloatingColumns());
            }
            node = retyper.Retype(node);
            validator.Validate(node);
            var aliaser = new SqlAliaser();
            node = aliaser.AssociateColumnsWithAliases(node);
            validator.Validate(node);
            node = SqlLiftWhereClauses.Lift(node, typeProvider, model);
            node = SqlLiftIndependentRowExpressions.Lift(node);
            node = SqlOuterApplyReducer.Reduce(node, sqlFactory, annotations);
            node = SqlTopReducer.Reduce(node, annotations, sqlFactory);

            node = new SqlResolver().Resolve(node);
            validator.Validate(node);
            node = aliaser.AssociateColumnsWithAliases(node);
            validator.Validate(node);
            node = SqlUnionizer.Unionize(node);
            node = SqlRemoveConstantOrderBy.Remove(node);
            node = new SqlDeflator().Deflate(node);
            validator.Validate(node);
            node = SqlCrossApplyToCrossJoin.Reduce(node, annotations);
            node = new SqlNamer(Mode).AssignNames(node);
            validator.Validate(node);
            node = new LongTypeConverter(sqlFactory).AddConversions(node, annotations);
            validator.AddValidator(new ExpectNoMethodCalls());
            validator.AddValidator(new ValidateNoInvalidComparison());
            validator.Validate(node);
            var parameterizer = CreateSqlParameterizer(typeProvider, annotations);
            var formatter = CreateSqlFormatter();
            formatter.SetIdentifier(SqlIdentifier);
            if ((Mode == ProviderMode.SqlCE) || (Mode == ProviderMode.Sql2005))
            {
                formatter.ParenthesizeTop = true;
            }
            var block = node as SqlBlock;
            if ((block != null) && (Mode == ProviderMode.SqlCE || Mode == ProviderMode.Access ||
                                    Mode == ProviderMode.Oracle || Mode == ProviderMode.Firebird ||
                                    Mode == ProviderMode.OdpOracle || Mode == ProviderMode.EffiProz ||
                                    Mode == ProviderMode.DB2 || Mode == ProviderMode.Pgsql))
            {
                var onlys = parameterizer.ParameterizeBlock(block);
                string[] strArray = formatter.FormatBlock(block, false);
                var infoArray = new QueryInfo[strArray.Length];
                int index = 0;
                int length = strArray.Length;
                while (index < length)
                {
                    infoArray[index] = new QueryInfo(block.Statements[index], strArray[index], onlys[index],
                                                     index < (length - 1) ? ResultShape.Return : resultShape,
                                                     index < (length - 1) ? typeof(int) : resultType);
                    index++;
                }
                return infoArray;
            }
            var parameters = parameterizer.Parameterize(node);
            string commandText = formatter.Format(node);
            return new[] { new QueryInfo(node, commandText, parameters, resultShape, resultType) };
        }

        internal virtual ISqlParameterizer CreateSqlParameterizer(ITypeSystemProvider typeProvider, SqlNodeAnnotations annotations)
        {
            return new SqlParameterizer(typeProvider, annotations);
        }

        internal abstract DbFormatter CreateSqlFormatter();

        private SqlIdentifier sqlIdentifier;
        internal virtual SqlIdentifier SqlIdentifier
        {
            get
            {
                if (sqlIdentifier == null)
                    sqlIdentifier = new MsSqlIdentifier();
                return sqlIdentifier;
            }
        }

        private PostBindDotNetConverter postBindDotNetConverter;
        internal virtual PostBindDotNetConverter PostBindDotNetConverter
        {
            get
            {
                if (postBindDotNetConverter == null)
                    postBindDotNetConverter = new SqlPostBindDotNetConverter(this.sqlFactory);

                return postBindDotNetConverter;
            }
        }

        internal ProviderMode Mode { get; set; }

        private bool CanBeColumn(SqlExpression expression)
        {
            if (!expression.SqlType.CanBeColumn)
            {
                return false;
            }
            switch (expression.NodeType)
            {
                case SqlNodeType.MethodCall:
                case SqlNodeType.Member:
                case SqlNodeType.New:
                    return PostBindDotNetConverter.CanConvert(expression);
            }
            return true;
        }

        [DebuggerStepThrough]
        internal ResultShape GetResultShape(Expression query)
        {
            var expression = query as LambdaExpression;
            if (expression != null)
            {
                query = expression.Body;
            }
            if (query.Type == typeof(void))
            {
                return ResultShape.Return;
            }
            if (query.Type == typeof(IMultipleResults))
            {
                return ResultShape.MultipleResults;
            }
            bool flag = typeof(IEnumerable).IsAssignableFrom(query.Type);
            IProviderType type = typeProvider.From(query.Type);
            bool flag2 = !type.IsRuntimeOnlyType && !type.IsApplicationType;
            bool flag3 = flag2 || !flag;
            var expression2 = query as MethodCallExpression;
            if (expression2 != null)
            {
                if ((expression2.Method.DeclaringType == typeof(Queryable)) ||
                    (expression2.Method.DeclaringType == typeof(Enumerable)))
                {
                    string str = expression2.Method.Name;
                    if (str == "First" || str == "FirstOrDefault" ||
                         str == "Single" || str == "SingleOrDefault")
                    {
                        flag3 = true;
                    }
                }
                else if (expression2.Method.DeclaringType == typeof(DataContext))
                {
                    if (expression2.Method.Name == "ExecuteCommand")
                    {
                        return ResultShape.Return;
                    }
                }
                else if (expression2.Method.DeclaringType.IsSubclassOf(typeof(DataContext)))
                {
                    MetaFunction function = GetFunction(query);
                    if (function != null)
                    {
                        if (!function.IsComposable)
                        {
                            flag3 = false;
                        }
                    }
                }
                else if (expression2.Method.DeclaringType == typeof(DataManipulation)
                         && expression2.Method.ReturnType == typeof(int))
                {
                    if (expression2.Method.Name == "Insert" && expression2.Arguments.Count == 3)
                        return ResultShape.Singleton;

                    return ResultShape.Return;
                }
            }
            if (flag3)
            {
                return ResultShape.Singleton;
            }
            if (flag2)
            {
                return ResultShape.Return;
            }
            return ResultShape.Sequence;
        }

        [DebuggerStepThrough]
        internal Type GetResultType(Expression query)
        {
            var expression = query as LambdaExpression;
            if (expression != null)
            {
                query = expression.Body;
            }
            var mc = query as MethodCallExpression;
            if (mc != null && mc.Method.Name == "Insert" && mc.Arguments.Count == 3)
            {
                return typeof(object);
            }
            return query.Type;
        }

        private static SqlSelect GetFinalSelect(SqlNode node)
        {
            SqlNodeType nodeType = node.NodeType;
            if (nodeType != SqlNodeType.Block)
            {
                if (nodeType == SqlNodeType.Select)
                {
                    return (SqlSelect)node;
                }
                return null;
            }
            var block = (SqlBlock)node;
            return GetFinalSelect(block.Statements[block.Statements.Count - 1]);
        }

        internal IObjectReaderFactory GetReaderFactory(SqlNode node, Type elemType)
        {
            var finalSelect = node as SqlSelect;
            SqlExpression selection = null;
            if ((finalSelect == null) && (node.NodeType == SqlNodeType.Block))
            {
                finalSelect = GetFinalSelect(node);
            }
            if (finalSelect != null)
            {
                selection = finalSelect.Selection;
            }
            else
            {
                var query = node as SqlUserQuery;
                if ((query != null) && (query.Projection != null))
                {
                    selection = query.Projection;
                }
            }
            if (selection != null)
            {
                return readerCompiler.Compile(selection, elemType);
            }
            return GetDefaultFactory(services.Model.GetMetaType(elemType));
        }

        internal virtual ICompiledSubQuery[] CompileSubQueries(SqlNode query)
        {
            var compiler = new SubQueryCompiler(this);
            ICompiledSubQuery[] result = compiler.Compile(query);
            return result;
        }

        internal virtual ICompiledSubQuery CompileSubQuery(SqlNode query, Type elementType,
                                                            ReadOnlyCollection<SqlParameter> parameters)
        {
            query = SqlDuplicator.Copy(query);
            var annotations = new SqlNodeAnnotations();
            QueryInfo[] queries = BuildQuery(ResultShape.Sequence, TypeSystem.GetSequenceType(elementType), query,
                                                  parameters, annotations);
            QueryInfo queryInfo = queries[0];
            ICompiledSubQuery[] subQueries = CompileSubQueries(queryInfo.Query);
            IObjectReaderFactory readerFactory = GetReaderFactory(queryInfo.Query, elementType);
            CheckSqlCompatibility(queries, annotations);
            return new CompiledSubQuery(queryInfo, readerFactory, parameters, subQueries);
        }

        protected IExecuteResult GetCachedResult(Expression query)
        {
            object cachedObject = services.GetCachedObject(query);
            if (cachedObject != null)
            {
                switch (GetResultShape(query))
                {
                    case ResultShape.Singleton:
                        return CreateExecuteResult(null, null, null, cachedObject);

                    case ResultShape.Sequence:
                        var type = typeof(SequenceOfOne<>).MakeGenericType(new[] { TypeSystem.GetElementType(GetResultType(query)) });
                        var value = Activator.CreateInstance(type, BindingFlags.NonPublic | BindingFlags.Instance, null,
                                                             new[] { cachedObject }, null);
                        return CreateExecuteResult(null, null, null, value);
                }
            }
            return null;
        }

        internal void CheckSqlCompatibility(QueryInfo[] queries, SqlNodeAnnotations annotations)
        {
            if ((Mode == ProviderMode.Sql2000) || (Mode == ProviderMode.SqlCE))
            {
                int index = 0;
                int length = queries.Length;
                while (index < length)
                {
                    SqlServerCompatibilityCheck.ThrowIfUnsupported(queries[index].Query, annotations, Mode);
                    index++;
                }
            }
        }

        internal bool EnableCacheLookup
        {
            get
            {
                CheckDispose();
                return enableCacheLookup;
            }
            set
            {
                CheckDispose();
                enableCacheLookup = value;
            }
        }


        protected void CheckDispose()
        {
            if (disposed)
            {
                throw Error.ProviderCannotBeUsedAfterDispose();
            }
        }

        protected void CheckInitialized()
        {
            if (services == null)
            {
                throw Error.ContextNotInitialized();
            }
        }

        protected void CheckNotDeleted()
        {
            if (deleted)
            {
                throw Error.DatabaseDeleteThroughContext();
            }
        }

        protected void InitializeProviderMode()
        {
            //InvokeMethod("InitializeProviderMode", null);

            //typeProvider = new TypeSystemProvider(_typeProvider);
            if (typeProvider == null)
            {
                #region MyRegion
                //switch (mode)
                //{
                //    case ProviderMode.Sql2000:
                //        typeProvider = SqlTypeSystem.Create2000Provider();
                //        break;
                //    default:
                //    case ProviderMode.Sql2005:
                //        typeProvider = SqlTypeSystem.Create2005Provider();
                //        break;

                //    case ProviderMode.SqlCE:
                //        typeProvider = SqlTypeSystem.CreateCEProvider();
                //        break;
                //    case ProviderMode.Oracle:
                //        typeProvider = SqlTypeSystem.CreateOreacleProvider();
                //        break;
                //    case ProviderMode.Access:
                //        typeProvider = SqlTypeSystem.CreateAccessProvider();
                //        break;
                //    case ProviderMode.MySql:
                //        typeProvider = SqlTypeSystem.CreateMySqlProvider();
                //        break;
                //} 
                #endregion

                typeProvider = CreateTypeSystemProvider();
            }

            //sqlFactory = new SqlFactory(_sqlFactory);
            //translator = new Translator(services, sqlFactory, typeProvider);
            if (sqlFactory == null)
            {
                sqlFactory = CreateSqlFactory(typeProvider, services.Model);
                translator = new Translator(services, sqlFactory, typeProvider, this);
            }
        }

        internal virtual ITypeSystemProvider CreateTypeSystemProvider()
        {
            ITypeSystemProvider result;
            switch (Mode)
            {
                default:
                case ProviderMode.Sql2000:
                    result = new SqlTypeSystem.Sql2000Provider();
                    break;
                case ProviderMode.Sql2005:
                    result = new SqlTypeSystem.Sql2005Provider();
                    break;

                case ProviderMode.SqlCE:
                    result = new SqlTypeSystem.SqlCEProvider();
                    break;
            }
            return result;
        }

        internal virtual SqlFactory CreateSqlFactory(ITypeSystemProvider typeProvider, MetaModel metaModel)
        {
            return new SqlFactory(typeProvider, metaModel);
        }

        internal virtual ExecuteResult CreateExecuteResult(DbCommand command, ReadOnlyCollection<SqlParameterInfo> parameters,
               IObjectReaderSession session)
        {
            return new ExecuteResult(command, parameters, session);
        }

        internal virtual IExecuteResult CreateExecuteResult(DbCommand cmd, ReadOnlyCollection<SqlParameterInfo> parameters, IObjectReaderSession session, object value)
        {
            return new ExecuteResult(cmd, parameters, session, value);
            //return CreateExecuteResult(cmd, parameters, session, value, false);
        }

        internal virtual IExecuteResult CreateExecuteResult(DbCommand cmd, ReadOnlyCollection<SqlParameterInfo> parameters, IObjectReaderSession session, object value, bool useReturnValue)
        {
            return new ExecuteResult(cmd, parameters, session, value, useReturnValue);
        }


        ///<summary>
        /// Executes the query represented by a specified expression tree.
        ///</summary>
        ///<param name="query">An expression tree that represents a LINQ query.</param>
        ///<returns>The value that results from executing the specified query.</returns>
        public virtual IExecuteResult Execute(Expression query)
        {
            CheckDispose();
            CheckInitialized();
            CheckNotDeleted();
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }
            InitializeProviderMode();
            if (EnableCacheLookup)
            {
                IExecuteResult cachedResult = GetCachedResult(query);
                if (cachedResult != null)
                {
                    return cachedResult;
                }
            }
            var annotations = new SqlNodeAnnotations();
            var queries = BuildQuery(query, annotations);
            CheckSqlCompatibility(queries, annotations);
            var expression = query as LambdaExpression;

            if (expression != null)
            {
                query = expression.Body;
            }
            IObjectReaderFactory readerFactory = null;
            ICompiledSubQuery[] subQueries = null;
            var queryInfo = queries[queries.Length - 1];
            if (queryInfo.ResultShape == ResultShape.Singleton)
            {
                subQueries = CompileSubQueries(queryInfo.Query);
                readerFactory = GetReaderFactory(queryInfo.Query, queryInfo.ResultType);
            }
            else if (queryInfo.ResultShape == ResultShape.Sequence)
            {
                subQueries = CompileSubQueries(queryInfo.Query);
                readerFactory = GetReaderFactory(queryInfo.Query, TypeSystem.GetElementType(queryInfo.ResultType));
            }
            return ExecuteAll(query, queries, readerFactory, null, subQueries);
        }

        internal static string version;
        private int commandTimeout;
        private bool outputed;

        internal void LogCommand(TextWriter writer, IDbCommand cmd)
        {
            if (writer == null)
                return;

            writer.WriteLine(cmd.CommandText);
            writer.WriteLine("-- CommandType: {0}", cmd.CommandType);
            foreach (DbParameter parameter in cmd.Parameters)
            {
                int num = 0;
                int num2 = 0;
                PropertyInfo property = parameter.GetType().GetProperty("Precision");
                if (property != null)
                {
                    num = (int)Convert.ChangeType(property.GetValue(parameter, null), typeof(int),
                                                  CultureInfo.InvariantCulture);
                }
                PropertyInfo info2 = parameter.GetType().GetProperty("Scale");
                if (info2 != null)
                {
                    num2 = (int)Convert.ChangeType(info2.GetValue(parameter, null), typeof(int),
                                                   CultureInfo.InvariantCulture);
                }
                //SqlParameter parameter2 = parameter as SqlParameter;
                writer.WriteLine("-- {0}: {1} {2} (Size = {3}; Prec = {4}; Scale = {5}) [{6}]",
                                 new[]
                                     {
                                         parameter.ParameterName, parameter.Direction, GetParameterDbType(parameter),
                                         parameter.Size.ToString(CultureInfo.CurrentCulture), num, num2,
                                         ConvertParameterValue(parameter.Value)
                                     });
            }
            if (version == null)
            {
                var assembly = GetType().Assembly;
                Debug.Assert(assembly != null);
                Debug.Assert(assembly.Location != null);
                version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
            }

            Type modelType;
            if (services.Model is DynamicModel)
                modelType = ((DynamicModel)services.Model).Source.GetType();
            else
                modelType = services.Model.GetType();

            writer.WriteLine("-- Context: {0} Model: {1} {2} Build: {3}",
                             new object[] { GetType().Name, Mode, modelType.Name, version });
            writer.WriteLine();
        }

        internal virtual Enum GetParameterDbType(DbParameter parameter)
        {
            return parameter.DbType;
        }

        internal static object ConvertParameterValue(object value)
        {
            if (value != null)
            {
                if (value.GetType() == typeof(Binary) || value.GetType() == typeof(byte[]))
                {
                    return string.Format("<{0}>", value.GetType());
                }
            }
            return value;
        }

        #region Provider Members

        void IProvider.ClearConnection()
        {
            CheckDispose();
            CheckInitialized();
            conManager.ClearConnection();
        }

        ICompiledQuery IProvider.Compile(Expression query)
        {
            CheckDispose();
            CheckInitialized();
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }
            InitializeProviderMode();
            var annotations = new SqlNodeAnnotations();
            QueryInfo[] queries = BuildQuery(query, annotations);
            CheckSqlCompatibility(queries, annotations);
            var expression = query as LambdaExpression;
            if (expression != null)
            {
                query = expression.Body;
            }
            IObjectReaderFactory readerFactory = null;
            ICompiledSubQuery[] subQueries = null;
            QueryInfo info = queries[queries.Length - 1];
            if (info.ResultShape == ResultShape.Singleton)
            {
                subQueries = CompileSubQueries(info.Query);
                readerFactory = GetReaderFactory(info.Query, info.ResultType);
            }
            else if (info.ResultShape == ResultShape.Sequence)
            {
                subQueries = CompileSubQueries(info.Query);
                readerFactory = GetReaderFactory(info.Query, TypeSystem.GetElementType(info.ResultType));
            }
            return new CompiledQuery(this, query, queries, readerFactory, subQueries);
        }



        void IProvider.CreateDatabase()
        {
            var sqlBuilder = new SqlBuilder(SqlIdentifier);
            //InvokeIProviderMethod("CreateDatabase");
            object obj3;
            CheckDispose();
            CheckInitialized();
            string dbName = null;
            string str2 = null;
            var builder = new DbConnectionStringBuilder
                              {
                                  ConnectionString = conManager.Connection.ConnectionString
                              };
            if (conManager.Connection.State != ConnectionState.Closed)
            {
                object obj4;
                if ((Mode == ProviderMode.SqlCE) && File.Exists(this.dbName))
                {
                    throw Error.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(this.dbName);
                }
                if (builder.TryGetValue("Initial Catalog", out obj4))
                {
                    dbName = obj4.ToString();
                }
                if (builder.TryGetValue("Database", out obj4))
                {
                    dbName = obj4.ToString();
                }
                if (builder.TryGetValue("AttachDBFileName", out obj4))
                {
                    str2 = obj4.ToString();
                }
                goto Label_01D2;
            }
            if (Mode == ProviderMode.SqlCE)
            {
                if (!File.Exists(this.dbName))
                {
                    Type type =
                        conManager.Connection.GetType().Module.GetType("System.Data.SqlServerCe.SqlCeEngine");
                    object target = Activator.CreateInstance(type, new object[] { builder.ToString() });
                    try
                    {
                        type.InvokeMember("CreateDatabase",
                                          BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null,
                                          target, new object[0], CultureInfo.InvariantCulture);
                        goto Label_0153;
                    }
                    catch (TargetInvocationException exception)
                    {
                        throw exception.InnerException;
                    }
                    finally
                    {
                        var disposable = target as IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                }
                throw Error.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(this.dbName);
            }
            if (builder.TryGetValue("Initial Catalog", out obj3))
            {
                dbName = obj3.ToString();
                builder.Remove("Initial Catalog");
            }
            if (builder.TryGetValue("Database", out obj3))
            {
                dbName = obj3.ToString();
                builder.Remove("Database");
            }
            if (builder.TryGetValue("AttachDBFileName", out obj3))
            {
                str2 = obj3.ToString();
                builder.Remove("AttachDBFileName");
            }
        Label_0153:
            conManager.Connection.ConnectionString = builder.ToString();
        Label_01D2:
            if (string.IsNullOrEmpty(dbName))
            {
                if (string.IsNullOrEmpty(str2))
                {
                    if (string.IsNullOrEmpty(this.dbName))
                    {
                        throw Error.CouldNotDetermineCatalogName();
                    }
                    dbName = this.dbName;
                }
                else
                {
                    dbName = Path.GetFullPath(str2);
                }
            }
            conManager.UseConnection(this);
            conManager.AutoClose = false;
            try
            {
                if (services.Model.GetTables().FirstOrDefault() == null)
                {
                    throw Error.CreateDatabaseFailedBecauseOfContextWithNoTables(services.Model.DatabaseName);
                }
                deleted = false;
                if (Mode == ProviderMode.SqlCE)
                {
                    foreach (MetaTable table in services.Model.GetTables())
                    {
                        string createTableCommand = sqlBuilder.GetCreateTableCommand(table);
                        if (!string.IsNullOrEmpty(createTableCommand))
                        {
                            ExecuteCommand(createTableCommand);
                        }
                    }
                    foreach (MetaTable table2 in services.Model.GetTables())
                    {
                        foreach (string str4 in sqlBuilder.GetCreateForeignKeyCommands(table2))
                        {
                            if (!string.IsNullOrEmpty(str4))
                            {
                                ExecuteCommand(str4);
                            }
                        }
                    }
                }
                else
                {
                    string command = sqlBuilder.GetCreateDatabaseCommand(dbName, str2,
                                                                         Path.ChangeExtension(str2, ".ldf"));
                    ExecuteCommand(command);
                    conManager.Connection.ChangeDatabase(dbName);
                    if (Mode == ProviderMode.Sql2005)
                    {
                        var set = new HashSet<string>();
                        foreach (MetaTable table3 in services.Model.GetTables())
                        {
                            string createSchemaForTableCommand = sqlBuilder.GetCreateSchemaForTableCommand(table3);
                            if (!string.IsNullOrEmpty(createSchemaForTableCommand))
                            {
                                set.Add(createSchemaForTableCommand);
                            }
                        }
                        foreach (string str7 in set)
                        {
                            ExecuteCommand(str7);
                        }
                    }
                    var builder2 = new StringBuilder();
                    foreach (MetaTable table4 in services.Model.GetTables())
                    {
                        string str8 = sqlBuilder.GetCreateTableCommand(table4);
                        if (!string.IsNullOrEmpty(str8))
                        {
                            builder2.AppendLine(str8);
                        }
                    }
                    foreach (MetaTable table5 in services.Model.GetTables())
                    {
                        foreach (string str9 in sqlBuilder.GetCreateForeignKeyCommands(table5))
                        {
                            if (!string.IsNullOrEmpty(str9))
                            {
                                builder2.AppendLine(str9);
                            }
                        }
                    }
                    if (builder2.Length > 0)
                    {
                        builder2.Insert(0, "SET ARITHABORT ON" + Environment.NewLine);
                        ExecuteCommand(builder2.ToString());
                    }
                }
            }
            finally
            {
                conManager.ReleaseConnection(this);
                if (conManager.Connection is SqlConnection)
                {
                    SqlConnection.ClearAllPools();
                }
            }
        }

        internal protected void ExecuteCommand(string command)
        {
            if (Log != null)
            {
                Log.WriteLine(command);
                Log.WriteLine();
            }
            IDbCommand command2 = conManager.Connection.CreateCommand();
            command2.CommandTimeout = CommandTimeout;
            command2.Transaction = conManager.Transaction;
            command2.CommandText = command;
            command2.ExecuteNonQuery();
        }

        bool IProvider.DatabaseExists()
        {
            CheckDispose();
            CheckInitialized();
            if (deleted)
            {
                return false;
            }
            bool flag;
            if (Mode == ProviderMode.SqlCE)
            {
                return File.Exists(dbName);
            }
            string connectionString = conManager.Connection.ConnectionString;
            try
            {
                conManager.UseConnection(this);
                conManager.Connection.ChangeDatabase(dbName);
                conManager.ReleaseConnection(this);
                flag = true;
            }
            //catch (Exception e)
            //{
            //}
            finally
            {
                if ((conManager.Connection.State == ConnectionState.Closed) &&
                    (string.Compare(conManager.Connection.ConnectionString, connectionString,
                                    StringComparison.Ordinal) != 0))
                {
                    conManager.Connection.ConnectionString = connectionString;
                }
            }
            return flag;
        }

        void IProvider.DeleteDatabase()
        {
            var sqlBuilder = new SqlBuilder(SqlIdentifier);
            CheckDispose();
            CheckInitialized();
            if (!deleted)
            {
                if (Mode == ProviderMode.SqlCE)
                {
                    ((IProvider)this).ClearConnection();
                    File.Delete(dbName);
                    deleted = true;
                }
                else
                {
                    string connectionString = conManager.Connection.ConnectionString;
                    IDbConnection connection = conManager.UseConnection(this);
                    try
                    {
                        connection.ChangeDatabase("MASTER");
                        if (connection is SqlConnection)
                        {
                            SqlConnection.ClearAllPools();
                        }
                        if (Log != null)
                        {
                            Log.WriteLine(Strings.LogAttemptingToDeleteDatabase(dbName));
                        }
                        ExecuteCommand(sqlBuilder.GetDropDatabaseCommand(dbName));
                        deleted = true;
                    }
                    finally
                    {
                        conManager.ReleaseConnection(this);
                        if ((conManager.Connection.State == ConnectionState.Closed) &&
                            (string.Compare(conManager.Connection.ConnectionString, connectionString,
                                            StringComparison.Ordinal) != 0))
                        {
                            conManager.Connection.ConnectionString = connectionString;
                        }
                    }
                }
            }
        }

        DbCommand IProvider.GetCommand(Expression query)
        {
            CheckDispose();
            CheckInitialized();
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }
            InitializeProviderMode();
            var annotations = new SqlNodeAnnotations();
            QueryInfo[] infoArray = BuildQuery(query, annotations);
            QueryInfo info = infoArray[infoArray.Length - 1];
            DbCommand cmd = conManager.Connection.CreateCommand();
            cmd.CommandText = info.CommandText;
            cmd.Transaction = conManager.Transaction;
            cmd.CommandTimeout = CommandTimeout;
            AssignParameters(cmd, info.Parameters, null, null);
            return cmd;
        }

        string IProvider.GetQueryText(Expression query)
        {
            //return InvokeIProviderMethod("GetQueryText", new object[] { query }) as string;
            CheckDispose();
            CheckInitialized();
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }
            InitializeProviderMode();
            var annotations = new SqlNodeAnnotations();
            QueryInfo[] infoArray = BuildQuery(query, annotations);
            var builder = new StringBuilder();
            int index = 0;
            int length = infoArray.Length;
            while (index < length)
            {
                QueryInfo info = infoArray[index];
                builder.Append(info.CommandText);
                builder.AppendLine();
                index++;
            }
            return builder.ToString();

        }

        void IProvider.Initialize(IDataServices dataServices, object connection)
        {
            throw new System.NotImplementedException();
        }


        internal void Initialize(IDataServices dataServices, IDbConnection connection)
        {
            services = dataServices;
            conManager = new SqlConnectionManager(this, (DbConnection)connection, 100);
            var type = typeof(DbDataReader);
            readerCompiler = new ObjectReaderCompiler(type, services);
            InitializeProviderMode();
        }

        IMultipleResults IProvider.Translate(DbDataReader reader)
        {
            CheckDispose();
            CheckInitialized();
            InitializeProviderMode();
            if (reader == null)
            {
                throw Error.ArgumentNull("reader");
            }
            return new MultipleResults(this, null, readerCompiler.CreateSession(CreateRereader(reader), this, null, null, null), null);
        }

        IEnumerable IProvider.Translate(Type elementType, DbDataReader reader)
        {
            CheckDispose();
            CheckInitialized();
            InitializeProviderMode();
            if (elementType == null)
            {
                throw Error.ArgumentNull("elementType");
            }
            if (reader == null)
            {
                throw Error.ArgumentNull("reader");
            }
            MetaType metaType = services.Model.GetMetaType(elementType);
            IEnumerator enumerator = GetDefaultFactory(metaType).Create(CreateRereader(reader), true, this, null, null, null);
            return (IEnumerable)Activator.CreateInstance(typeof(OneTimeEnumerable<>).MakeGenericType(new[] { elementType }), BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { enumerator }, null);
        }

        internal int CommandTimeout
        {
            get { return ((IProvider)this).CommandTimeout; }
            set { ((IProvider)this).CommandTimeout = value; }
        }

        int IProvider.CommandTimeout
        {
            get
            {
                CheckDispose();
                return commandTimeout;
            }
            set
            {
                CheckDispose();
                commandTimeout = value;
            }
        }

        internal bool CheckQueries
        {
            get
            {
                CheckDispose();
                return checkQueries;
            }
            set
            {
                CheckDispose();
                checkQueries = value;
            }
        }

        internal DbConnection Connection
        {
            get { return ((IProvider) this).Connection; }
        }

        DbConnection IProvider.Connection
        {
            get
            {
                CheckDispose();
                CheckInitialized();
                return conManager.Connection;
            }
        }

//#if FREE
//        void OutputProductInfo()
//        {
//            if (Log != null)
//            {
//                var msg = Constants.FreeEditionLimited;
//                log.WriteLine(msg);
//                log.WriteLine("For more information about ALinq, please visit http://www.alinq.org.");

//                Log.WriteLine();
//            }
//        }

//#else
//        void OutputProductInfo(TextWriter writer)
//        {
//            if (writer != null)
//            {
//                //var license = ((LicFileLicense)License);
//                Debug.Assert(License != null);
//                if (License.LicenseType == LicenseType.Trial)
//                {
//                    var msg = "This software is unregistered and will be expired after " + license.ExpiredDays +
//                              " days.";
//                    writer.WriteLine(msg);
//                }
//                else if (License.LicenseType == LicenseType.Free)
//                {
//                    var msg = Constants.FreeEditionLimited;
//                    writer.WriteLine(msg);
//                    writer.WriteLine("For more information about ALinq, please visit http://www.alinq.org.");

//                    writer.WriteLine();
//                }


//                writer.WriteLine();
//            }
//        }
//#endif

        internal TextWriter Log
        {
            get { return ((IProvider) this).Log; }
            set { ((IProvider) this).Log = value; }
        }

        TextWriter IProvider.Log
        {
            get { return log; }
            set
            {
                log = value;

                if (log != null && !outputed)
                {
                    //OutputProductInfo(log);
                    outputed = true;
                }
            }
        }

        //private LicFileLicense license;

        //internal LicFileLicense License
        //{
        //    get
        //    {
        //        if (license == null)
        //            license = ((ALinqLicenseContext)LicenseManager.CurrentContext).GetSavedLicense(this.GetType());

        //        return license;
        //    }
        //    //set { license = value; }
        //}

        internal DbTransaction Transaction
        {
            get { return ((IProvider) this).Transaction; }
        }

        DbTransaction IProvider.Transaction
        {
            get
            {
                CheckDispose();
                CheckInitialized();
                return conManager.Transaction;
            }
            set
            {
                CheckDispose();
                CheckInitialized();
                conManager.Transaction = value;
            }
        }

        #endregion


        #region IDisposable Members

        void IDisposable.Dispose()
        {
            disposed = true;
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                services = null;
                if (conManager != null)
                {
                    conManager.DisposeConnection();
                }
                conManager = null;
                typeProvider = null;
                sqlFactory = null;
                translator = null;
                readerCompiler = null;
                Log = null;
            }
        }

        #endregion

        void IConnectionUser.CompleteUse()
        {
        }

        internal IObjectReaderFactory GetDefaultFactory(MetaType rowType)
        {
            if (rowType == null)
            {
                throw Error.ArgumentNull("rowType");
            }
            var annotations = new SqlNodeAnnotations();
            Expression source = Expression.Constant(null);
            var query = new SqlUserQuery(string.Empty, null, null, source);
            if (TypeSystem.IsSimpleType(rowType.Type))
            {
                var item = new SqlUserColumn(rowType.Type, typeProvider.From(rowType.Type), query, "",
                                                       false, query.SourceExpression);
                query.Columns.Add(item);
                query.Projection = item;
            }
            else
            {
                var row = new SqlUserRow(rowType.InheritanceRoot, typeProvider.GetApplicationType(0), query, source);
                query.Projection = translator.BuildProjection(row, rowType, true, null, source);
            }
            Type sequenceType = TypeSystem.GetSequenceType(rowType.Type);
            QueryInfo[] infoArray = BuildQuery(ResultShape.Sequence, sequenceType, query, null, annotations);
            return GetReaderFactory(infoArray[infoArray.Length - 1].Query, rowType.Type);
        }

        #region IReaderProvider Members

        IConnectionManager IReaderProvider.ConnectionManager
        {
            get { return conManager; }
        }

        IDataServices IReaderProvider.Services
        {
            get { return services; }
        }

        internal ITypeSystemProvider TypeProvider
        {
            get { return typeProvider; }
        }

        #endregion


        #region IProviderExtend Members

        void IProviderExtend.CreateTable(MetaTable metaTable)
        {
            var sqlBuilder = new SqlBuilder(this);
            var command = sqlBuilder.GetCreateTableCommand(metaTable);
            services.Context.ExecuteCommand(command);
        }

        bool IProviderExtend.TableExists(MetaTable metaTable)
        {
            var sql = @"SELECT COUNT(*) FROM sysobjects WHERE id = object_id({0})";
            var count = services.Context.ExecuteQuery<int>(sql, metaTable.TableName).SingleOrDefault();
            return count > 0;
        }

        void IProviderExtend.DeleteTable(MetaTable metaTable)
        {
            var sql = "DROP TABLE {0}";
            sql = string.Format(sql, metaTable.TableName);
            services.Context.ExecuteCommand(sql);
        }

        void IProviderExtend.CreateForeignKeys(MetaTable metaTable)
        {
            var sqlBuilder = new SqlBuilder(this);
            var commands = sqlBuilder.GetCreateForeignKeyCommands(metaTable);
            foreach (var command in commands)
                services.Context.ExecuteCommand(command);
        }

        #endregion
    }

}