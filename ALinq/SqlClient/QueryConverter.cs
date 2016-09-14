using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using ALinq;
using ALinq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ALinq.Provider;


namespace ALinq.SqlClient
{
    internal abstract class QueryConverter
    {

        public class GroupInfo
        {
            // Fields
            public SqlExpression ElementOnGroupSource;
            public SqlSelect SelectWithGroup;
        }

        public enum ConversionMethod
        {
            Treat,
            Ignore,
            Convert,
            Lift
        }

        //public static Type SourceType = GetType(TYPE_NAME);

        protected Expression dominatingExpression;
        protected bool outerNode;
        protected SqlFactory sql;
        private readonly IDictionary<ParameterExpression, SqlNode> dupMap;
        private readonly IDictionary<SqlNode, GroupInfo> gmap;
        protected Translator translator;
        private readonly IDataServices services;
        protected ITypeSystemProvider typeProvider;
        protected Dictionary<ParameterExpression, SqlExpression> map;
        private readonly Dictionary<ParameterExpression, Expression> exprMap;
        protected bool allowDeferred;
        private readonly SqlProvider.ProviderMode mode;
        private readonly SqlIdentifier sqlIdentifier;

        protected QueryConverter(IDataServices services, ITypeSystemProvider typeProvider, Translator translator,
                                 SqlFactory sql)
        {
            if (services == null)
            {
                throw Error.ArgumentNull("services");
            }
            if (sql == null)
            {
                throw Error.ArgumentNull("sql");
            }
            if (translator == null)
            {
                throw Error.ArgumentNull("translator");
            }
            if (typeProvider == null)
            {
                throw Error.ArgumentNull("typeProvider");
            }
            this.services = services;
            this.translator = translator;
            this.sql = sql;
            this.typeProvider = typeProvider;
            map = new Dictionary<ParameterExpression, SqlExpression>();
            exprMap = new Dictionary<ParameterExpression, Expression>();
            dupMap = new Dictionary<ParameterExpression, SqlNode>();
            gmap = new Dictionary<SqlNode, GroupInfo>();
            allowDeferred = true;
            this.mode = ((SqlProvider)services.Context.Provider).Mode;
            this.sqlIdentifier = ((SqlProvider)services.Context.Provider).SqlIdentifier;



           
        }


        public SqlNode ConvertOuter(Expression node)
        {
            //var model = services.Model as DynamicModel;
            //if (model != null)
            //{
            //    model.UpdateBy(node);
            //}

            dominatingExpression = node;
            outerNode = true;

            //model = new MergeModel(services.Model);
            //new MetaDataMemberBuilder(sql, model).Build(node);
            //model = services.Model;

            SqlNode node2 = typeof(ITable).IsAssignableFrom(node.Type) ? VisitSequence(node) : VisitInner(node);

            if (node2.NodeType == SqlNodeType.MethodCall)
            {
                throw Error.InvalidMethodExecution(((SqlMethodCall)node2).Method.Name);
            }

            var selection = node2 as SqlExpression;
            if (selection != null)
            {
                node2 = new SqlSelect(selection, null, dominatingExpression);
            }
            return new SqlIncludeScope(node2, dominatingExpression);
        }

        protected virtual SqlNode VisitInner(Expression node)
        {
            if (node == null)
            {
                return null;
            }
            Expression expression = dominatingExpression;
            dominatingExpression = ChooseBestDominatingExpression(dominatingExpression, node);
            try
            {
                switch (node.NodeType)
                {
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Coalesce:
                    case ExpressionType.Divide:
                    case ExpressionType.Equal:
                    case ExpressionType.ExclusiveOr:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.Modulo:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.NotEqual:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                    case ExpressionType.Power:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                        return VisitBinary((BinaryExpression)node);

                    case ExpressionType.ArrayLength:
                        return VisitArrayLength((UnaryExpression)node);

                    case ExpressionType.ArrayIndex:
                        return VisitArrayIndex((BinaryExpression)node);

                    case ExpressionType.Call:
                        return VisitMethodCall((MethodCallExpression)node);

                    case ExpressionType.Conditional:
                        return VisitConditional((ConditionalExpression)node);

                    case ExpressionType.Constant:
                        return VisitConstant((ConstantExpression)node);

                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                        return VisitCast((UnaryExpression)node);

                    case ExpressionType.Invoke:
                        return VisitInvocation((InvocationExpression)node);

                    case ExpressionType.Lambda:
                        return VisitLambda((LambdaExpression)node);

                    case ExpressionType.LeftShift:
                    case ExpressionType.RightShift:
                        throw Error.UnsupportedNodeType(node.NodeType);

                    case ExpressionType.MemberAccess:
                        return VisitMemberAccess((MemberExpression)node);

                    case ExpressionType.MemberInit:
                        return VisitMemberInit((MemberInitExpression)node);

                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                    case ExpressionType.Not:
                        return VisitUnary((UnaryExpression)node);

                    case ExpressionType.New:
                        return VisitNew((NewExpression)node);

                    case ExpressionType.NewArrayInit:
                        return VisitNewArrayInit((NewArrayExpression)node);

                    case ExpressionType.Parameter:
                        return VisitParameter((ParameterExpression)node);

                    case ExpressionType.Quote:
                        return Visit(((UnaryExpression)node).Operand);

                    case ExpressionType.TypeAs:
                        return VisitAs((UnaryExpression)node);

                    case ExpressionType.TypeIs:
                        return VisitTypeBinary((TypeBinaryExpression)node);

                    case ((ExpressionType)0x7d0):
                        return ((KnownExpression)node).Node;

                    case ((ExpressionType)0x7d1):
                        return VisitLinkedTable((LinkedTableExpression)node);
                }
                throw Error.UnrecognizedExpressionNode(node.NodeType);
            }
            catch (Exception exc)
            {
                throw exc;
            }
            finally
            {
                dominatingExpression = expression;
            }
        }

        private SqlNode VisitLinkedTable(LinkedTableExpression linkedTable)
        {
            return TranslateConstantTable(linkedTable.Table, linkedTable.Link);
        }

        private SqlNode VisitTypeBinary(TypeBinaryExpression b)
        {
            SqlExpression expr = VisitExpression(b.Expression);
            if (b.NodeType != ExpressionType.TypeIs)
            {
                throw Error.TypeBinaryOperatorNotRecognized();
            }
            var typeOperand = b.TypeOperand;
            var unary = new SqlUnary(SqlNodeType.Treat, typeOperand, typeProvider.From(typeOperand),
                                     expr, dominatingExpression);
            return sql.Unary(SqlNodeType.IsNotNull, unary, dominatingExpression);
        }


        private SqlNode VisitAs(UnaryExpression a)
        {
            SqlNode node = Visit(a.Operand);
            var expr = node as SqlExpression;
            if (expr != null)
            {
                return new SqlUnary(SqlNodeType.Treat, a.Type, typeProvider.From(a.Type), expr, a);
            }
            var select = node as SqlSelect;
            if (select == null)
            {
                throw Error.DidNotExpectAs(a);
            }
            return new SqlUnary(SqlNodeType.Treat, a.Type, typeProvider.From(a.Type),
                                sql.SubSelect(SqlNodeType.Multiset, select), a);
        }

        private SqlNode VisitParameter(ParameterExpression p)
        {
            SqlExpression expression;
            Expression expression2;
            SqlNode node;
            if (map.TryGetValue(p, out expression))
            {
                return expression;
            }
            if (exprMap.TryGetValue(p, out expression2))
            {
                return Visit(expression2);
            }

            if (dupMap.TryGetValue(p, out node))
            {
                var duplicator = new SqlDuplicator(true);
                return duplicator.Duplicate(node);
            }

            //----- hack ! for debug -------
            //HACK:双主键连接。
            if (p.Name == "keys")
            {
                var key = dupMap.Keys.Where(o => o.Name == p.Name).SingleOrDefault();
                if (key == null)
                    throw Error.ParameterNotInScope(p.Name);
                node = dupMap[key];
                var duplicator = new SqlDuplicator(true);
                return duplicator.Duplicate(node);
            }
            /**/
            //-----------------------------
            throw Error.ParameterNotInScope(p.Name);
        }

        private SqlNode VisitNewArrayInit(NewArrayExpression arr)
        {
            var exprs = new SqlExpression[arr.Expressions.Count];
            int index = 0;
            int length = exprs.Length;
            while (index < length)
            {
                exprs[index] = VisitExpression(arr.Expressions[index]);
                index++;
            }
            return new SqlClientArray(arr.Type, typeProvider.From(arr.Type), exprs, dominatingExpression);
        }

        private SqlNode VisitNew(NewExpression qn)
        {
            if ((TypeSystem.IsNullableType(qn.Type) && (qn.Arguments.Count == 1)) &&
                (TypeSystem.GetNonNullableType(qn.Type) == qn.Arguments[0].Type))
            {
                return (VisitCast(Expression.Convert(qn.Arguments[0], qn.Type)) as SqlExpression);
            }
            if ((qn.Type == typeof(decimal)) && (qn.Arguments.Count == 1))
            {
                return (VisitCast(Expression.Convert(qn.Arguments[0], typeof(decimal))) as SqlExpression);
            }
            MetaType metaType = services.Model.GetMetaType(qn.Type);
            if (metaType.IsEntity)
            {
                throw Error.CannotMaterializeEntityType(qn.Type);
            }
            SqlExpression[] args = null;
            if (qn.Arguments.Count > 0)
            {
                args = new SqlExpression[qn.Arguments.Count];
                int index = 0;
                int count = qn.Arguments.Count;
                while (index < count)
                {
                    args[index] = VisitExpression(qn.Arguments[index]);
                    index++;
                }
            }
            return sql.New(metaType, qn.Constructor, args, PropertyOrFieldOf(qn.Members), null,
                                dominatingExpression);
        }

        private SqlNode VisitUnary(UnaryExpression u)
        {
            SqlExpression expression = VisitExpression(u.Operand);
            if (u.Method != null)
            {
                return sql.MethodCall(u.Type, u.Method, null, new[] { expression }, dominatingExpression);
            }
            switch (u.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    return sql.Unary(SqlNodeType.Negate, expression, dominatingExpression);

                case ExpressionType.UnaryPlus:
                    return null;

                case ExpressionType.Not:
                    if ((u.Operand.Type != typeof(bool)) && (u.Operand.Type != typeof(bool?)))
                    {
                        return sql.Unary(SqlNodeType.BitNot, expression, dominatingExpression);
                    }
                    return sql.Unary(SqlNodeType.Not, expression, dominatingExpression);

                case ExpressionType.TypeAs:
                    return sql.Unary(SqlNodeType.Treat, expression, dominatingExpression);
            }
            return null;
        }

        private SqlNode VisitMemberInit(MemberInitExpression init)
        {
            MetaType metaType = services.Model.GetMetaType(init.Type);
            if (metaType.IsEntity && !EnableMaterializeEntityType)//metaType.Type.GetInterface("IChangedProperties") == null)
            {
                throw Error.CannotMaterializeEntityType(init.Type);
            }
            SqlExpression[] args = null;
            NewExpression newExpression = init.NewExpression;
            if ((newExpression.Type == typeof(decimal)) && (newExpression.Arguments.Count == 1))
            {
                return (VisitCast(Expression.Convert(newExpression.Arguments[0], typeof(decimal))) as SqlExpression);
            }
            if (newExpression.Arguments.Count > 0)
            {
                args = new SqlExpression[newExpression.Arguments.Count];
                int index = 0;
                int length = args.Length;
                while (index < length)
                {
                    args[index] = this.VisitExpression(newExpression.Arguments[index]);
                    index++;
                }
            }
            int count = init.Bindings.Count;
            var items = new SqlMemberAssign[count];
            var keys = new int[items.Length];
            for (int i = 0; i < count; i++)
            {
                var assignment = init.Bindings[i] as MemberAssignment;
                if (assignment == null)
                {
                    throw Error.UnhandledBindingType(init.Bindings[i].BindingType);
                }
                SqlExpression expr = this.VisitExpression(assignment.Expression);
                items[i] = new SqlMemberAssign(assignment.Member, expr);
                keys[i] = metaType.GetDataMember(assignment.Member).Ordinal;
            }
            Array.Sort(keys, items, 0, items.Length);
            return this.sql.New(metaType, newExpression.Constructor, args,
                                PropertyOrFieldOf(newExpression.Members), items, this.dominatingExpression);
        }

        private bool EnableMaterializeEntityType { get; set; }

        private static IEnumerable<MemberInfo> PropertyOrFieldOf(IEnumerable<MemberInfo> members)
        {
            if (members == null)
            {
                return null;
            }
            var list = new List<MemberInfo>();
            foreach (MemberInfo info in members)
            {
                MemberTypes memberType = info.MemberType;
                if (memberType != MemberTypes.Field)
                {
                    if (memberType != MemberTypes.Method)
                    {
                        if (memberType != MemberTypes.Property)
                        {
                            throw Error.CouldNotConvertToPropertyOrField(info);
                        }
                    }
                    else
                    {
                        foreach (PropertyInfo info2 in
                                 info.DeclaringType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public |
                                                                  BindingFlags.Instance))
                        {
                            if (info2.CanRead && (info2.GetGetMethod() == info))
                            {
                                list.Add(info2);
                                break;
                            }
                        }
                        continue;
                    }
                }
                list.Add(info);
            }
            return list;
        }

        protected virtual SqlNode VisitMemberAccess(MemberExpression expression)
        {
            Type memberType = TypeSystem.GetMemberType(expression.Member);
            if (memberType.IsGenericType && (memberType.GetGenericTypeDefinition() == typeof(Table<>)))
            {
                Type type = memberType.GetGenericArguments()[0];
                this.CheckContext(this.VisitExpression(expression.Expression));
                ITable table = this.Services.Context.GetTable(type);
                if (table != null)
                {
                    return this.Visit(Expression.Constant(table));
                }
            }
            if ((expression.Member.Name == "Count") && TypeSystem.IsSequenceType(expression.Expression.Type))
            {
                return this.VisitAggregate(expression.Expression, null, SqlNodeType.Count, typeof(int));
            }
            return this.sql.Member(this.VisitExpression(expression.Expression), expression.Member);
        }

        private SqlNode VisitLambda(LambdaExpression lambda)
        {
            int num = 0;
            int count = lambda.Parameters.Count;
            while (num < count)
            {
                ParameterExpression expression2;
                ParameterExpression expression = lambda.Parameters[num];
                if (expression.Type == typeof(Type))
                {
                    throw Error.BadParameterType(expression.Type);
                }
                LambdaExpression accessor =
                    Expression.Lambda(typeof(Func<,>).MakeGenericType(new[] { typeof(object[]), expression.Type }),
                                      Expression.Convert(
                                          Expression.ArrayIndex(
                                              expression2 = Expression.Parameter(typeof(object[]), "args"),
                                              Expression.Constant(num)), expression.Type), new[] { expression2 });
                var parameter = new SqlClientParameter(expression.Type,
                                                                      typeProvider.From(expression.Type), accessor,
                                                                      dominatingExpression);
                this.dupMap[expression] = parameter;
                num++;
            }
            return this.VisitInner(lambda.Body);
        }

        private SqlNode VisitInvocation(InvocationExpression invoke)
        {
            LambdaExpression expression = (invoke.Expression.NodeType == ExpressionType.Quote)
                                              ? ((LambdaExpression)((UnaryExpression)invoke.Expression).Operand)
                                              : (invoke.Expression as LambdaExpression);
            if (expression != null)
            {
                int num = 0;
                int count = invoke.Arguments.Count;
                while (num < count)
                {
                    exprMap[expression.Parameters[num]] = invoke.Arguments[num];
                    num++;
                }
                return VisitInner(expression.Body);
            }
            SqlExpression expression2 = VisitExpression(invoke.Expression);
            if (expression2.NodeType == SqlNodeType.Value)
            {
                var value2 = (SqlValue)expression2;
                var delegate2 = value2.Value as Delegate;
                if (delegate2 != null)
                {
                    var target = delegate2.Target as CompiledQuery;
                    if (target != null)
                    {
                        return this.VisitInvocation(Expression.Invoke(target.Expression, invoke.Arguments));
                    }
                    if (invoke.Arguments.Count == 0)
                    {
                        object obj2;
                        try
                        {
                            obj2 = delegate2.DynamicInvoke(null);
                        }
                        catch (TargetInvocationException exception)
                        {
                            throw exception.InnerException;
                        }
                        return this.sql.ValueFromObject(obj2, invoke.Type, true, dominatingExpression);
                    }
                }
            }
            var exprs = new SqlExpression[invoke.Arguments.Count];
            for (int i = 0; i < exprs.Length; i++)
            {
                exprs[i] = (SqlExpression)this.Visit(invoke.Arguments[i]);
            }
            var array = new SqlClientArray(typeof(object[]), this.typeProvider.From(typeof(object[])),
                                                      exprs, this.dominatingExpression);
            return this.sql.MethodCall(invoke.Type, typeof(Delegate).GetMethod("DynamicInvoke"), expression2,
                                       new SqlExpression[] { array }, this.dominatingExpression);
        }

        protected virtual SqlNode VisitCast(UnaryExpression c)
        {
            if (c.Method != null)
            {
                SqlExpression expression = VisitExpression(c.Operand);
                return sql.MethodCall(c.Type, c.Method, null, new[] { expression }, dominatingExpression);
            }
            //MY CODE: Disable Change Type
            //return Visit(c.Operand);
            return this.VisitChangeType(c.Operand, c.Type);
        }

        protected virtual SqlNode VisitConstant(ConstantExpression expression)
        {
            Type type = expression.Type;
            if (expression.Value == null)
            {
                return sql.TypedLiteralNull(type, this.dominatingExpression);
            }
            if (type == typeof(object))
            {
                type = expression.Value.GetType();
            }
            return sql.ValueFromObject(expression.Value, type, true, this.dominatingExpression);
        }

        protected virtual SqlNode VisitConditional(ConditionalExpression cond)
        {
            var list = new List<SqlWhen>(1) { new SqlWhen(VisitExpression(cond.Test), VisitExpression(cond.IfTrue)) };
            SqlExpression @else = VisitExpression(cond.IfFalse);
            while (@else.NodeType == SqlNodeType.SearchedCase)
            {
                var @case = (SqlSearchedCase)@else;
                list.AddRange(@case.Whens);
                @else = @case.Else;
            }
            return this.sql.SearchedCase(list.ToArray(), @else, this.dominatingExpression);
        }

        protected virtual SqlNode VisitMethodCall(MethodCallExpression mc)
        {

            if (mc.Method.Name == "get_Item" && mc.Arguments.Count == 1 && services.Model is DynamicModel)
            {
                var arg = VisitExpression(mc.Arguments[0]) as SqlValue;
                if (arg != null && arg.Value is string)
                {
                    var metaType = services.Model.GetMetaType(mc.Object.Type);
                    var expo = VisitExpression(mc.Object);
                    var member = metaType.DataMembers.LastOrDefault(o => o.Name == (string)arg.Value);
                    if (member != null)
                        return sql.Member(expo, member);
                }
            }


            Type declaringType = mc.Method.DeclaringType;
            if (mc.Method.IsStatic)
            {
                if (IsSequenceOperatorCall(mc))
                {
                    return VisitSequenceOperatorCall(mc);
                }
                if (IsDataManipulationCall(mc))
                {
                    return VisitDataManipulationCall(mc);
                }
                if (((declaringType == typeof(DBConvert)) || (declaringType == typeof(Convert))) &&
                    (mc.Method.Name == "ChangeType"))
                {
                    SqlNode node = null;
                    if (mc.Arguments.Count == 2)
                    {
                        object obj2 = GetValue(mc.Arguments[1], "ChangeType");
                        if ((obj2 != null) && typeof(Type).IsAssignableFrom(obj2.GetType()))
                        {
                            node = VisitChangeType(mc.Arguments[0], (Type)obj2);
                        }
                    }
                    if (node == null)
                    {
                        throw Error.MethodFormHasNoSupportConversionToSql(mc.Method.Name, mc.Method);
                    }
                    return node;
                }
            }
            else if (typeof(DataContext).IsAssignableFrom(mc.Method.DeclaringType))
            {
                string name = mc.Method.Name;
                if (name != null)
                {
                    if (!(name == "GetTable"))
                    {
                        if ((name == "ExecuteCommand") || (name == "ExecuteQuery"))
                        {
                            return VisitUserQuery((string)GetValue(mc.Arguments[0], mc.Method.Name),
                                                  GetArray(mc.Arguments[1]), mc.Type);
                        }
                    }
                    else if (mc.Method.IsGenericMethod)
                    {
                        Type[] genericArguments = mc.Method.GetGenericArguments();
                        if ((genericArguments.Length == 1) && (mc.Method.GetParameters().Length == 0))
                        {
                            CheckContext(VisitExpression(mc.Object));
                            ITable table = Services.Context.GetTable(genericArguments[0]);
                            if (table != null)
                            {
                                return Visit(Expression.Constant(table));
                            }
                        }
                    }
                }
                if (IsMappedFunctionCall(mc))
                {
                    return VisitMappedFunctionCall(mc);
                }
            }
            else if ((((mc.Method.DeclaringType != typeof(string)) && (mc.Method.Name == "Contains")) &&
                      (!mc.Method.IsStatic && typeof(IList).IsAssignableFrom(mc.Method.DeclaringType))) &&
                     (((mc.Type == typeof(bool)) && (mc.Arguments.Count == 1)) &&
                      TypeSystem.GetElementType(mc.Method.DeclaringType).IsAssignableFrom(mc.Arguments[0].Type)))
            {
                return VisitContains(mc.Object, mc.Arguments[0]);
            }
            SqlExpression expression = VisitExpression(mc.Object);
            var args = new SqlExpression[mc.Arguments.Count];
            int index = 0;
            int length = args.Length;
            while (index < length)
            {
                args[index] = VisitExpression(mc.Arguments[index]);
                index++;
            }
            var result = sql.MethodCall(mc.Method, expression, args, dominatingExpression);
            return result;
        }

        private SqlNode VisitContains(Expression sequence, Expression value)
        {
            Type elemType = TypeSystem.GetElementType(sequence.Type);
            if (sequence.NodeType == ExpressionType.NewArrayInit)
            {
                var expression = (NewArrayExpression)sequence;
                //if (selector == null)
                //{
                Func<Expression, SqlExpression> selector = (v => this.VisitExpression(v));
                //}
                List<SqlExpression> list = expression.Expressions.Select(selector).ToList();
                SqlExpression expr = VisitExpression(value);
                return this.GenerateInExpression(expr, list);
            }
            SqlNode node = this.Visit(sequence);
            if (node.NodeType == SqlNodeType.Value)
            {
                var source = ((SqlValue)node).Value as IEnumerable;
                var queryable = source as IQueryable;
                if (queryable == null)
                {
                    SqlExpression expression3 = this.VisitExpression(value);
                    //if (func2 == null)
                    //{
                    Func<object, SqlExpression> func2 = (v => this.sql.ValueFromObject(v, elemType, true, this.dominatingExpression));
                    //}
                    Debug.Assert(source != null);
                    List<SqlExpression> list2 = source.OfType<object>().Select(func2).ToList();
                    return this.GenerateInExpression(expression3, list2);
                }
                node = this.Visit(queryable.Expression);
            }
            var left = Expression.Parameter(value.Type, "p");
            var lambda = Expression.Lambda(Expression.Equal(left, value), new[] { left });
            //return CoerceToSequence(node);
            return VisitQuantifier(CoerceToSequence(node), lambda, true);
        }

        private SqlNode VisitMappedFunctionCall(MethodCallExpression mc)
        {
            MetaFunction function = services.Model.GetFunction(mc.Method);
            CheckContext(VisitExpression(mc.Object));
            if (!function.IsComposable)
            {
                return TranslateStoredProcedureCall(mc, function);
            }
            if (function.ResultRowTypes.Count > 0)
            {
                return TranslateTableValuedFunction(mc, function);
            }
            IProviderType sqlType = ((function.ReturnParameter != null) &&
                                    !string.IsNullOrEmpty(function.ReturnParameter.DbType))
                                       ? typeProvider.Parse(function.ReturnParameter.DbType)
                                       : typeProvider.From(mc.Method.ReturnType);
            List<SqlExpression> functionParameters = GetFunctionParameters(mc, function);
            return sql.FunctionCall(mc.Method.ReturnType, sqlType, function.MappedName, functionParameters, mc);
        }

        internal SqlNode VisitMappedFunctionCall(MethodCallExpression mc, SqlQueryConverter converter)
        {
            MetaFunction function = services.Model.GetFunction(mc.Method);
            converter.CheckContext(converter.VisitExpression(mc.Object));
            if (!function.IsComposable)
            {
                return converter.TranslateStoredProcedureCall(mc, function);
            }
            if (function.ResultRowTypes.Count > 0)
            {
                return converter.TranslateTableValuedFunction(mc, function);
            }
            IProviderType sqlType = ((function.ReturnParameter != null) &&
                                    !string.IsNullOrEmpty(function.ReturnParameter.DbType))
                                       ? converter.typeProvider.Parse(function.ReturnParameter.DbType)
                                       : converter.typeProvider.From(mc.Method.ReturnType);
            List<SqlExpression> functionParameters = converter.GetFunctionParameters(mc, function);
            return converter.sql.FunctionCall(mc.Method.ReturnType, sqlType, function.MappedName, functionParameters, mc);
        }

        protected List<SqlExpression> GetFunctionParameters(MethodCallExpression mce, MetaFunction function)
        {
            var list = new List<SqlExpression>(mce.Arguments.Count);
            int num = 0;
            int count = mce.Arguments.Count;
            while (num < count)
            {
                SqlExpression item = this.VisitExpression(mce.Arguments[num]);
                MetaParameter parameter = function.Parameters[num];
                if (!string.IsNullOrEmpty(parameter.DbType))
                {
                    var expression2 = item as SqlSimpleTypeExpression;
                    if (expression2 != null)
                    {
                        IProviderType type = typeProvider.Parse(parameter.DbType);
                        expression2.SetSqlType(type);
                    }
                }
                list.Add(item);
                num++;
            }
            return list;
        }

        private SqlNode TranslateTableValuedFunction(MethodCallExpression mce, MetaFunction function)
        {
            List<SqlExpression> functionParameters = GetFunctionParameters(mce, function);
            var alias = new SqlAlias(sql.TableValuedFunctionCall(function.ResultRowTypes[0].InheritanceRoot,
                                                              mce.Method.ReturnType, function.MappedName,
                                                              functionParameters, mce));
            var item = new SqlAliasRef(alias);
            var sqlExpression = translator.BuildProjection(item, function.ResultRowTypes[0].InheritanceRoot,
                                                           allowDeferred, null, mce);
            return new SqlSelect(sqlExpression, alias, mce);
        }


        protected virtual SqlNode TranslateStoredProcedureCall(MethodCallExpression mce, MetaFunction function)
        {
            if (!outerNode)
            {
                throw Error.SprocsCannotBeComposed();
            }
            List<SqlExpression> functionParameters = GetFunctionParameters(mce, function);
            var query = new SqlStoredProcedureCall(function, null, functionParameters, mce);
            Type returnType = mce.Method.ReturnType;
            if (returnType.IsGenericType &&
                ((returnType.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                 (returnType.GetGenericTypeDefinition() == typeof(ISingleResult<>))))
            {
                MetaType inheritanceRoot = function.ResultRowTypes[0].InheritanceRoot;
                var item = new SqlUserRow(inheritanceRoot, typeProvider.GetApplicationType(0), query, mce);
                query.Projection = translator.BuildProjection(item, inheritanceRoot, allowDeferred, null, mce);
                return query;
            }
            //var provider = services.Context.Provider as SqlProvider;
            //if (provider != null && (provider.Mode == SqlProvider.ProviderMode.Oracle))
            //    return query;

            if (typeof(IMultipleResults).IsAssignableFrom(returnType) || returnType.IsValueType ||
                returnType == typeof(string) || returnType == typeof(void))
            {
                return query;
            }

            var provider = services.Context.Provider as SqlProvider;
            if (provider != null && (provider.Mode == SqlProvider.ProviderMode.Oracle ||
                                     provider.Mode == SqlProvider.ProviderMode.OdpOracle ||
                                     provider.Mode == SqlProvider.ProviderMode.Access))
                return query;

            throw Error.InvalidReturnFromSproc(returnType);

        }

        internal static bool IsValidateReturnType(Type returnType)
        {
            //var value = (!typeof(IMultipleResults).IsAssignableFrom(returnType) && (returnType != typeof(int))) && (returnType != typeof(int?));
            var allow = returnType != typeof(void) && (returnType.IsValueType || returnType == typeof(string));
            return allow;
        }


        protected virtual SqlExpression GetReturnIdentityExpression(MetaDataMember idMember, bool isOutputFromInsert)
        {
            if (isOutputFromInsert)
            {
                return new SqlVariable(idMember.Type, sql.Default(idMember), "@id", dominatingExpression);
            }
            IProviderType type = this.sql.Default(idMember);
            if (!IsLegalIdentityType(type.GetClosestRuntimeType()))
            {
                throw Error.InvalidDbGeneratedType(type.ToQueryString());
            }
            if ((this.ConverterStrategy & ConverterStrategy.CanUseScopeIdentity) != ConverterStrategy.Default)
            {
                return new SqlVariable(typeof(decimal), typeProvider.From(typeof(decimal)), "SCOPE_IDENTITY()",
                                       this.dominatingExpression);
            }
            return new SqlVariable(typeof(decimal), typeProvider.From(typeof(decimal)), "@@IDENTITY",
                                   dominatingExpression);
        }

        protected SqlExpression GetAggregate(SqlNodeType aggType, Type clrType, SqlExpression exp)
        {
            return new SqlUnary(aggType, clrType, this.typeProvider.From(clrType), exp, this.dominatingExpression);
        }

        protected GroupInfo FindGroupInfo(SqlNode source)
        {
            GroupInfo info = null;
            this.gmap.TryGetValue(source, out info);
            if (info == null)
            {
                var alias = source as SqlAlias;
                if (alias != null)
                {
                    var node = alias.Node as SqlSelect;
                    if (node != null)
                    {
                        return this.FindGroupInfo(node.Selection);
                    }
                    source = alias.Node;
                }
                var key = source as SqlExpression;
                if (key == null)
                {
                    return null;
                }
                switch (key.NodeType)
                {
                    case SqlNodeType.AliasRef:
                        return this.FindGroupInfo(((SqlAliasRef)key).Alias);

                    case SqlNodeType.Member:
                        return this.FindGroupInfo(((SqlMember)key).Expression);
                }
                this.gmap.TryGetValue(key, out info);
            }
            return info;
        }

        private SqlExpression GenerateInExpression(SqlExpression expr, List<SqlExpression> list)
        {
            if (list.Count == 0)
            {
                return this.sql.ValueFromObject(false, this.dominatingExpression);
            }
            if (list[0].SqlType.CanBeColumn)
            {
                return this.sql.In(expr, list, this.dominatingExpression);
            }

            SqlExpression left = this.sql.Binary(SqlNodeType.EQ, expr, list[0]);
            int num = 1;
            int count = list.Count;
            while (num < count)
            {
                left = this.sql.Binary(SqlNodeType.Or, left,
                                       this.sql.Binary(SqlNodeType.EQ, (SqlExpression)SqlDuplicator.Copy(expr),
                                                       list[num]));
                num++;
            }
            return left;
        }

        private SqlExpression GenerateQuantifier(SqlAlias alias, SqlExpression cond, bool isAny)
        {
            var selection = new SqlAliasRef(alias);
            if (isAny)
            {
                var select = new SqlSelect(selection, alias, this.dominatingExpression);
                select.Where = cond;
                select.OrderingType = SqlOrderingType.Never;
                return this.sql.SubSelect(SqlNodeType.Exists, select);
            }
            var select3 = new SqlSelect(selection, alias, this.dominatingExpression);
            SqlSubSelect expression = this.sql.SubSelect(SqlNodeType.Exists, select3);
            select3.Where = this.sql.Unary(SqlNodeType.Not2V, cond, this.dominatingExpression);
            return this.sql.Unary(SqlNodeType.Not, expression, this.dominatingExpression);
        }

        private static Expression[] GetArray(Expression array)
        {
            var expression = array as NewArrayExpression;
            if (expression != null)
            {
                return expression.Expressions.ToArray();
            }
            var expression2 = array as ConstantExpression;
            if (expression2 != null)
            {
                var source = expression2.Value as object[];
                if (source != null)
                {
                    Type elemType = TypeSystem.GetElementType(expression2.Type);
                    return source.Select(o => Expression.Constant(o, elemType)).ToArray();
                }
            }
            return new Expression[0];
        }


        private bool IsMappedFunctionCall(MethodCallExpression mc)
        {
            var modle = services.Model;
            return (modle.GetFunction(mc.Method) != null);
        }

        protected IDataServices Services
        {
            get { return services; }
        }

        protected SqlExpression VisitExpression(Expression exp)
        {
            SqlNode node = this.Visit(exp);
            if (node == null)
            {
                return null;
            }
            var expression = node as SqlExpression;
            if (expression != null)
            {
                return expression;
            }
            var select = node as SqlSelect;
            if (select == null)
            {
                throw Error.UnrecognizedExpressionNode(node);
            }
            return this.sql.SubSelect(SqlNodeType.Multiset, select, exp.Type);
        }

        private SqlNode ChangeType(SqlExpression expr, Type type)
        {
            if (type == typeof(object))
            {
                return expr;
            }
            if ((expr.NodeType == SqlNodeType.Value) && (((SqlValue)expr).Value == null))
            {
                return this.sql.TypedLiteralNull(type, expr.SourceExpression);
            }
            if (expr.NodeType == SqlNodeType.ClientParameter)
            {
                var parameter = (SqlClientParameter)expr;
                return new SqlClientParameter(type, this.sql.TypeProvider.From(type),
                                              Expression.Lambda(Expression.Convert(parameter.Accessor.Body, type),
                                                                parameter.Accessor.Parameters.ToArray()),
                                              parameter.SourceExpression);
            }
            ConversionMethod method = this.ChooseConversionMethod(expr.ClrType, type);
            switch (method)
            {
                case ConversionMethod.Treat:
                    return new SqlUnary(SqlNodeType.Treat, type, this.typeProvider.From(type), expr,
                                        expr.SourceExpression);

                case ConversionMethod.Ignore:
                    return expr;

                case ConversionMethod.Convert:
                    return SqlFactory.UnaryConvert(type, this.typeProvider.From(type), expr, expr.SourceExpression);

                case ConversionMethod.Lift:
                    return new SqlLift(type, expr, this.dominatingExpression);
            }
            throw Error.UnhandledExpressionType(method);
        }

        private void CheckContext(SqlExpression expr)
        {
            SqlValue value2 = expr as SqlValue;
            if (value2 != null)
            {
                DataContext context = value2.Value as DataContext;
                if ((context != null) && (context != this.Services.Context))
                {
                    throw Error.WrongDataContext();
                }
            }
        }

        private SqlUserQuery VisitUserQuery(string query, Expression[] arguments, Type resultType)
        {
            SqlExpression[] args = new SqlExpression[arguments.Length];
            int index = 0;
            int length = args.Length;
            while (index < length)
            {
                args[index] = this.VisitExpression(arguments[index]);
                index++;
            }
            SqlUserQuery query2 = new SqlUserQuery(query, null, args, this.dominatingExpression);
            if (resultType != typeof(void))
            {
                Type elementType = TypeSystem.GetElementType(resultType);
                MetaType metaType = services.Model.GetMetaType(elementType);
                if (TypeSystem.IsSimpleType(elementType))
                {
                    SqlUserColumn column = new SqlUserColumn(elementType, this.typeProvider.From(elementType), query2,
                                                             "", false, this.dominatingExpression);
                    query2.Columns.Add(column);
                    query2.Projection = column;
                    return query2;
                }
                SqlUserRow item = new SqlUserRow(metaType.InheritanceRoot, this.typeProvider.GetApplicationType(0),
                                                 query2, this.dominatingExpression);
                query2.Projection = this.translator.BuildProjection(item, metaType, this.allowDeferred, null,
                                                                    this.dominatingExpression);
            }
            return query2;
        }

        private SqlNode VisitChangeType(Expression expression, Type type)
        {
            SqlExpression expr = this.VisitExpression(expression);

            //ALinq 3.0 更改动态属性的类型，如：(string)o["LastName"] 
            //o["LastName"] 无没得知，在 (string) 后再设定
            //if (expr.NodeType == SqlNodeType.Member && ((SqlMember)expr).Member is IndexMemberInfo)
            //{
            //    expr.SetClrType(type);
            //    ((SqlMember)expr).SetSqlType(sql.TypeProvider.From(type));
            //    //((IndexMemberInfo)((SqlMember)expr).Member).SetReturnType(type);
            //    return expr;
            //}

            return this.ChangeType(expr, type);
        }

        private object GetValue(Expression expression, string operation)
        {
            SqlExpression expression2 = this.VisitExpression(expression);
            if (expression2.NodeType != SqlNodeType.Value)
            {
                throw Error.NonConstantExpressionsNotSupportedFor(operation);
            }
            return ((SqlValue)expression2).Value;
        }

        private SqlNode VisitDataManipulationCall(MethodCallExpression mc)
        {
            if (!IsDataManipulationCall(mc))
            {
                throw Error.InvalidSequenceOperatorCall(mc.Method.Name);
            }
            bool flag = false;
            string name = mc.Method.Name;
            if (name != null)
            {
                if ((name == "Insert"))
                {
                    flag = true;
                    if (mc.Arguments.Count == 3)
                    {
                        //if (mc.Arguments[0].Type.GetGenericTypeDefinition() == typeof(ALinq.Table<>))
                        //{
                        //    return this.VisitMyInsert(mc.Arguments[0], mc.Arguments[1]);
                        //}
                        Debug.Assert(mc.Arguments[0] != null);
                        Debug.Assert(mc.Arguments[1] != null);
                        Debug.Assert(mc.Arguments[2] != null);
                        var result = this.VisitInsert(mc.Arguments[0], GetLambda(mc.Arguments[1]),
                                                      GetLambda(mc.Arguments[2]));
                        return result;
                    }
                    if (mc.Arguments.Count == 2)
                    {
                        Debug.Assert(mc.Arguments[0] != null);
                        Debug.Assert(mc.Arguments[1] != null);
                        var result = this.VisitInsert(mc.Arguments[0], this.GetLambda(mc.Arguments[1]));
                        return result;
                    }
                    if (mc.Arguments.Count == 1)
                    {
                        return this.VisitInsert(mc.Arguments[0], null);
                    }
                }
                else if (name == "Update")
                {
                    flag = true;
                    if (mc.Arguments.Count == 3)
                    {
                        var arg0 = mc.Arguments[0];
                        var arg1 = mc.Arguments[1];
                        var arg2 = mc.Arguments[2];

                        //MY CODE 
                        //if (arg1.NodeType == ExpressionType.Lambda && arg2.NodeType == ExpressionType.Lambda)
                        //{
                        //    return VisitMyUpdate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]),
                        //                         GetLambda(mc.Arguments[2]));
                        //    //throw new NotImplementedException();
                        //}
                        if (arg2.NodeType == ExpressionType.Constant)
                        {
                            //do generate the primary key parameter for where stament 
                            return VisitUpdate(arg0, GetLambda(arg1), null,
                                               (bool)((ConstantExpression)mc.Arguments[2]).Value);
                        }
                        //======================
                        return VisitUpdate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]),
                                           GetLambda(mc.Arguments[2]));
                    }
                    if (mc.Arguments.Count == 2)
                    {
                        if (mc.Method.GetGenericArguments().Length == 1)
                        {
                            return this.VisitUpdate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), null);
                        }
                        if (mc.Arguments[0].NodeType == ExpressionType.Lambda && mc.Arguments[1].NodeType == ExpressionType.Lambda)
                        {
                            //return this.VisitUpdate(GetLambda(mc.Arguments[0]), GetLambda(mc.Arguments[1]));
                            throw new NotImplementedException();
                        }
                        return this.VisitUpdate(mc.Arguments[0], null, this.GetLambda(mc.Arguments[1]));
                    }
                    if (mc.Arguments.Count == 1)
                    {
                        return this.VisitUpdate(mc.Arguments[0], null, null);
                    }
                }
                else if (name == "Delete")
                {
                    flag = true;
                    if (mc.Arguments.Count == 2)
                    {
                        return this.VisitDelete(mc.Arguments[0], this.GetLambda(mc.Arguments[1]));
                    }
                    if (mc.Arguments.Count == 1)
                    {
                        return this.VisitDelete(mc.Arguments[0], null);
                    }
                }
            }
            if (flag)
            {
                throw Error.QueryOperatorOverloadNotSupported(mc.Method.Name);
            }
            throw Error.QueryOperatorNotSupported(mc.Method.Name);
        }


        SqlNew CreateMemberInit(MemberInitExpression init, out bool generateIdentity)
        {
            MetaType metaType = services.Model.GetMetaType(init.Type);
            if (metaType.IsEntity && !EnableMaterializeEntityType)
            {
                throw Error.CannotMaterializeEntityType(init.Type);
            }
            SqlExpression[] args = null;
            NewExpression newExpression = init.NewExpression;

            if (newExpression.Arguments.Count > 0)
            {
                args = new SqlExpression[newExpression.Arguments.Count];
                int index = 0;
                int length = args.Length;
                while (index < length)
                {
                    args[index] = this.VisitExpression(newExpression.Arguments[index]);
                    index++;
                }
            }
            int count = init.Bindings.Count;
            var items = new List<SqlMemberAssign>();
            generateIdentity = true;
            for (int i = 0; i < count; i++)
            {
                var assignment = init.Bindings[i] as MemberAssignment;
                if (assignment == null)
                {
                    throw Error.UnhandledBindingType(init.Bindings[i].BindingType);
                }
                var dataMember = metaType.GetDataMember(assignment.Member);
                if (dataMember.IsDbGenerated)
                    generateIdentity = false;

                SqlExpression expr = this.VisitExpression(assignment.Expression);
                items.Add(new SqlMemberAssign(assignment.Member, expr));
            }

            var inheritanceType = metaType.GetInheritanceType(init.Type);
            if (generateIdentity && inheritanceType.DBGeneratedIdentityMember != null)
            {
                var member = inheritanceType.DBGeneratedIdentityMember;
                var identityExpression = GetInsertIdentityExpression(member);
                if (identityExpression != null)
                {
                    SqlMemberAssign memberAssign = new SqlMemberAssign(member.Member, identityExpression);
                    items.Insert(0, memberAssign);
                }
            }

            return this.sql.New(metaType, newExpression.Constructor, null, null, items, this.dominatingExpression);
        }

        protected virtual SqlNode VisitInsert(Expression item, LambdaExpression resultSelector)
        {
            if (item == null)
            {
                throw Error.ArgumentNull("item");
            }
            SqlInsert insert;
            MetaType inheritanceType = null;
            MetaTable table;
            SqlTable sqlTable;
            if (item.NodeType == ExpressionType.Lambda)
            {
                EnableMaterializeEntityType = true;
                var rowType = ((LambdaExpression)item).Parameters[0].Type;
                table = services.Model.GetTable(rowType);
                inheritanceType = table.RowType.GetInheritanceType(rowType);
                sqlTable = sql.Table(table, table.RowType, dominatingExpression);
                //sequence = Services.Context.GetTable(table.RowType.Type).Expression;
                bool generateIdentity;

                var exprItem = this.VisitExpression(item);
                var body = ((LambdaExpression)item).Body;
                var expr = (SqlNew)CreateMemberInit((MemberInitExpression)body, out generateIdentity);
                if (generateIdentity == false)
                    resultSelector = null;

                insert = new SqlInsert(sqlTable, expr, item);
            }
            else
            {
                this.dominatingExpression = item;
                table = services.Model.GetTable(item.Type);
                Expression sourceExpression = this.Services.Context.GetTable(table.RowType.Type).Expression;
                SqlNew expr = null;
                var expression2 = item as ConstantExpression;
                if (expression2 == null)
                {
                    throw Error.InsertItemMustBeConstant();
                }
                if (expression2.Value == null)
                {
                    throw Error.ArgumentNull("item");
                }
                var bindings = new List<SqlMemberAssign>();
                inheritanceType = table.RowType.GetInheritanceType(expression2.Value.GetType());
                SqlExpression itemExpression = this.sql.ValueFromObject(expression2.Value, true, sourceExpression);
                foreach (MetaDataMember member in inheritanceType.PersistentDataMembers)
                {
                    System.Diagnostics.Debug.Assert(member != null);
                    if (!member.IsAssociation && !member.IsVersion)
                    {
                        SqlMemberAssign memberAssign;
                        if (member.IsDbGenerated)
                        {
                            var identityExpression = GetInsertIdentityExpression(member);
                            if (identityExpression == null)
                                continue;

                            memberAssign = new SqlMemberAssign(member.Member, identityExpression);
                        }
                        else
                        {
                            memberAssign = new SqlMemberAssign(member.Member, sql.Member(itemExpression, member.Member));
                        }
                        bindings.Add(memberAssign);
                    }
                }
                ConstructorInfo constructor = inheritanceType.Type.GetConstructor(Type.EmptyTypes);
                expr = sql.New(inheritanceType, constructor, null, null, bindings, item);
                sqlTable = sql.Table(table, table.RowType, dominatingExpression);
                insert = new SqlInsert(sqlTable, expr, item);
            }

            if (resultSelector == null)
            {
                return insert;
            }
            MetaDataMember dBGeneratedIdentityMember = inheritanceType.DBGeneratedIdentityMember;
            bool flag = false;
            if (dBGeneratedIdentityMember != null)
            {

                flag = this.IsDbGeneratedKeyProjectionOnly(resultSelector.Body, dBGeneratedIdentityMember);
                if (dBGeneratedIdentityMember.Type == typeof(Guid) &&
                    (this.ConverterStrategy & ConverterStrategy.CanOutputFromInsert) != ConverterStrategy.Default)
                {
                    insert.OutputKey = new SqlColumn(dBGeneratedIdentityMember.Type,
                                                     this.sql.Default(dBGeneratedIdentityMember),
                                                     dBGeneratedIdentityMember.Name, dBGeneratedIdentityMember, null,
                                                     this.dominatingExpression);
                    if (!flag)
                    {
                        insert.OutputToLocal = true;
                    }
                }
            }
            SqlSelect select = null;
            SqlSelect select2 = null;
            SqlAlias alias = new SqlAlias(sqlTable);
            SqlAliasRef ref2 = new SqlAliasRef(alias);
            this.map.Add(resultSelector.Parameters[0], ref2);
            SqlExpression selection = this.VisitExpression(resultSelector.Body);
            SqlExpression expression5 = null;
            if (dBGeneratedIdentityMember != null)
            {
                expression5 = this.sql.Binary(SqlNodeType.EQ, this.sql.Member(ref2, dBGeneratedIdentityMember.Member),
                                              this.GetReturnIdentityExpression(dBGeneratedIdentityMember,
                                                                         insert.OutputKey != null));
            }
            else
            {
                SqlExpression right = this.VisitExpression(item);
                expression5 = this.sql.Binary(SqlNodeType.EQ2V, ref2, right);
            }
            select = new SqlSelect(selection, alias, resultSelector);
            select.Where = expression5;
            if ((dBGeneratedIdentityMember != null) && flag)
            {
                if (insert.OutputKey == null)
                {
                    SqlExpression identityExpression = this.GetReturnIdentityExpression(dBGeneratedIdentityMember, false);
                    if (identityExpression.ClrType != dBGeneratedIdentityMember.Type)
                    {
                        IProviderType sqlType = this.sql.Default(dBGeneratedIdentityMember);
                        identityExpression = this.sql.ConvertTo(dBGeneratedIdentityMember.Type, sqlType,
                                                                identityExpression);
                    }
                    ParameterExpression expression = Expression.Parameter(dBGeneratedIdentityMember.Type, "p");
                    var initializers = new Expression[] { Expression.Convert(expression, typeof(object)) };
                    LambdaExpression expression10 =
                        Expression.Lambda(Expression.NewArrayInit(typeof(object), initializers),
                                          new ParameterExpression[] { expression });
                    this.map.Add(expression, identityExpression);
                    SqlSource from = null;
                    if (mode == SqlProvider.ProviderMode.EffiProz)
                        from = alias;
                    select2 = new SqlSelect(this.VisitExpression(expression10.Body), from, expression10);
                }
                select.DoNotOutput = true;
            }
            var block = new SqlBlock(this.dominatingExpression);
            block.Statements.Add(insert);
            if (select2 != null)
            {
                block.Statements.Add(select2);
            }
            block.Statements.Add(select);
            return block;
        }

        protected virtual SqlNode VisitInsert(Expression sequence, LambdaExpression inserter,
                                              LambdaExpression resultSelector)
        {
            //var node = Visit(inserter.Body) ;
            var table = (ITable)((ConstantExpression)sequence).Value;
            var sourceExpression = table.Expression;
            var assignments = new List<SqlAssign>();

            var sqlValue = Visit(inserter.Body) as SqlValue;
            Debug.Assert(sqlValue != null);

            var t = services.Model.GetMetaType(sqlValue.Value.GetType());

            ReadOnlyCollection<MetaDataMember> dataMembers;
            if (t.PersistentDataMembers.Count > 0)
                dataMembers = t.PersistentDataMembers;
            else
            {
                dataMembers = t.DataMembers.Where(o => o.Member.MemberType == MemberTypes.Property).ToList().AsReadOnly();
            }

            var changedProperties = sqlValue.Value as IUpdateProperties;
            if (changedProperties != null)
            {
                dataMembers = dataMembers.Where(o => changedProperties.Properties.Contains(o.Name))
                                         .ToList().AsReadOnly();
            }

            foreach (var dataMember in dataMembers)
            {
                if (dataMember.IsAssociation || dataMember.IsDbGenerated ||
                   !dataMember.StorageAccessor.HasAssignedValue(sqlValue.Value))
                {
                    continue;
                }
                var variableName = QuoteName(dataMember.MappedName);
                var sqlVariable = new SqlVariable(dataMember.Type, null, variableName, sourceExpression);
                var sqlExpression = sql.Value(dataMember.Type, typeProvider.From(dataMember.Type),
                                              dataMember.MemberAccessor.GetBoxedValue(sqlValue.Value), true, sourceExpression);
                var sqlAssign = new SqlAssign(sqlVariable, sqlExpression, sourceExpression);
                assignments.Add(sqlAssign);
            }

            var select = new RetypeCheckClause().VisitSelect(VisitSequence(sourceExpression));
            var block = new SqlBlock(this.dominatingExpression);
            var update = new SqlUpdate(select, assignments, sourceExpression) { IsInsert = true };
            block.Statements.Add(update);

            var rowType = inserter.Parameters[0].Type;
            var metaTable = Services.Model.GetTable(table.ElementType);
            var inheritanceType = metaTable.RowType.GetInheritanceType(rowType);
            MetaDataMember dBGeneratedIdentityMember = inheritanceType.DBGeneratedIdentityMember;
            if (dBGeneratedIdentityMember != null)
            {
                SqlExpression identityExpression = this.GetReturnIdentityExpression(dBGeneratedIdentityMember, false);
                if (identityExpression.ClrType != dBGeneratedIdentityMember.Type)
                {
                    IProviderType sqlType = this.sql.Default(dBGeneratedIdentityMember);
                    identityExpression = this.sql.ConvertTo(dBGeneratedIdentityMember.Type, sqlType,
                                                            identityExpression);
                }
                ParameterExpression expression = Expression.Parameter(dBGeneratedIdentityMember.Type, "p");
                var initializers = new Expression[] { Expression.Convert(expression, typeof(object)) };
                LambdaExpression expression10 =
                    Expression.Lambda(Expression.NewArrayInit(typeof(object), initializers), new[] { expression });
                this.map.Add(expression, identityExpression);
                //SqlSource from = null;
                //if (mode == SqlProvider.ProviderMode.EffiProz)
                //    from = alias;
                var select2 = new SqlSelect(this.VisitExpression(expression10.Body), null, expression10);
                block.Statements.Add(select2);
            }

            return block;

        }
        protected virtual SqlExpression GetInsertIdentityExpression(MetaDataMember member)
        {
            return null;
        }

        #region MyRegion
        //protected SqlNode VisitInsert1(Expression item, LambdaExpression resultSelector)
        //{
        //    if (item == null)
        //    {
        //        throw Error.ArgumentNull("item");
        //    }
        //    Type type;
        //    if (item.Type.IsGenericType && item.Type.GetGenericTypeDefinition() == typeof(ALinq.Table<>))
        //    {
        //        //return VisitMyInsert(item, resultSelector);
        //        type = item.Type.GetGenericArguments()[0];
        //    }
        //    else
        //    {
        //        type = item.Type;
        //    }
        //    this.dominatingExpression = item;
        //    MetaTable table = Services.Model.GetTable(type); ;//Services.Model.GetTable(item.Type);
        //    Expression sourceExpression = this.Services.Context.GetTable(table.RowType.Type).Expression;
        //    MetaType inheritanceType = null;
        //    SqlNew expr = null;
        //    var expression2 = item as ConstantExpression;
        //    if (expression2 == null)
        //    {
        //        throw Error.InsertItemMustBeConstant();
        //    }
        //    if (expression2.Value == null)
        //    {
        //        throw Error.ArgumentNull("item");
        //    }
        //    var bindings = new List<SqlMemberAssign>();

        //    type = expression2.Value.GetType();
        //    if (type.IsGenericType && item.Type.GetGenericTypeDefinition() == typeof(ALinq.Table<>))
        //        type = type.GetGenericArguments()[0];

        //    inheritanceType = table.RowType.GetInheritanceType(type);
        //    //SqlExpression expression3 = this.sql.ValueFromObject(expression2.Value, true, sourceExpression);
        //    SqlExpression expression3 = this.sql.ValueFromObject(type, true, sourceExpression);
        //    foreach (MetaDataMember member in inheritanceType.PersistentDataMembers)
        //    {
        //        if ((!member.IsAssociation && !member.IsDbGenerated) && !member.IsVersion)
        //        {
        //            bindings.Add(new SqlMemberAssign(member.Member, this.sql.Member(expression3, member.Member)));
        //        }
        //    }
        //    ConstructorInfo constructor = inheritanceType.Type.GetConstructor(Type.EmptyTypes);
        //    expr = sql.New(inheritanceType, constructor, null, null, bindings, item);
        //    SqlTable table2 = sql.Table(table, table.RowType, dominatingExpression);
        //    var insert = new SqlInsert(table2, expr, item);
        //    if (resultSelector == null)
        //    {
        //        return insert;
        //    }
        //    MetaDataMember dBGeneratedIdentityMember = inheritanceType.DBGeneratedIdentityMember;
        //    bool flag = false;
        //    if (dBGeneratedIdentityMember != null)
        //    {
        //        flag = this.IsDbGeneratedKeyProjectionOnly(resultSelector.Body, dBGeneratedIdentityMember);
        //        if ((dBGeneratedIdentityMember.Type == typeof(Guid)) &&
        //            ((this.ConverterStrategy & ConverterStrategy.CanOutputFromInsert) != ConverterStrategy.Default))
        //        {
        //            insert.OutputKey = new SqlColumn(dBGeneratedIdentityMember.Type,
        //                                             this.sql.Default(dBGeneratedIdentityMember),
        //                                             dBGeneratedIdentityMember.Name, dBGeneratedIdentityMember, null,
        //                                             this.dominatingExpression);
        //            if (!flag)
        //            {
        //                insert.OutputToLocal = true;
        //            }
        //        }
        //    }
        //    SqlSelect select = null;
        //    SqlSelect select2 = null;
        //    SqlAlias alias = new SqlAlias(table2);
        //    SqlAliasRef ref2 = new SqlAliasRef(alias);
        //    this.map.Add(resultSelector.Parameters[0], ref2);
        //    SqlExpression selection = this.VisitExpression(resultSelector.Body);
        //    SqlExpression expression5 = null;
        //    if (dBGeneratedIdentityMember != null)
        //    {
        //        expression5 = this.sql.Binary(SqlNodeType.EQ, this.sql.Member(ref2, dBGeneratedIdentityMember.Member),
        //                                      this.GetIdentityExpression(dBGeneratedIdentityMember,
        //                                                                 insert.OutputKey != null));
        //    }
        //    else
        //    {
        //        SqlExpression right = this.VisitExpression(item);
        //        expression5 = this.sql.Binary(SqlNodeType.EQ2V, ref2, right);
        //    }
        //    select = new SqlSelect(selection, alias, resultSelector);
        //    select.Where = expression5;
        //    if ((dBGeneratedIdentityMember != null) && flag)
        //    {
        //        if (insert.OutputKey == null)
        //        {
        //            SqlExpression identityExpression = this.GetIdentityExpression(dBGeneratedIdentityMember, false);
        //            if (identityExpression.ClrType != dBGeneratedIdentityMember.Type)
        //            {
        //                IProviderType sqlType = this.sql.Default(dBGeneratedIdentityMember);
        //                identityExpression = this.sql.ConvertTo(dBGeneratedIdentityMember.Type, sqlType,
        //                                                        identityExpression);
        //            }
        //            ParameterExpression expression = Expression.Parameter(dBGeneratedIdentityMember.Type, "p");
        //            var initializers = new Expression[] { Expression.Convert(expression, typeof(object)) };
        //            LambdaExpression expression10 =
        //                Expression.Lambda(Expression.NewArrayInit(typeof(object), initializers),
        //                                  new ParameterExpression[] { expression });
        //            this.map.Add(expression, identityExpression);
        //            SqlSource from = null;
        //            if (mode == SqlProvider.ProviderMode.EffiProz)
        //                from = alias;
        //            select2 = new SqlSelect(this.VisitExpression(expression10.Body), from, expression10);
        //        }
        //        select.DoNotOutput = true;
        //    }
        //    var block = new SqlBlock(this.dominatingExpression);
        //    block.Statements.Add(insert);
        //    if (select2 != null)
        //    {
        //        block.Statements.Add(select2);
        //    }
        //    block.Statements.Add(select);
        //    return block;
        //} 
        #endregion

        private SqlNode VisitDelete(Expression item, LambdaExpression check)
        {
            SqlStatement statement;
            if (item == null)
            {
                throw Error.ArgumentNull("item");
            }

            if (item.NodeType == ExpressionType.Constant)
            {
                var value = ((ConstantExpression)item).Value;
                if (value is Type)
                {
                    return VisitDelete((Type)value, check);
                }
            }

            bool oldValue = allowDeferred;
            this.allowDeferred = false;
            try
            {
                LambdaExpression expression;

                MetaTable table = services.Model.GetTable(item.Type);
                Expression sourceExpression = this.Services.Context.GetTable(table.RowType.Type).Expression;
                Type type = table.RowType.Type;
                ParameterExpression left = Expression.Parameter(type, "p");
                expression = Expression.Lambda(Expression.Equal(left, item), new[] { left });
                if (check != null)
                {
                    expression = Expression.Lambda(Expression.And(Expression.Invoke(expression, new Expression[] { left }),
                                                    Expression.Invoke(check, new Expression[] { left })), new[] { left });
                }

                Expression exp = Expression.Call(typeof(Enumerable), "Where", new[] { type },
                                                 new[] { sourceExpression, expression });
                SqlSelect select = new RetypeCheckClause().VisitSelect(VisitSequence(exp));
                this.allowDeferred = oldValue;
                var delete = new SqlDelete(select, sourceExpression);
                statement = delete;
            }
            finally
            {
                this.allowDeferred = oldValue;
            }
            return statement;
        }

        private SqlNode VisitDelete(Type objType, LambdaExpression check)
        {
            SqlStatement statement;
            if (objType == null)
                throw Error.ArgumentNull("type");
            if (check == null)
                throw Error.ArgumentNull("check");

            bool oldValue = allowDeferred;
            this.allowDeferred = false;
            try
            {
                MetaTable table = services.Model.GetTable(objType);
                Expression sourceExpression = this.Services.Context.GetTable(table.RowType.Type).Expression;
                Type type = table.RowType.Type;
                ParameterExpression left = Expression.Parameter(type, "p");

                Expression exp = Expression.Call(typeof(Enumerable), "Where", new[] { type },
                                                 new[] { sourceExpression, check });
                var select = new RetypeCheckClause().VisitSelect(VisitSequence(exp));
                allowDeferred = oldValue;
                var delete = new SqlDelete(select, sourceExpression);
                statement = delete;
            }
            finally
            {
                this.allowDeferred = oldValue;
            }
            return statement;

            #region MyRegion
            //SqlStatement statement;
            //if (item == null)
            //{
            //    throw Error.ArgumentNull("item");
            //}
            //bool oldValue = allowDeferred;
            //this.allowDeferred = false;
            //try
            //{
            //    MetaTable table = item;//Services.Model.GetTable(item.Type);
            //    Expression sourceExpression = this.Services.Context.GetTable(table.RowType.Type).Expression;
            //    Type type = table.RowType.Type;
            //    ParameterExpression left = Expression.Parameter(type, "p");
            //    LambdaExpression expression = Expression.Lambda(Expression.Equal(left, item), new[] { left });
            //    if (check != null)
            //    {
            //        expression = Expression.Lambda(Expression.And(Expression.Invoke(expression, new Expression[] { left }),
            //                                        Expression.Invoke(check, new Expression[] { left })), new[] { left });
            //    }
            //    Expression exp = Expression.Call(typeof(Enumerable), "Where", new[] { type },
            //                                     new[] { sourceExpression, expression });
            //    SqlSelect select = new RetypeCheckClause().VisitSelect(VisitSequence(exp));
            //    this.allowDeferred = oldValue;
            //    var delete = new SqlDelete(select, sourceExpression);
            //    statement = delete;
            //}
            //finally
            //{
            //    this.allowDeferred = oldValue;
            //}
            ///**/
            //return statement; 
            #endregion
        }

        class MyUpdateVisitor : SqlVisitor
        {
            private readonly MemberInfo[] members;
            private readonly MetaType metaType;
            private readonly SqlIdentifier sqlIdentifier;
            private SqlProvider.ProviderMode mode;

            public MyUpdateVisitor(MemberInfo[] members, MetaType metaType, SqlProvider sqlProvider)
            {
                this.members = members;
                this.metaType = metaType;
                this.sqlIdentifier = sqlProvider.SqlIdentifier;
                this.mode = sqlProvider.Mode;
            }

            protected override SqlNode VisitMember(SqlMember m)
            {
                var member = members.Where(o => o.Name == m.Member.Name).SingleOrDefault();
                if (member != null)
                {
                    var dataMember = metaType.GetDataMember(member);
                    return new SqlVariable(m.ClrType, m.SqlType, QuoteName(dataMember.MappedName), m.SourceExpression);
                }
                return new SqlVariable(m.ClrType, m.SqlType, QuoteName(m.Member.Name), m.SourceExpression);
            }

            string QuoteName(string name)
            {
                switch (mode)
                {
                    case SqlProvider.ProviderMode.Firebird:
                    case SqlProvider.ProviderMode.Oracle:
                    case SqlProvider.ProviderMode.OdpOracle:
                    case SqlProvider.ProviderMode.Pgsql:
                    case SqlProvider.ProviderMode.DB2:
                        return name;
                    default:
                        return sqlIdentifier.QuoteCompoundIdentifier(name);
                }
            }
        }

        class MyInsertVisitor : SqlVisitor
        {
            //private readonly MemberInfo[] members;
            private readonly MetaType metaType;
            private SqlTable sqlTable;

            public MyInsertVisitor(MetaType metaType, SqlTable sqlTable)
            {
                //this.members = members;
                this.metaType = metaType;
                this.sqlTable = sqlTable;
            }

            protected override SqlNode VisitMember(SqlMember m)
            {

                var column = new SqlColumn(m.ClrType, m.SqlType, m.Member.Name, this.metaType.GetDataMember(m.Member), null,
                                           m.SourceExpression);
                sqlTable.Columns.Add(column);
                return new SqlColumnRef(column);

            }


        }

        string QuoteName(string name)
        {
            switch (mode)
            {
                case SqlProvider.ProviderMode.Firebird:
                case SqlProvider.ProviderMode.Oracle:
                case SqlProvider.ProviderMode.OdpOracle:
                case SqlProvider.ProviderMode.DB2:
                case SqlProvider.ProviderMode.Pgsql:
                    return name;
                default:
                    return sqlIdentifier.QuoteCompoundIdentifier(name);
            }
        }

        protected SqlInsert CreateInsertExpression(LambdaExpression item, Expression resultSelector)
        {
            //MetaTable table = Services.Model.GetTable(item.Type);
            var table = services.Model.GetTable(item.Body.Type); //Services.Model.GetTable(exprType);
            var sequence = Services.Context.GetTable(table.RowType.Type).Expression;
            EnableMaterializeEntityType = true;
            var alias = new SqlAlias(VisitSequence(sequence));
            var ref2 = new SqlAliasRef(alias);
            map[item.Parameters[0]] = ref2;
            var node = Visit(item.Body);

            var selection = node as SqlExpression;
            if (selection == null)
            {
                throw Error.BadProjectionInSelect();
            }
            var exprType = item.Parameters[0].Type;
            var metaType = table.RowType;
            var sourceExpression = Services.Context.GetTable(table.RowType.Type).Expression;

            var sqlTable = sql.Table(table, metaType, dominatingExpression);
            new MyInsertVisitor(metaType, sqlTable).VisitExpression(selection);
            var insert = new SqlInsert(sqlTable, selection, sourceExpression);

            return insert;

        }

        private SqlNode VisitMyUpdate(Expression sequence, LambdaExpression selector, LambdaExpression predicate)
        {
            EnableMaterializeEntityType = true;
            var alias = new SqlAlias(VisitSequence(sequence));
            var ref2 = new SqlAliasRef(alias);
            map[selector.Parameters[0]] = ref2;
            var node = Visit(selector.Body);
            var selection = node as SqlExpression;
            if (selection == null)
            {
                throw Error.BadProjectionInSelect();
            }
            var exprType = selector.Parameters[0].Type;
            var obj = Activator.CreateInstance(exprType);
            var exprObj = Expression.Constant(obj);

            MemberInfo[] members = exprType.GetMembers();
            var table = services.Model.GetTable(exprType);
            var metaType = table.RowType;

            var sourceExpression = Services.Context.GetTable(table.RowType.Type).Expression;

            var assignments = new List<SqlAssign>();
            //var sqlIdentifier = ((SqlProvider)Services.Context.Provider).SqlIdentifier;
            //((SqlProvider)Services.Context.Provider).Mode
            if (node.NodeType == SqlNodeType.Value)
            {
                var sqlValue = (SqlValue)node;
                #region MyRegion
                //var propertyInfos = sqlValue.ClrType.GetProperties();
                //foreach (var propertyInfo in propertyInfos)
                //{
                //    string variableName;
                //    var property = propertyInfo;
                //    var member = members.Where(o => o.Name == property.Name).SingleOrDefault();
                //    if (member != null)
                //    {
                //        var dataMember = metaType.GetDataMember(member);
                //        variableName = dataMember.MappedName;
                //    }
                //    else
                //    {
                //        variableName = property.Name;
                //    }
                //    var sqlVariable = new SqlVariable(propertyInfo.PropertyType, null, variableName, sourceExpression);
                //    var sqlExpression = sql.Value(propertyInfo.PropertyType, typeProvider.From(propertyInfo.PropertyType),
                //                                  propertyInfo.GetValue(sqlValue.Value, null), true, sourceExpression);
                //    var sqlAssign = new SqlAssign(sqlVariable, sqlExpression, sourceExpression);
                //    assignments.Add(sqlAssign);
                //} 
                #endregion

                var t = services.Model.GetMetaType(sqlValue.Value.GetType());

                ReadOnlyCollection<MetaDataMember> dataMembers;
                if (t.PersistentDataMembers.Count > 0)
                    dataMembers = t.PersistentDataMembers;
                else
                {
                    dataMembers = t.DataMembers.Where(o => o.Member.MemberType == MemberTypes.Property).ToList().AsReadOnly();
                }
                var changedProperties = sqlValue.Value as IUpdateProperties;
                if (changedProperties != null)
                {
                    dataMembers = dataMembers.Where(o => changedProperties.Properties.Contains(o.Name))
                                             .ToList().AsReadOnly();
                }

                foreach (var dataMember in dataMembers)
                {
                    if (dataMember.IsAssociation || dataMember.IsDbGenerated ||
                       !dataMember.StorageAccessor.HasAssignedValue(sqlValue.Value))
                    {
                        continue;
                    }
                    var variableName = QuoteName(dataMember.MappedName);
                    var sqlVariable = new SqlVariable(dataMember.Type, null, variableName, sourceExpression);
                    var sqlExpression = sql.Value(dataMember.Type, typeProvider.From(dataMember.Type),
                                                  dataMember.MemberAccessor.GetBoxedValue(sqlValue.Value), true, sourceExpression);
                    var sqlAssign = new SqlAssign(sqlVariable, sqlExpression, sourceExpression);
                    assignments.Add(sqlAssign);
                }
            }
            else if (node.NodeType == SqlNodeType.New)
            {
                var sqlNew = (SqlNew)node;
                var visitor = new MyUpdateVisitor(members, metaType, (SqlProvider)Services.Context.Provider);
                for (var i = 0; i < sqlNew.ArgMembers.Count; i++)
                {
                    var value = visitor.VisitExpression(sqlNew.Args[i]);

                    var index = i;
                    string variableName;
                    var member = members.Where(o => o.Name == sqlNew.ArgMembers[index].Name).SingleOrDefault();
                    if (member != null)
                    {
                        var dataMember = metaType.GetDataMember(member);
                        if (dataMember == null || dataMember.IsAssociation || dataMember.IsDbGenerated ||
                            !dataMember.StorageAccessor.HasAssignedValue(value))
                        {
                            continue;
                        }
                        variableName = dataMember.MappedName;
                    }
                    else
                    {
                        variableName = sqlNew.ArgMembers[index].Name;
                    }
                    var sqlVariable = new SqlVariable(value.ClrType, null, QuoteName(variableName), sourceExpression);
                    var sqlAssign = new SqlAssign(sqlVariable, value, sourceExpression);
                    assignments.Add(sqlAssign);
                }
                sqlNew.Members.Reverse();
                for (var i = 0; i < sqlNew.Members.Count; i++)
                {
                    var m = sqlNew.Members[i];
                    if (m.Expression is SqlMember)
                    {
                        if (((SqlMember)m.Expression).Member == m.Member)
                            continue;
                    }
                    var value = visitor.VisitExpression(m.Expression);
                    var member = m.Member;
                    string variableName = metaType.GetDataMember(member).MappedName;
                    var sqlVariable = new SqlVariable(value.ClrType, null, QuoteName(variableName), sourceExpression);
                    var sqlAssign = new SqlAssign(sqlVariable, value, sourceExpression);
                    assignments.Add(sqlAssign);
                }

                EnableMaterializeEntityType = false;
            }

            var type = table.RowType.Type;
            var expParameter = Expression.Parameter(type, "p");
            var r = Expression.Invoke(predicate, new Expression[] { expParameter });
            var expression = Expression.Lambda(r, new[] { expParameter });

            Expression exp = Expression.Call(typeof(Enumerable), "Where", new[] { type }, new[] { sourceExpression, expression });

            var select = new RetypeCheckClause().VisitSelect(VisitSequence(exp));
            select.Selection = null;
            var update = new SqlUpdate(select, assignments, sourceExpression);
            return update;
        }


        private SqlNode VisitUpdate(Expression expr, LambdaExpression check, LambdaExpression resultSelector)
        {
            //return VisitUpdate(expr, check, resultSelector, false);
            SqlStatement statement;
            if (expr == null)
            {
                throw Error.ArgumentNull("item");
            }
            var exprType = expr.Type;

            if (exprType.IsGenericType && exprType.GetGenericTypeDefinition() == typeof(ALinq.Table<>))
            {
                return VisitMyUpdate(expr, check, resultSelector);
            }

            MetaTable table = services.Model.GetTable(exprType);
            var sourceExpression = Services.Context.GetTable(table.RowType.Type).Expression;
            var type = table.RowType.Type;
            var deferred = allowDeferred;

            allowDeferred = false;

            try
            {
                var expParameter = Expression.Parameter(type, "p");
                var lambdaExpression = Expression.Lambda(Expression.Equal(expParameter, expr), new[] { expParameter });
                var expression = lambdaExpression;
                Expression exp;
                //if (!disablePrimaryKey)
                //{
                if (check != null)
                {
                    var l = Expression.Invoke(expression, new Expression[] { expParameter });
                    var r = Expression.Invoke(check, new Expression[] { expParameter });
                    expression = Expression.Lambda(Expression.And(l, r), new[] { expParameter });
                }
                exp = Expression.Call(typeof(Enumerable), "Where", new[] { type }, new[] { sourceExpression, expression });
                //}
                //else
                //{
                //    if (check == null)
                //        throw Error.ArgumentNull("check");
                //    exp = Expression.Call(typeof(Enumerable), "Where", new[] { type }, new[] { sourceExpression, check });
                //}
                var select = new RetypeCheckClause().VisitSelect(VisitSequence(exp));
                var assignments = new List<SqlAssign>();//(IList)Activator.CreateInstance(t);
                var expItem = expr as ConstantExpression;

                if (expItem == null)
                    throw Error.UpdateItemMustBeConstant();

                if (expItem.Value == null)
                    throw Error.ArgumentNull("item");

                var type2 = expItem.Value.GetType();
                var metaType = services.Model.GetMetaType(type2);
                var source = Activator.CreateInstance(type2);
                var modifiedMembers = Services.Context.GetTable(metaType.InheritanceRoot.Type)
                                                      .GetModifiedMembers(expItem.Value);
                //if (modifiedMembers.Length == 0)
                //{
                //    var list = new List<ModifiedMemberInfo>();
                //    foreach (var dataMember in metaType.PersistentDataMembers)
                //    {
                //        if (dataMember.IsAssociation || dataMember.IsDbGenerated ||
                //            !dataMember.StorageAccessor.HasAssignedValue(expItem.Value))
                //        {
                //            continue;
                //        }
                //        var value1 = dataMember.MemberAccessor.GetBoxedValue(expItem.Value);
                //        var value2 = dataMember.MemberAccessor.GetBoxedValue(source);
                //        if (Equals(value1, value2))
                //            continue;

                //        list.Add(new ModifiedMemberInfo(dataMember.Member, value1, value2));
                //    }
                //    modifiedMembers = list.ToArray();
                //}
                foreach (var info in modifiedMembers)
                {
                    MetaDataMember dataMember = metaType.GetDataMember(info.Member);
                    Type memberType;

                    //if (dataMember.Type == typeof(object) && info.CurrentValue != null)
                    //{
                    //    memberType = info.CurrentValue.GetType();
                    //    ((DynamicMetaDataMember)dataMember).SetType(memberType);
                    //}
                    //else
                    //{
                    memberType = dataMember.Type;
                    //}

                    IProviderType sqlType = typeProvider.From(memberType);
                    SqlValue sqlValue = new SqlValue(memberType, sqlType, info.CurrentValue, true, sourceExpression);
                    var sqlMember = sql.Member(select.Selection, info.Member);
                    var sqlAssign = new SqlAssign(sqlMember, sqlValue, sourceExpression);
                    assignments.Add(sqlAssign);
                }

                var update = new SqlUpdate(select, assignments, sourceExpression);
                if (resultSelector == null)
                {
                    return update;
                }
                exp = sourceExpression;
                exp = Expression.Call(typeof(Enumerable), "Where", new[] { type }, new[] { exp, lambdaExpression });
                exp = Expression.Call(typeof(Enumerable), "Select", new[] { type, resultSelector.Body.Type },
                                      new[] { exp, resultSelector });
                var select2 = VisitSequence(exp);
                var mode = ((SqlProvider)Services.Context.Provider).Mode;
                if (mode == SqlProvider.ProviderMode.SQLite || mode == SqlProvider.ProviderMode.Access ||
                    mode == SqlProvider.ProviderMode.MySql || mode == SqlProvider.ProviderMode.Firebird ||
                    mode == SqlProvider.ProviderMode.NotYetDecided)
                {
                    select2.Where = select2.Where;
                }
                else
                {
                    var left1 = sql.Binary(SqlNodeType.GT, GetRowCountExpression(),
                                          sql.ValueFromObject(0, false, dominatingExpression));
                    select2.Where = sql.AndAccumulate(left1, select2.Where);
                }


                var block = new SqlBlock(sourceExpression);
                block.Statements.Add(update);
                block.Statements.Add(select2);
                statement = block;
            }
            catch (Exception exc)
            {
                throw exc;
            }
            finally
            {
                allowDeferred = deferred;
            }
            return statement;
        }


        private SqlNode VisitUpdate(Expression expr, LambdaExpression check, LambdaExpression resultSelector, bool disablePrimaryKey)
        {
            SqlStatement statement;
            if (expr == null)
            {
                throw Error.ArgumentNull("item");
            }
            var exprType = expr.Type;
            var table = services.Model.GetTable(exprType);
            var sourceExpression = Services.Context.GetTable(table.RowType.Type).Expression;
            var type = table.RowType.Type;
            var deferred = allowDeferred;

            allowDeferred = false;

            try
            {
                var expParameter = Expression.Parameter(type, "p");
                var lambdaExpression = Expression.Lambda(Expression.Equal(expParameter, expr), new[] { expParameter });
                var expression = lambdaExpression;
                Expression exp;
                if (!disablePrimaryKey)
                {
                    if (check != null)
                    {
                        var l = Expression.Invoke(expression, new Expression[] { expParameter });
                        var r = Expression.Invoke(check, new Expression[] { expParameter });
                        expression = Expression.Lambda(Expression.And(l, r), new[] { expParameter });
                    }
                    exp = Expression.Call(typeof(Enumerable), "Where", new[] { type }, new[] { sourceExpression, expression });
                }
                else
                {
                    if (check == null)
                        throw Error.ArgumentNull("check");
                    exp = Expression.Call(typeof(Enumerable), "Where", new[] { type }, new[] { sourceExpression, check });
                }
                var select = new RetypeCheckClause().VisitSelect(VisitSequence(exp));
                var assignments = new List<SqlAssign>();//(IList)Activator.CreateInstance(t);
                var expItem = expr as ConstantExpression;

                if (expItem == null)
                    throw Error.UpdateItemMustBeConstant();

                if (expItem.Value == null)
                    throw Error.ArgumentNull("item");

                var type2 = expItem.Value.GetType();
                var metaType = services.Model.GetMetaType(type2);
                var source = Activator.CreateInstance(type2);
                ModifiedMemberInfo[] modifiedMembers;
                if (expItem.Value is IUpdateProperties)
                {
                    //modifiedMembers = new ModifiedMemberInfo[10];
                    var dataMembers = metaType.PersistentDataMembers;
                    modifiedMembers = dataMembers.Where(o => ((IUpdateProperties)expItem.Value).Properties.Contains(o.Name))
                                                 .Select(o => new ModifiedMemberInfo(o.Member,
                                                                                     o.MemberAccessor.GetBoxedValue(expItem.Value),
                                                                                     o.MemberAccessor.GetBoxedValue(source)))
                                                 .ToArray();
                }
                else
                {
                    modifiedMembers = Services.Context.GetTable(metaType.InheritanceRoot.Type)
                                              .GetModifiedMembers(expItem.Value);
                    if (modifiedMembers.Length == 0)
                    {
                        var list = new List<ModifiedMemberInfo>();
                        foreach (var dataMember in metaType.PersistentDataMembers)
                        {
                            if (dataMember.IsAssociation || dataMember.IsDbGenerated ||
                                !dataMember.StorageAccessor.HasAssignedValue(expItem.Value))
                            {
                                continue;
                            }
                            var value1 = dataMember.MemberAccessor.GetBoxedValue(expItem.Value);
                            var value2 = dataMember.MemberAccessor.GetBoxedValue(source);
                            if (Equals(value1, value2))
                                continue;

                            list.Add(new ModifiedMemberInfo(dataMember.Member, value1, value2));
                        }
                        modifiedMembers = list.ToArray();
                    }
                }
                foreach (var info in modifiedMembers)
                {
                    MetaDataMember dataMember = metaType.GetDataMember(info.Member);
                    var sqlValue = new SqlValue(dataMember.Type, typeProvider.From(dataMember.Type),
                                                info.CurrentValue, true, sourceExpression);
                    var sqlMember = sql.Member(select.Selection, info.Member);
                    var sqlAssign = new SqlAssign(sqlMember, sqlValue, sourceExpression);
                    assignments.Add(sqlAssign);
                }

                var update = new SqlUpdate(select, assignments, sourceExpression);
                if (resultSelector == null)
                {
                    return update;
                }
                exp = sourceExpression;
                exp = Expression.Call(typeof(Enumerable), "Where", new[] { type }, new[] { exp, lambdaExpression });
                exp = Expression.Call(typeof(Enumerable), "Select", new[] { type, resultSelector.Body.Type },
                                      new[] { exp, resultSelector });
                var select2 = VisitSequence(exp);
                var mode = ((SqlProvider)Services.Context.Provider).Mode;
                if (mode == SqlProvider.ProviderMode.SQLite || mode == SqlProvider.ProviderMode.Access ||
                    mode == SqlProvider.ProviderMode.MySql || mode == SqlProvider.ProviderMode.Firebird ||
                    mode == SqlProvider.ProviderMode.NotYetDecided)
                {
                    select2.Where = select2.Where;
                }
                else
                {
                    var left1 = sql.Binary(SqlNodeType.GT, GetRowCountExpression(),
                                          sql.ValueFromObject(0, false, dominatingExpression));
                    select2.Where = sql.AndAccumulate(left1, select2.Where);
                }


                var block = new SqlBlock(sourceExpression);
                block.Statements.Add(update);
                block.Statements.Add(select2);
                statement = block;
            }
            finally
            {
                allowDeferred = deferred;
            }
            return statement;
        }

        private SqlExpression GetRowCountExpression()
        {
            if ((ConverterStrategy & ConverterStrategy.CanUseRowStatus) != ConverterStrategy.Default)
            {
                return new SqlVariable(typeof(decimal), typeProvider.From(typeof(decimal)), "@@ROWCOUNT",
                                       dominatingExpression);
            }
            return new SqlVariable(typeof(decimal), typeProvider.From(typeof(decimal)), "@ROWCOUNT",
                                   dominatingExpression);
        }

        

        public ConverterStrategy ConverterStrategy
        {
            get;
            //{
            //const BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField;
            //return (ConverterStrategy)SourceType.InvokeMember("converterStrategy", bf, null, Source, null);
            //}
            set;
            //{
            //const BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField;
            //SourceType.InvokeMember("converterStrategy", bf, null, Source, new object[] { value }); ;
            //}
        }

        protected bool IsDbGeneratedKeyProjectionOnly(Expression projection, MetaDataMember keyMember)
        {
            if (mode == SqlProvider.ProviderMode.Access)
                return true;

            NewArrayExpression expression = projection as NewArrayExpression;
            if ((expression != null) && (expression.Expressions.Count == 1))
            {
                Expression operand = expression.Expressions[0];
                while ((operand.NodeType == ExpressionType.Convert) ||
                       (operand.NodeType == ExpressionType.ConvertChecked))
                {
                    operand = ((UnaryExpression)operand).Operand;
                }
                MemberExpression expression3 = operand as MemberExpression;
                if ((expression3 != null) && (expression3.Member == keyMember.Member))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsDataManipulationCall(MethodCallExpression mc)
        {
            if (!mc.Method.IsStatic)
                return false;
            return (mc.Method.DeclaringType == typeof(DataManipulation));
        }

        private SqlNode VisitSequenceOperatorCall(MethodCallExpression mc)
        {
            Type declaringType = mc.Method.DeclaringType;
            bool flag = false;
            if (!IsSequenceOperatorCall(mc))
            {
                throw Error.InvalidSequenceOperatorCall(declaringType);
            }
            switch (mc.Method.Name)
            {
                //case "Update":
                //    //var arg0 = (ConstantExpression)mc.Arguments[0];

                //    //return VisitUpdate(mc.Arguments[0], GetLambda(mc.Arguments[1]));
                //    return VisitUpdate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]));
                //    break;

                case "Select":
                    flag = true;
                    if (((mc.Arguments.Count != 2) || !this.IsLambda(mc.Arguments[1])) ||
                        (GetLambda(mc.Arguments[1]).Parameters.Count != 1))
                    {
                        break;
                    }
                    return VisitSelect(mc.Arguments[0], this.GetLambda(mc.Arguments[1]));

                case "SelectMany":
                    flag = true;
                    if (((mc.Arguments.Count != 2) || !this.IsLambda(mc.Arguments[1])) ||
                        (this.GetLambda(mc.Arguments[1]).Parameters.Count != 1))
                    {
                        if ((((mc.Arguments.Count != 3) || !this.IsLambda(mc.Arguments[1])) ||
                             ((this.GetLambda(mc.Arguments[1]).Parameters.Count != 1) || !this.IsLambda(mc.Arguments[2]))) ||
                            (this.GetLambda(mc.Arguments[2]).Parameters.Count != 2))
                        {
                            break;
                        }
                        return this.VisitSelectMany(mc.Arguments[0], this.GetLambda(mc.Arguments[1]),
                                                    this.GetLambda(mc.Arguments[2]));
                    }
                    return this.VisitSelectMany(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), null);

                case "Join":
                    flag = true;
                    if ((((mc.Arguments.Count != 5) || !this.IsLambda(mc.Arguments[2])) ||
                         ((this.GetLambda(mc.Arguments[2]).Parameters.Count != 1) || !this.IsLambda(mc.Arguments[3]))) ||
                        (((this.GetLambda(mc.Arguments[3]).Parameters.Count != 1) || !this.IsLambda(mc.Arguments[4])) ||
                         (this.GetLambda(mc.Arguments[4]).Parameters.Count != 2)))
                    {
                        break;
                    }
                    return this.VisitJoin(mc.Arguments[0], mc.Arguments[1], this.GetLambda(mc.Arguments[2]),
                                          this.GetLambda(mc.Arguments[3]), this.GetLambda(mc.Arguments[4]));

                case "GroupJoin":
                    flag = true;
                    if ((((mc.Arguments.Count != 5) || !this.IsLambda(mc.Arguments[2])) ||
                         ((this.GetLambda(mc.Arguments[2]).Parameters.Count != 1) || !this.IsLambda(mc.Arguments[3]))) ||
                        (((this.GetLambda(mc.Arguments[3]).Parameters.Count != 1) || !this.IsLambda(mc.Arguments[4])) ||
                         (this.GetLambda(mc.Arguments[4]).Parameters.Count != 2)))
                    {
                        break;
                    }
                    return this.VisitGroupJoin(mc.Arguments[0], mc.Arguments[1], this.GetLambda(mc.Arguments[2]),
                                               this.GetLambda(mc.Arguments[3]), this.GetLambda(mc.Arguments[4]));

                case "DefaultIfEmpty":
                    flag = true;
                    if (mc.Arguments.Count != 1)
                    {
                        break;
                    }
                    return this.VisitDefaultIfEmpty(mc.Arguments[0]);

                case "OfType":
                    {
                        flag = true;
                        if (mc.Arguments.Count != 1)
                        {
                            break;
                        }
                        Type ofType = mc.Method.GetGenericArguments()[0];
                        return this.VisitOfType(mc.Arguments[0], ofType);
                    }
                case "Cast":
                    {
                        flag = true;
                        if (mc.Arguments.Count != 1)
                        {
                            break;
                        }
                        Type type = mc.Method.GetGenericArguments()[0];
                        return this.VisitSequenceCast(mc.Arguments[0], type);
                    }
                case "Where":
                    flag = true;
                    if (((mc.Arguments.Count != 2) || !this.IsLambda(mc.Arguments[1])) ||
                        (this.GetLambda(mc.Arguments[1]).Parameters.Count != 1))
                    {
                        break;
                    }
                    return this.VisitWhere(mc.Arguments[0], this.GetLambda(mc.Arguments[1]));

                case "First":
                case "FirstOrDefault":
                    flag = true;
                    if (mc.Arguments.Count != 1)
                    {
                        if (((mc.Arguments.Count != 2) || !this.IsLambda(mc.Arguments[1])) ||
                            (this.GetLambda(mc.Arguments[1]).Parameters.Count != 1))
                        {
                            break;
                        }
                        return this.VisitFirst(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), true);
                    }
                    return this.VisitFirst(mc.Arguments[0], null, true);

                case "Single":
                case "SingleOrDefault":
                    flag = true;
                    if (mc.Arguments.Count != 1)
                    {
                        if (((mc.Arguments.Count != 2) || !this.IsLambda(mc.Arguments[1])) ||
                            (this.GetLambda(mc.Arguments[1]).Parameters.Count != 1))
                        {
                            break;
                        }
                        return this.VisitFirst(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), false);
                    }
                    return this.VisitFirst(mc.Arguments[0], null, false);

                case "Distinct":
                    flag = true;
                    if (mc.Arguments.Count != 1)
                    {
                        break;
                    }
                    return this.VisitDistinct(mc.Arguments[0]);

                case "Concat":
                    flag = true;
                    if (mc.Arguments.Count != 2)
                    {
                        break;
                    }
                    return this.VisitConcat(mc.Arguments[0], mc.Arguments[1]);

                case "Union":
                    flag = true;
                    if (mc.Arguments.Count != 2)
                    {
                        break;
                    }
                    return this.VisitUnion(mc.Arguments[0], mc.Arguments[1]);

                case "Intersect":
                    flag = true;
                    if (mc.Arguments.Count != 2)
                    {
                        break;
                    }
                    return this.VisitIntersect(mc.Arguments[0], mc.Arguments[1]);

                case "Except":
                    flag = true;
                    if (mc.Arguments.Count != 2)
                    {
                        break;
                    }
                    return this.VisitExcept(mc.Arguments[0], mc.Arguments[1]);

                case "Any":
                    flag = true;
                    if (mc.Arguments.Count != 1)
                    {
                        if (((mc.Arguments.Count != 2) || !this.IsLambda(mc.Arguments[1])) ||
                            (this.GetLambda(mc.Arguments[1]).Parameters.Count != 1))
                        {
                            break;
                        }
                        return this.VisitQuantifier(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), true);
                    }
                    return this.VisitQuantifier(mc.Arguments[0], null, true);

                case "All":
                    flag = true;
                    if (((mc.Arguments.Count != 2) || !this.IsLambda(mc.Arguments[1])) ||
                        (this.GetLambda(mc.Arguments[1]).Parameters.Count != 1))
                    {
                        break;
                    }
                    return this.VisitQuantifier(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), false);

                case "Count":
                    flag = true;
                    if (mc.Arguments.Count != 1)
                    {
                        if (((mc.Arguments.Count != 2) || !this.IsLambda(mc.Arguments[1])) ||
                            (this.GetLambda(mc.Arguments[1]).Parameters.Count != 1))
                        {
                            break;
                        }
                        return this.VisitAggregate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlNodeType.Count,
                                                   mc.Type);
                    }
                    return this.VisitAggregate(mc.Arguments[0], null, SqlNodeType.Count, mc.Type);

                case "LongCount":
                    flag = true;
                    if (mc.Arguments.Count != 1)
                    {
                        if (((mc.Arguments.Count != 2) || !this.IsLambda(mc.Arguments[1])) ||
                            (this.GetLambda(mc.Arguments[1]).Parameters.Count != 1))
                        {
                            break;
                        }
                        return this.VisitAggregate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]),
                                                   SqlNodeType.LongCount, mc.Type);
                    }
                    return this.VisitAggregate(mc.Arguments[0], null, SqlNodeType.LongCount, mc.Type);

                case "Sum":
                    flag = true;
                    if (mc.Arguments.Count != 1)
                    {
                        if (((mc.Arguments.Count != 2) || !this.IsLambda(mc.Arguments[1])) ||
                            (this.GetLambda(mc.Arguments[1]).Parameters.Count != 1))
                        {
                            break;
                        }
                        return this.VisitAggregate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlNodeType.Sum,
                                                   mc.Type);
                    }
                    return this.VisitAggregate(mc.Arguments[0], null, SqlNodeType.Sum, mc.Type);

                case "Min":
                    flag = true;
                    if (mc.Arguments.Count != 1)
                    {
                        if (((mc.Arguments.Count != 2) || !this.IsLambda(mc.Arguments[1])) ||
                            (this.GetLambda(mc.Arguments[1]).Parameters.Count != 1))
                        {
                            break;
                        }
                        return this.VisitAggregate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlNodeType.Min,
                                                   mc.Type);
                    }
                    return this.VisitAggregate(mc.Arguments[0], null, SqlNodeType.Min, mc.Type);

                case "Max":
                    flag = true;
                    if (mc.Arguments.Count != 1)
                    {
                        if (((mc.Arguments.Count != 2) || !this.IsLambda(mc.Arguments[1])) ||
                            (this.GetLambda(mc.Arguments[1]).Parameters.Count != 1))
                        {
                            break;
                        }
                        return this.VisitAggregate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlNodeType.Max,
                                                   mc.Type);
                    }
                    return this.VisitAggregate(mc.Arguments[0], null, SqlNodeType.Max, mc.Type);

                case "Average":
                    flag = true;
                    if (mc.Arguments.Count != 1)
                    {
                        if (((mc.Arguments.Count != 2) || !this.IsLambda(mc.Arguments[1])) ||
                            (this.GetLambda(mc.Arguments[1]).Parameters.Count != 1))
                        {
                            break;
                        }
                        return this.VisitAggregate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlNodeType.Avg,
                                                   mc.Type);
                    }
                    return this.VisitAggregate(mc.Arguments[0], null, SqlNodeType.Avg, mc.Type);

                case "GroupBy":
                    flag = true;
                    if (((mc.Arguments.Count != 2) || !this.IsLambda(mc.Arguments[1])) ||
                        (this.GetLambda(mc.Arguments[1]).Parameters.Count != 1))
                    {
                        if ((((mc.Arguments.Count == 3) && this.IsLambda(mc.Arguments[1])) &&
                             ((this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) && this.IsLambda(mc.Arguments[2]))) &&
                            (this.GetLambda(mc.Arguments[2]).Parameters.Count == 1))
                        {
                            return this.VisitGroupBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]),
                                                     this.GetLambda(mc.Arguments[2]), null);
                        }
                        if ((((mc.Arguments.Count == 3) && this.IsLambda(mc.Arguments[1])) &&
                             ((this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) && this.IsLambda(mc.Arguments[2]))) &&
                            (this.GetLambda(mc.Arguments[2]).Parameters.Count == 2))
                        {
                            return this.VisitGroupBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), null,
                                                     this.GetLambda(mc.Arguments[2]));
                        }
                        if ((((mc.Arguments.Count == 4) && this.IsLambda(mc.Arguments[1])) &&
                             ((this.GetLambda(mc.Arguments[1]).Parameters.Count == 1) && this.IsLambda(mc.Arguments[2]))) &&
                            (((this.GetLambda(mc.Arguments[2]).Parameters.Count == 1) && this.IsLambda(mc.Arguments[3])) &&
                             (this.GetLambda(mc.Arguments[3]).Parameters.Count == 2)))
                        {
                            return this.VisitGroupBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]),
                                                     this.GetLambda(mc.Arguments[2]), this.GetLambda(mc.Arguments[3]));
                        }
                        break;
                    }
                    return this.VisitGroupBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), null, null);

                case "OrderBy":
                    flag = true;
                    if (((mc.Arguments.Count != 2) || !this.IsLambda(mc.Arguments[1])) ||
                        (this.GetLambda(mc.Arguments[1]).Parameters.Count != 1))
                    {
                        break;
                    }
                    return this.VisitOrderBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlOrderType.Ascending);

                case "OrderByDescending":
                    flag = true;
                    if (((mc.Arguments.Count != 2) || !this.IsLambda(mc.Arguments[1])) ||
                        (this.GetLambda(mc.Arguments[1]).Parameters.Count != 1))
                    {
                        break;
                    }
                    return this.VisitOrderBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlOrderType.Descending);

                case "ThenBy":
                    flag = true;
                    if (((mc.Arguments.Count != 2) || !this.IsLambda(mc.Arguments[1])) ||
                        (this.GetLambda(mc.Arguments[1]).Parameters.Count != 1))
                    {
                        break;
                    }
                    return this.VisitThenBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlOrderType.Ascending);

                case "ThenByDescending":
                    flag = true;
                    if (((mc.Arguments.Count != 2) || !this.IsLambda(mc.Arguments[1])) ||
                        (this.GetLambda(mc.Arguments[1]).Parameters.Count != 1))
                    {
                        break;
                    }
                    return this.VisitThenBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlOrderType.Descending);

                case "Take":
                    flag = true;
                    if (mc.Arguments.Count != 2)
                    {
                        break;
                    }
                    return this.VisitTake(mc.Arguments[0], mc.Arguments[1]);

                case "Skip":
                    flag = true;
                    if (mc.Arguments.Count != 2)
                    {
                        break;
                    }
                    return this.VisitSkip(mc.Arguments[0], mc.Arguments[1]);

                case "Contains":
                    flag = true;
                    if (mc.Arguments.Count != 2)
                    {
                        break;
                    }
                    return this.VisitContains(mc.Arguments[0], mc.Arguments[1]);

                case "ToList":
                case "AsEnumerable":
                case "ToArray":
                    flag = true;
                    if (mc.Arguments.Count != 1)
                    {
                        break;
                    }
                    return this.Visit(mc.Arguments[0]);
            }
            if (flag)
            {
                throw Error.QueryOperatorOverloadNotSupported(mc.Method.Name);
            }
            throw Error.QueryOperatorNotSupported(mc.Method.Name);
        }

        private SqlNode VisitQuantifier(SqlSelect select, LambdaExpression lambda, bool isAny)
        {
            var alias = new SqlAlias(select);
            var ref2 = new SqlAliasRef(alias);
            if (lambda != null)
            {
                map[lambda.Parameters[0]] = ref2;
            }
            SqlExpression cond = (lambda != null) ? VisitExpression(lambda.Body) : null;
            return GenerateQuantifier(alias, cond, isAny);
        }

        private SqlNode VisitQuantifier(Expression sequence, LambdaExpression lambda, bool isAny)
        {
            return VisitQuantifier(VisitSequence(sequence), lambda, isAny);
        }

        private SqlNode VisitUnion(Expression source1, Expression source2)
        {
            SqlSelect left = this.VisitSequence(source1);
            SqlSelect right = this.VisitSequence(source2);
            var node = new SqlUnion(left, right, false);
            var alias = new SqlAlias(node);
            var selection = new SqlAliasRef(alias);
            var select3 = new SqlSelect(selection, alias, dominatingExpression)
            {
                OrderingType = SqlOrderingType.Blocked
            };
            return select3;
        }

        private SqlNode VisitConcat(Expression source1, Expression source2)
        {
            SqlSelect left = this.VisitSequence(source1);
            SqlSelect right = this.VisitSequence(source2);
            SqlUnion node = new SqlUnion(left, right, true);
            SqlAlias alias = new SqlAlias(node);
            SqlAliasRef selection = new SqlAliasRef(alias);
            SqlSelect select3 = new SqlSelect(selection, alias, this.dominatingExpression);
            select3.OrderingType = SqlOrderingType.Blocked;
            return select3;
        }


        private SqlNode VisitDistinct(Expression sequence)
        {
            SqlSelect select = this.LockSelect(this.VisitSequence(sequence));
            select.IsDistinct = true;
            select.OrderingType = SqlOrderingType.Blocked;
            return select;
        }

        protected virtual SqlNode VisitFirst(Expression sequence, LambdaExpression lambda, bool isFirst)
        {
            SqlSelect select = this.LockSelect(this.VisitSequence(sequence));
            if (lambda != null)
            {
                this.map[lambda.Parameters[0]] = select.Selection;
                select.Where = this.VisitExpression(lambda.Body);
            }
            if (isFirst)
            {
                select.Top = this.sql.ValueFromObject(1, false, this.dominatingExpression);
            }
            if (this.outerNode)
            {
                return select;
            }
            SqlNodeType nt = this.typeProvider.From(select.Selection.ClrType).CanBeColumn
                                 ? SqlNodeType.ScalarSubSelect
                                 : SqlNodeType.Element;
            return this.sql.SubSelect(nt, select, sequence.Type);
        }

        protected SqlSelect VisitWhere(Expression sequence, LambdaExpression predicate)
        {
            SqlSelect select = this.LockSelect(this.VisitSequence(sequence));
            this.map[predicate.Parameters[0]] = (SqlAliasRef)select.Selection;
            select.Where = this.VisitExpression(predicate.Body);
            return select;
        }

        protected SqlSelect LockSelect(SqlSelect sel)
        {
            if ((((sel.Selection.NodeType == SqlNodeType.AliasRef) && (sel.Where == null)) &&
                 ((sel.OrderBy.Count <= 0) && (sel.GroupBy.Count <= 0))) &&
                (((sel.Having == null) && (sel.Top == null)) &&
                 ((sel.OrderingType == SqlOrderingType.Default) && !sel.IsDistinct)))
            {
                return sel;
            }
            SqlAlias from = new SqlAlias(sel);
            return new SqlSelect(new SqlAliasRef(from), from, this.dominatingExpression);
        }

        private SqlNode VisitSequenceCast(Expression sequence, Type type)
        {
            Type elementType = TypeSystem.GetElementType(sequence.Type);
            ParameterExpression expression = Expression.Parameter(elementType, "pc");
            return this.Visit(Expression.Call(typeof(Enumerable), "Select", new[] { elementType, type },
                                              new[]
                                                  {
                                                      sequence,
                                                      Expression.Lambda(Expression.Convert(expression, type),
                                                                        new[] {expression})
                                                  }));
        }

        private SqlSelect VisitOfType(Expression sequence, Type ofType)
        {
            SqlSelect sel = this.LockSelect(this.VisitSequence(sequence));
            SqlAliasRef selection = (SqlAliasRef)sel.Selection;
            sel.Selection = new SqlUnary(SqlNodeType.Treat, ofType, this.typeProvider.From(ofType), selection,
                                         this.dominatingExpression);
            sel = this.LockSelect(sel);
            selection = (SqlAliasRef)sel.Selection;
            sel.Where = this.sql.AndAccumulate(sel.Where,
                                               this.sql.Unary(SqlNodeType.IsNotNull, selection,
                                                              this.dominatingExpression));
            return sel;
        }

        private SqlSelect VisitThenBy(Expression sequence, LambdaExpression expression, SqlOrderType orderType)
        {
            if (this.IsGrouping(expression.Body.Type))
            {
                throw Error.GroupingNotSupportedAsOrderCriterion();
            }
            if (!this.typeProvider.From(expression.Body.Type).IsOrderable)
            {
                throw Error.TypeCannotBeOrdered(expression.Body.Type);
            }
            SqlSelect select = this.VisitSequence(sequence);
            this.map[expression.Parameters[0]] = select.Selection;
            SqlExpression expr = this.VisitExpression(expression.Body);
            select.OrderBy.Add(new SqlOrderExpression(orderType, expr));
            return select;
        }

        private SqlNode VisitGroupBy(Expression sequence, LambdaExpression keyLambda, LambdaExpression elemLambda,
                                     LambdaExpression resultSelector)
        {
            SqlSelect sel = this.VisitSequence(sequence);
            sel = this.LockSelect(sel);
            SqlAlias alias = new SqlAlias(sel);
            SqlAliasRef ref2 = new SqlAliasRef(alias);
            this.map[keyLambda.Parameters[0]] = ref2;
            SqlExpression expr = this.VisitExpression(keyLambda.Body);
            SqlDuplicator duplicator = new SqlDuplicator();
            SqlSelect node = (SqlSelect)duplicator.Duplicate(sel);
            SqlAlias alias2 = new SqlAlias(node);
            SqlAliasRef ref3 = new SqlAliasRef(alias2);
            this.map[keyLambda.Parameters[0]] = ref3;
            SqlExpression right = this.VisitExpression(keyLambda.Body);
            SqlExpression selection = null;
            SqlExpression expression4 = null;
            if (elemLambda != null)
            {
                this.map[elemLambda.Parameters[0]] = ref3;
                selection = this.VisitExpression(elemLambda.Body);
                this.map[elemLambda.Parameters[0]] = ref2;
                expression4 = this.VisitExpression(elemLambda.Body);
            }
            else
            {
                selection = ref3;
                expression4 = ref2;
            }
            SqlSharedExpression expression5 = new SqlSharedExpression(expr);
            expr = new SqlSharedExpressionRef(expression5);
            SqlSelect select = new SqlSelect(selection, alias2, this.dominatingExpression);
            select.Where = this.sql.Binary(SqlNodeType.EQ2V, expr, right);
            SqlSubSelect group = this.sql.SubSelect(SqlNodeType.Multiset, select);
            SqlSelect select5 = new SqlSelect(new SqlSharedExpressionRef(expression5), alias, this.dominatingExpression);
            select5.GroupBy.Add(expression5);
            SqlAlias from = new SqlAlias(select5);
            SqlSelect select6 = null;
            if (resultSelector != null)
            {
                Type type = typeof(IGrouping<,>).MakeGenericType(new Type[] { expr.ClrType, selection.ClrType });
                SqlExpression expression6 = new SqlGrouping(type, this.typeProvider.From(type), expr, group,
                                                            this.dominatingExpression);
                SqlSelect select7 = new SqlSelect(expression6, from, this.dominatingExpression);
                SqlAlias alias4 = new SqlAlias(select7);
                SqlAliasRef ref4 = new SqlAliasRef(alias4);
                this.map[resultSelector.Parameters[0]] = this.sql.Member(ref4, type.GetProperty(ConstColumns.Key));
                //type.GetProperty("Key"));
                this.map[resultSelector.Parameters[1]] = ref4;
                var info = new GroupInfo { SelectWithGroup = select5, ElementOnGroupSource = expression4 };
                this.gmap[ref4] = info;
                SqlExpression expression7 = VisitExpression(resultSelector.Body);
                select6 = new SqlSelect(expression7, alias4, this.dominatingExpression);
                var info2 = new GroupInfo { SelectWithGroup = select5, ElementOnGroupSource = expression4 };
                this.gmap[expression7] = info2;
                return select6;
            }
            Type clrType = typeof(IGrouping<,>).MakeGenericType(new Type[] { expr.ClrType, selection.ClrType });
            SqlExpression expression8 = new SqlGrouping(clrType, this.typeProvider.From(clrType), expr, group,
                                                        this.dominatingExpression);
            select6 = new SqlSelect(expression8, from, this.dominatingExpression);
            GroupInfo info3 = new GroupInfo();
            info3.SelectWithGroup = select5;
            info3.ElementOnGroupSource = expression4;
            this.gmap[expression8] = info3;
            return select6;
        }

        private SqlNode VisitOrderBy(Expression sequence, LambdaExpression expression, SqlOrderType orderType)
        {
            if (this.IsGrouping(expression.Body.Type))
            {
                throw Error.GroupingNotSupportedAsOrderCriterion();
            }
            if (!this.typeProvider.From(expression.Body.Type).IsOrderable)
            {
                throw Error.TypeCannotBeOrdered(expression.Body.Type);
            }
            SqlSelect node = this.LockSelect(this.VisitSequence(sequence));
            if ((node.Selection.NodeType != SqlNodeType.AliasRef) || (node.OrderBy.Count > 0))
            {
                SqlAlias alias = new SqlAlias(node);
                SqlAliasRef selection = new SqlAliasRef(alias);
                node = new SqlSelect(selection, alias, this.dominatingExpression);
            }
            this.map[expression.Parameters[0]] = node.Selection;
            SqlExpression expr = this.VisitExpression(expression.Body);
            node.OrderBy.Add(new SqlOrderExpression(orderType, expr));
            return node;
        }

        protected bool IsGrouping(Type t)
        {
            return (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(IGrouping<,>)));
        }

        private SqlNode VisitExcept(Expression source1, Expression source2)
        {
            Type elementType = TypeSystem.GetElementType(source1.Type);
            if (this.IsGrouping(elementType))
            {
                throw Error.ExceptNotSupportedForHierarchicalTypes();
            }
            SqlSelect node = this.LockSelect(this.VisitSequence(source1));
            SqlSelect select2 = this.VisitSequence(source2);
            SqlAlias alias = new SqlAlias(node);
            SqlAliasRef left = new SqlAliasRef(alias);
            SqlAlias alias2 = new SqlAlias(select2);
            SqlAliasRef right = new SqlAliasRef(alias2);
            SqlExpression expression = this.GenerateQuantifier(alias2, this.sql.Binary(SqlNodeType.EQ2V, left, right),
                                                               true);
            SqlSelect select3 = new SqlSelect(left, alias, node.SourceExpression);
            select3.Where = this.sql.Unary(SqlNodeType.Not, expression);
            select3.IsDistinct = true;
            select3.OrderingType = SqlOrderingType.Blocked;
            return select3;
        }

        protected virtual SqlNode VisitTake(Expression sequence, Expression count)
        {
            SqlExpression takeExp = VisitExpression(count);
            if (takeExp.NodeType == SqlNodeType.Value)
            {
                var value2 = (SqlValue)takeExp;
                if (typeof(int).IsAssignableFrom(value2.Value.GetType()) && (((int)value2.Value) < 0))
                {
                    throw Error.ArgumentOutOfRange("takeCount");
                }
            }
            var mc = sequence as MethodCallExpression;
            if (((mc != null) && IsSequenceOperatorCall(mc)) &&
                ((mc.Method.Name == "Skip") && (mc.Arguments.Count == 2)))
            {
                SqlExpression skipExp = this.VisitExpression(mc.Arguments[1]);
                if (skipExp.NodeType == SqlNodeType.Value)
                {
                    var value3 = (SqlValue)skipExp;
                    if (typeof(int).IsAssignableFrom(value3.Value.GetType()) && (((int)value3.Value) < 0))
                    {
                        throw Error.ArgumentOutOfRange("skipCount");
                    }
                }
                SqlSelect select = VisitSequence(mc.Arguments[0]);
                return GenerateSkipTake(select, skipExp, takeExp);
            }
            SqlSelect select2 = VisitSequence(sequence);
            return GenerateSkipTake(select2, null, takeExp);
        }

        protected virtual SqlSelect GenerateSkipTake(SqlSelect sequence, SqlExpression skipExp, SqlExpression takeExp)
        {
            SqlSelect node = this.LockSelect(sequence);
            var value2 = skipExp as SqlValue;
            if ((skipExp == null) || ((value2 != null) && (((int)value2.Value) <= 0)))
            {
                if (takeExp != null)
                {
                    node.Top = takeExp;
                }
                return node;
            }
            var alias = new SqlAlias(node);
            var selection = new SqlAliasRef(alias);
            if (this.UseConverterStrategy(ConverterStrategy.SkipWithRowNumber))
            {
                var col = new SqlColumn("ROW_NUMBER",
                                              this.sql.RowNumber(new List<SqlOrderExpression>(),
                                                                 this.dominatingExpression));
                var expr = new SqlColumnRef(col);
                node.Row.Columns.Add(col);
                SqlSelect select2 = new SqlSelect(selection, alias, this.dominatingExpression);
                if (takeExp != null)
                {
                    select2.Where = this.sql.Between(expr, this.sql.Add(skipExp, 1),
                                                     this.sql.Binary(SqlNodeType.Add,
                                                                     (SqlExpression)SqlDuplicator.Copy(skipExp),
                                                                     takeExp), this.dominatingExpression);
                    return select2;
                }
                select2.Where = this.sql.Binary(SqlNodeType.GT, expr, skipExp);
                return select2;
            }
            if (!this.CanSkipOnSelection(node.Selection))
            {
                throw Error.SkipNotSupportedForSequenceTypes();
            }
            var visitor = new SingleTableQueryVisitor();
            visitor.Visit(node);
            if (!visitor.IsValid)
            {
                throw Error.SkipRequiresSingleTableQueryWithPKs();
            }
            var select3 = (SqlSelect)SqlDuplicator.Copy(node);
            select3.Top = skipExp;
            var alias2 = new SqlAlias(select3);
            var ref4 = new SqlAliasRef(alias2);
            var select = new SqlSelect(ref4, alias2, dominatingExpression)
            {
                Where = this.sql.Binary(SqlNodeType.EQ2V, selection, ref4)
            };
            SqlSubSelect expression = sql.SubSelect(SqlNodeType.Exists, select);
            var select6 = new SqlSelect(selection, alias, dominatingExpression)
            {
                Where = this.sql.Unary(SqlNodeType.Not, expression, this.dominatingExpression),
                Top = takeExp
            };
            return select6;
        }

        public bool CanSkipOnSelection(SqlExpression selection)
        {
            if (!this.IsGrouping(selection.ClrType))
            {
                if (services.Model.GetTable(selection.ClrType) != null)
                {
                    return true;
                }
                if (TypeSystem.IsSequenceType(selection.ClrType) && !selection.SqlType.CanBeColumn)
                {
                    return false;
                }
                switch (selection.NodeType)
                {
                    case SqlNodeType.AliasRef:
                        {
                            SqlNode node = ((SqlAliasRef)selection).Alias.Node;
                            SqlSelect select = node as SqlSelect;
                            if (select != null)
                            {
                                return this.CanSkipOnSelection(select.Selection);
                            }
                            SqlUnion union = node as SqlUnion;
                            if (union != null)
                            {
                                bool flag = false;
                                bool flag2 = false;
                                SqlSelect left = union.Left as SqlSelect;
                                if (left != null)
                                {
                                    flag = this.CanSkipOnSelection(left.Selection);
                                }
                                SqlSelect right = union.Right as SqlSelect;
                                if (right != null)
                                {
                                    flag2 = this.CanSkipOnSelection(right.Selection);
                                }
                                return (flag && flag2);
                            }
                            SqlExpression expression = (SqlExpression)node;
                            return this.CanSkipOnSelection(expression);
                        }
                    case SqlNodeType.New:
                        {
                            SqlNew new2 = (SqlNew)selection;
                            foreach (SqlMemberAssign assign in new2.Members)
                            {
                                if (!this.CanSkipOnSelection(assign.Expression))
                                {
                                    return false;
                                }
                            }
                            if (new2.ArgMembers != null)
                            {
                                int num = 0;
                                int count = new2.ArgMembers.Count;
                                while (num < count)
                                {
                                    if (!this.CanSkipOnSelection(new2.Args[num]))
                                    {
                                        return false;
                                    }
                                    num++;
                                }
                            }
                            break;
                        }
                }
            }
            return true;
        }


        protected bool UseConverterStrategy(ConverterStrategy strategy)
        {
            return ((this.ConverterStrategy & strategy) == strategy);
        }

        private SqlNode VisitIntersect(Expression source1, Expression source2)
        {
            //var args = new object[] { source1, source2 };
            //var result = InvokeMethod(Source, "VisitIntersect", args);
            //return (SqlNode)CreateInstance(result);
            Type elementType = TypeSystem.GetElementType(source1.Type);
            if (this.IsGrouping(elementType))
            {
                throw Error.IntersectNotSupportedForHierarchicalTypes();
            }
            SqlSelect node = this.LockSelect(this.VisitSequence(source1));
            SqlSelect select2 = this.VisitSequence(source2);
            SqlAlias alias = new SqlAlias(node);
            SqlAliasRef left = new SqlAliasRef(alias);
            SqlAlias alias2 = new SqlAlias(select2);
            SqlAliasRef right = new SqlAliasRef(alias2);
            SqlExpression expression = this.GenerateQuantifier(alias2, this.sql.Binary(SqlNodeType.EQ2V, left, right),
                                                               true);
            SqlSelect select3 = new SqlSelect(left, alias, node.SourceExpression);
            select3.Where = expression;
            select3.IsDistinct = true;
            select3.OrderingType = SqlOrderingType.Blocked;
            return select3;
        }

        protected virtual SqlNode VisitSkip(Expression sequence, Expression skipCount)
        {
            SqlExpression skipExp = VisitExpression(skipCount);
            if (skipExp.NodeType == SqlNodeType.Value)
            {
                var value2 = (SqlValue)skipExp;
                if (typeof(int).IsAssignableFrom(value2.Value.GetType()) && (((int)value2.Value) < 0))
                {
                    throw Error.ArgumentOutOfRange("skipCount");
                }
            }
            SqlSelect select = VisitSequence(sequence);
            return GenerateSkipTake(select, skipExp, null);
        }

        protected virtual SqlNode VisitAggregate(Expression sequence, LambdaExpression lambda, SqlNodeType aggType,
                                                 Type returnType)
        {
            bool flag = (aggType == SqlNodeType.Count) || (aggType == SqlNodeType.LongCount);
            SqlNode node = this.Visit(sequence);
            SqlSelect select = CoerceToSequence(node);
            var from = new SqlAlias(select);
            var ref2 = new SqlAliasRef(from);
            var mc = sequence as MethodCallExpression;
            if (((!this.outerNode && !flag) &&
                 ((lambda == null) || ((lambda.Parameters.Count == 1) && (lambda.Parameters[0] == lambda.Body)))) &&
                (((mc != null) && this.IsSequenceOperatorCall(mc, "Select")) && (select.From is SqlAlias)))
            {
                LambdaExpression expression2 = this.GetLambda(mc.Arguments[1]);
                lambda = Expression.Lambda(expression2.Type, expression2.Body, expression2.Parameters);
                from = (SqlAlias)select.From;
                ref2 = new SqlAliasRef(from);
            }

            if ((lambda != null) && !TypeSystem.IsSimpleType(lambda.Body.Type))
            {
                throw Error.CannotAggregateType(lambda.Body.Type);
            }
            /**/
            if ((select.Selection.SqlType.IsRuntimeOnlyType && !this.IsGrouping(sequence.Type)) &&
                (!flag && (lambda == null)))
            {
                throw Error.NonCountAggregateFunctionsAreNotValidOnProjections(aggType);
            }
            if (lambda != null)
            {
                this.map[lambda.Parameters[0]] = ref2;
            }
            if (this.outerNode)
            {
                SqlExpression expression3 = (lambda != null) ? this.VisitExpression(lambda.Body) : null;
                SqlExpression expression4 = null;
                if (flag && (expression3 != null))
                {
                    expression4 = expression3;
                    expression3 = null;
                }
                else if ((expression3 == null) && !flag)
                {
                    expression3 = ref2;
                }
                if (expression3 != null)
                {
                    expression3 = new SqlSimpleExpression(expression3);
                }
                var aggregate1 = GetAggregate(aggType, returnType, expression3);
                var select2 = new SqlSelect(aggregate1, from, dominatingExpression)
                {
                    Where = expression4,
                    OrderingType = SqlOrderingType.Never
                };
                return select2;
            }
            if (!flag || (lambda == null))
            {
                GroupInfo info = this.FindGroupInfo(node);
                if (info != null)
                {
                    SqlExpression elementOnGroupSource = null;
                    if (lambda != null)
                    {
                        this.map[lambda.Parameters[0]] = (SqlExpression)SqlDuplicator.Copy(info.ElementOnGroupSource);
                        elementOnGroupSource = this.VisitExpression(lambda.Body);
                    }
                    else if (!flag)
                    {
                        elementOnGroupSource = info.ElementOnGroupSource;
                    }
                    if (elementOnGroupSource != null)
                    {
                        elementOnGroupSource = new SqlSimpleExpression(elementOnGroupSource);
                    }
                    SqlExpression expression6 = this.GetAggregate(aggType, returnType, elementOnGroupSource);
                    SqlColumn item = new SqlColumn(expression6.ClrType, expression6.SqlType, null, null, expression6,
                                                   this.dominatingExpression);
                    info.SelectWithGroup.Row.Columns.Add(item);
                    return new SqlColumnRef(item);
                }
            }
            SqlExpression expr = (lambda != null) ? this.VisitExpression(lambda.Body) : null;
            if (expr != null)
            {
                expr = new SqlSimpleExpression(expr);
            }
            SqlSelect select3 =
                new SqlSelect(
                    this.GetAggregate(aggType, returnType,
                                      flag ? null : ((lambda == null) ? ((SqlExpression)ref2) : expr)), from,
                    this.dominatingExpression);
            select3.Where = flag ? expr : null;
            return this.sql.SubSelect(SqlNodeType.ScalarSubSelect, select3);
        }

        private SqlNode VisitDefaultIfEmpty(Expression sequence)
        {
            //var args = new object[] { expression };
            //var result = InvokeMethod(Source, "VisitDefaultIfEmpty", args);
            //return (SqlNode)CreateInstance(result);
            SqlAlias alias = new SqlAlias(this.VisitSequence(sequence));
            SqlAliasRef ref2 = new SqlAliasRef(alias);
            SqlExpression selection =
                new SqlOptionalValue(
                    new SqlColumn(ConstColumns.Test,//"test",
                                  this.sql.Unary(SqlNodeType.OuterJoinedValue,
                                                 this.sql.Value(typeof(int?), this.typeProvider.From(typeof(int)), 1,
                                                                false, this.dominatingExpression))),
                    this.sql.Unary(SqlNodeType.OuterJoinedValue, ref2));
            SqlSelect node = new SqlSelect(selection, alias, this.dominatingExpression);
            alias = new SqlAlias(node);
            ref2 = new SqlAliasRef(alias);
            SqlSelect select3 = new SqlSelect(this.sql.TypedLiteralNull(typeof(string), this.dominatingExpression),
                                              null, this.dominatingExpression);
            SqlAlias left = new SqlAlias(select3);
            return new SqlSelect(ref2, new SqlJoin(SqlJoinType.OuterApply, left, alias, null, this.dominatingExpression),
                                 this.dominatingExpression);
        }

        private SqlSelect VisitGroupJoin(Expression outerSequence, Expression innerSequence,
                                         LambdaExpression outerKeySelector, LambdaExpression innerKeySelector,
                                         LambdaExpression resultSelector)
        {
            //var args = new object[] { outerSequence, innerSequence, outerKeySelector, innerKeySelector, resultSelector };
            //var result = InvokeMethod(Source, "VisitGroupJoin", args);
            //return (SqlSelect)CreateInstance(result);
            SqlSelect node = this.VisitSequence(outerSequence);
            SqlSelect select2 = this.VisitSequence(innerSequence);
            SqlAlias alias = new SqlAlias(node);
            SqlAliasRef ref2 = new SqlAliasRef(alias);
            SqlAlias alias2 = new SqlAlias(select2);
            SqlAliasRef selection = new SqlAliasRef(alias2);
            this.map[outerKeySelector.Parameters[0]] = ref2;
            SqlExpression left = this.VisitExpression(outerKeySelector.Body);
            this.map[innerKeySelector.Parameters[0]] = selection;
            SqlExpression right = this.VisitExpression(innerKeySelector.Body);
            SqlExpression expression3 = this.sql.Binary(SqlNodeType.EQ, left, right);
            SqlSelect select = new SqlSelect(selection, alias2, this.dominatingExpression);
            select.Where = expression3;
            SqlSubSelect select4 = this.sql.SubSelect(SqlNodeType.Multiset, select);
            this.map[resultSelector.Parameters[0]] = ref2;
            this.dupMap[resultSelector.Parameters[1]] = select4;
            return new SqlSelect(this.VisitExpression(resultSelector.Body), alias, this.dominatingExpression);
        }

        private SqlNode VisitJoin(Expression outerSequence, Expression innerSequence,
                                  LambdaExpression outerKeySelector, LambdaExpression innerKeySelector,
                                  LambdaExpression resultSelector)
        {
            //var args = new object[] { outerSequence, innerSequence, outerKeySelector, innerKeySelector, resultSelector };
            //var result = InvokeMethod(Source, "VisitJoin", args);
            //return (SqlNode)CreateInstance(result);
            SqlSelect node = this.VisitSequence(outerSequence);
            SqlSelect select2 = this.VisitSequence(innerSequence);
            SqlAlias alias = new SqlAlias(node);
            SqlAliasRef ref2 = new SqlAliasRef(alias);
            SqlAlias alias2 = new SqlAlias(select2);
            SqlAliasRef ref3 = new SqlAliasRef(alias2);
            this.map[outerKeySelector.Parameters[0]] = ref2;
            SqlExpression left = this.VisitExpression(outerKeySelector.Body);
            this.map[innerKeySelector.Parameters[0]] = ref3;
            SqlExpression right = this.VisitExpression(innerKeySelector.Body);
            this.map[resultSelector.Parameters[0]] = ref2;
            this.map[resultSelector.Parameters[1]] = ref3;
            SqlExpression selection = this.VisitExpression(resultSelector.Body);
            SqlExpression cond = this.sql.Binary(SqlNodeType.EQ, left, right);
            SqlSelect select3 = null;
            if ((this.ConverterStrategy & ConverterStrategy.CanUseJoinOn) != ConverterStrategy.Default)
            {
                return new SqlSelect(selection,
                                     new SqlJoin(SqlJoinType.Inner, alias, alias2, cond, this.dominatingExpression),
                                     this.dominatingExpression);
            }
            SqlJoin from = new SqlJoin(SqlJoinType.Cross, alias, alias2, null, this.dominatingExpression);
            select3 = new SqlSelect(selection, from, this.dominatingExpression);
            select3.Where = cond;
            return select3;
        }

        private SqlNode VisitSelectMany(Expression sequence, LambdaExpression colSelector,
                                        LambdaExpression resultSelector)
        {
            //var args = new object[] { expression, lambdaExpression, expression1 };
            //var result = InvokeMethod(Source, "VisitSelectMany", args);
            //return (SqlNode)CreateInstance(result);
            SqlAlias alias = new SqlAlias(this.VisitSequence(sequence));
            SqlAliasRef ref2 = new SqlAliasRef(alias);
            this.map[colSelector.Parameters[0]] = ref2;
            SqlAlias alias2 = new SqlAlias(this.VisitSequence(colSelector.Body));
            SqlAliasRef ref3 = new SqlAliasRef(alias2);
            SqlJoin from = new SqlJoin(SqlJoinType.CrossApply, alias, alias2, null, this.dominatingExpression);
            SqlExpression selection = ref3;
            if (resultSelector != null)
            {
                this.map[resultSelector.Parameters[0]] = ref2;
                this.map[resultSelector.Parameters[1]] = ref3;
                selection = this.VisitExpression(resultSelector.Body);
            }
            return new SqlSelect(selection, from, this.dominatingExpression);
        }

        private SqlNode VisitSelect(Expression sequence, LambdaExpression selector)
        {
            SqlAlias alias = new SqlAlias(this.VisitSequence(sequence));
            var ref2 = new SqlAliasRef(alias);
            map[selector.Parameters[0]] = ref2;
            SqlNode node = Visit(selector.Body);
            var select = node as SqlSelect;
            if (select != null)
            {
                return new SqlSelect(sql.SubSelect(SqlNodeType.Multiset, select, selector.Body.Type), alias,
                                     dominatingExpression);
            }
            if (((node.NodeType == SqlNodeType.Element) || (node.NodeType == SqlNodeType.ScalarSubSelect)) &&
                ((ConverterStrategy & ConverterStrategy.CanUseOuterApply) != ConverterStrategy.Default))
            {
                var select3 = (SqlSubSelect)node;
                var select4 = select3.Select;
                var alias2 = new SqlAlias(select4);
                var ref3 = new SqlAliasRef(alias2);
                if (node.NodeType == SqlNodeType.Element)
                {
                    select4.Selection =
                        new SqlOptionalValue(new SqlColumn(ConstColumns.Test,//"test",
                                                 sql.Unary(SqlNodeType.OuterJoinedValue,
                                                 sql.Value(typeof(int?),
                                                 typeProvider.From(typeof(int)), 1,
                                                 false, dominatingExpression))),
                                             sql.Unary(SqlNodeType.OuterJoinedValue, select4.Selection));
                }
                else
                {
                    select4.Selection = sql.Unary(SqlNodeType.OuterJoinedValue, select4.Selection);
                }
                return new SqlSelect(ref3,
                                     new SqlJoin(SqlJoinType.OuterApply, alias, alias2, null, this.dominatingExpression),
                                     dominatingExpression);
            }
            var selection = node as SqlExpression;
            if (selection == null)
            {
                throw Error.BadProjectionInSelect();
            }
            var result = new SqlSelect(selection, alias, this.dominatingExpression);
            return result;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected LambdaExpression GetLambda(Expression expression)
        {
            return (RemoveQuotes(expression) as LambdaExpression);
        }

        private bool IsLambda(Expression expression)
        {
            return (RemoveQuotes(expression).NodeType == ExpressionType.Lambda);
        }

        [System.Diagnostics.DebuggerStepThrough]
        private static Expression RemoveQuotes(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
            {
                expression = ((UnaryExpression)expression).Operand;
            }
            return expression;
        }

        protected static bool IsSequenceOperatorCall(MethodCallExpression mc)
        {
            Type declaringType = mc.Method.DeclaringType;
            if ((declaringType != typeof(Enumerable)) && (declaringType != typeof(Queryable)))
            {
                return false;
            }
            return true;
        }

        protected bool IsSequenceOperatorCall(MethodCallExpression mc, string methodName)
        {
            return (IsSequenceOperatorCall(mc) && (mc.Method.Name == methodName));
        }

        private SqlNode VisitArrayIndex(BinaryExpression b)
        {
            SqlExpression expression = this.VisitExpression(b.Left);
            SqlExpression expression2 = this.VisitExpression(b.Right);
            if ((expression.NodeType != SqlNodeType.ClientParameter) || (expression2.NodeType != SqlNodeType.Value))
            {
                throw Error.UnrecognizedExpressionNode(b.NodeType);
            }
            SqlClientParameter parameter = (SqlClientParameter)expression;
            SqlValue value2 = (SqlValue)expression2;
            return new SqlClientParameter(b.Type, this.sql.TypeProvider.From(b.Type),
                                          Expression.Lambda(
                                              Expression.ArrayIndex(parameter.Accessor.Body,
                                                                    Expression.Constant(value2.Value, value2.ClrType)),
                                              parameter.Accessor.Parameters.ToArray<ParameterExpression>()),
                                          this.dominatingExpression);
        }

        private SqlNode VisitArrayLength(UnaryExpression c)
        {
            SqlExpression expr = this.VisitExpression(c.Operand);
            if (!expr.SqlType.IsString && !expr.SqlType.IsChar)
            {
                return this.sql.DATALENGTH(expr);
            }
            return this.sql.CLRLENGTH(expr);
        }

        protected virtual SqlNode VisitBinary(BinaryExpression b)
        {
            SqlExpression left = this.VisitExpression(b.Left);
            SqlExpression right = VisitExpression(b.Right);
            if (b.Method != null)
            {
                return this.sql.MethodCall(b.Type, b.Method, null, new[] { left, right }, dominatingExpression);
            }
            switch (b.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return this.sql.Binary(SqlNodeType.Add, left, right, b.Type);

                case ExpressionType.And:
                    if ((b.Left.Type != typeof(bool)) && (b.Left.Type != typeof(bool?)))
                    {
                        return this.sql.Binary(SqlNodeType.BitAnd, left, right, b.Type);
                    }
                    return this.sql.Binary(SqlNodeType.And, left, right, b.Type);

                case ExpressionType.AndAlso:
                    return this.sql.Binary(SqlNodeType.And, left, right, b.Type);

                case ExpressionType.Coalesce:
                    return this.MakeCoalesce(left, right, b.Type);

                case ExpressionType.Divide:
                    return this.sql.Binary(SqlNodeType.Div, left, right, b.Type);

                case ExpressionType.Equal:
                    return this.sql.Binary(SqlNodeType.EQ, left, right, b.Type);

                case ExpressionType.ExclusiveOr:
                    return this.sql.Binary(SqlNodeType.BitXor, left, right, b.Type);

                case ExpressionType.GreaterThan:
                    return this.sql.Binary(SqlNodeType.GT, left, right, b.Type);

                case ExpressionType.GreaterThanOrEqual:
                    return this.sql.Binary(SqlNodeType.GE, left, right, b.Type);

                case ExpressionType.LessThan:
                    return this.sql.Binary(SqlNodeType.LT, left, right, b.Type);

                case ExpressionType.LessThanOrEqual:
                    return this.sql.Binary(SqlNodeType.LE, left, right, b.Type);

                case ExpressionType.Modulo:
                    return this.sql.Binary(SqlNodeType.Mod, left, right, b.Type);

                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return this.sql.Binary(SqlNodeType.Mul, left, right, b.Type);

                case ExpressionType.NotEqual:
                    return this.sql.Binary(SqlNodeType.NE, left, right, b.Type);

                case ExpressionType.Or:
                    if ((b.Left.Type != typeof(bool)) && (b.Left.Type != typeof(bool?)))
                    {
                        return this.sql.Binary(SqlNodeType.BitOr, left, right, b.Type);
                    }
                    return this.sql.Binary(SqlNodeType.Or, left, right, b.Type);

                case ExpressionType.OrElse:
                    return this.sql.Binary(SqlNodeType.Or, left, right, b.Type);

                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return this.sql.Binary(SqlNodeType.Sub, left, right, b.Type);
            }
            throw Error.BinaryOperatorNotRecognized(b.NodeType);
        }

        protected SqlExpression MakeCoalesce(SqlExpression left, SqlExpression right, Type resultType)
        {
            //throw new NotImplementedException();
            return sql.MakeCoalesce(left, right, resultType, dominatingExpression);
        }

        private static Expression ChooseBestDominatingExpression(Expression last, Expression next)
        {
            if (last != null)
            {
                if (next == null)
                {
                    return last;
                }
                if (next is MethodCallExpression)
                {
                    return next;
                }
                if (last is MethodCallExpression)
                {
                    return last;
                }
            }
            return next;
        }

        protected virtual ConversionMethod ChooseConversionMethod(Type fromType, Type toType)
        {
            Type nonNullableType = TypeSystem.GetNonNullableType(fromType);
            Type seqType = TypeSystem.GetNonNullableType(toType);
            if ((fromType != toType) && (nonNullableType == seqType))
            {
                return ConversionMethod.Lift;
            }
            if (!TypeSystem.IsSequenceType(nonNullableType) && !TypeSystem.IsSequenceType(seqType))
            {
                IProviderType type3 = this.typeProvider.From(nonNullableType);
                IProviderType type4 = this.typeProvider.From(seqType);
                bool isRuntimeOnlyType = type3.IsRuntimeOnlyType;
                bool flag2 = type4.IsRuntimeOnlyType;
                if (isRuntimeOnlyType || flag2)
                {
                    return ConversionMethod.Treat;
                }
                if (((nonNullableType != seqType) && (!type3.IsString || !type3.Equals(type4))) &&
                    (!nonNullableType.IsEnum && !seqType.IsEnum))
                {
                    return ConversionMethod.Convert;
                }
            }
            return ConversionMethod.Ignore;
        }

        protected SqlSelect VisitSequence(Expression exp)
        {
            return CoerceToSequence(Visit(exp));
        }

        protected SqlNode Visit(Expression exp)
        {
            bool tmpOuterNode = this.outerNode;
            this.outerNode = false;
            SqlNode node2 = VisitInner(exp);
            this.outerNode = tmpOuterNode;
            return node2;
        }

        protected SqlSelect CoerceToSequence(SqlNode node)
        {
            var select = node as SqlSelect;
            if (select != null)
            {
                return select;
            }
            if (node.NodeType == SqlNodeType.Value)
            {
                var value2 = (SqlValue)node;
                var table = value2.Value as ITable;
                if (table != null)
                {
                    return CoerceToSequence(TranslateConstantTable(table, null));
                }
                var queryable = value2.Value as IQueryable;
                if (queryable == null)
                {
                    throw Error.CapturedValuesCannotBeSequences();
                }
                Expression exp = Funcletizer.Funcletize(queryable.Expression);
                return VisitSequence(exp);
            }
            if ((node.NodeType == SqlNodeType.Multiset) || (node.NodeType == SqlNodeType.Element))
            {
                return ((SqlSubSelect)node).Select;
            }
            if (node.NodeType == SqlNodeType.ClientArray)
            {
                throw Error.ConstructedArraysNotSupported();
            }
            if (node.NodeType == SqlNodeType.ClientParameter)
            {
                throw Error.ParametersCannotBeSequences();
            }
            SqlExpression expression2 = (SqlExpression)node;
            SqlAlias from = new SqlAlias(expression2);
            return new SqlSelect(new SqlAliasRef(from), from, dominatingExpression);
        }

        private Dictionary<Type, SqlTable> sqlTables = new Dictionary<Type, SqlTable>();
        private SqlNode TranslateConstantTable(ITable table, SqlLink link)
        {
            if (table.Context != this.Services.Context)
            {
                throw Error.WrongDataContext();
            }
            MetaTable table2 = Services.Model.GetTable(table.ElementType);
            var sqlSelect = this.translator.BuildDefaultQuery(table2.RowType, this.allowDeferred, link, this.dominatingExpression);
            if (sqlSelect.From is SqlAlias && ((SqlAlias)(sqlSelect.From)).Node is SqlTable)
            {
                sqlTables[table.ElementType] = (SqlTable)((SqlAlias)(sqlSelect.From)).Node;
            }
            return sqlSelect;
        }

        internal Dictionary<Type, SqlTable> SqlTables
        {
            get { return this.sqlTables; }
        }

        private static bool IsLegalIdentityType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                    return true;
            }
            return false;
        }

        internal SqlNode ConvertInner(Expression node, Expression dominantExpression)
        {
            this.dominatingExpression = dominantExpression;
            bool outerNode = this.outerNode;
            this.outerNode = false;
            SqlNode node2 = this.VisitInner(node);
            this.outerNode = outerNode;
            return node2;
        }





    }
}