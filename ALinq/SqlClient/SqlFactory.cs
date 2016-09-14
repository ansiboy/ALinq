using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using ALinq.Mapping;

namespace ALinq.SqlClient
{
    internal class SqlFactory
    {
        // Fields
        private readonly MetaModel model;
        private readonly ITypeSystemProvider typeProvider;

        // Methods
        internal SqlFactory(ITypeSystemProvider typeProvider, MetaModel model)
        {
            this.typeProvider = typeProvider;
            this.model = model;
        }

        internal SqlExpression Add(params SqlExpression[] expressions)
        {
            SqlExpression right = expressions[expressions.Length - 1];
            for (int i = expressions.Length - 2; i >= 0; i--)
            {
                right = this.Binary(SqlNodeType.Add, expressions[i], right);
            }
            return right;
        }

        internal SqlExpression Add(SqlExpression expr, int second)
        {
            return this.Binary(SqlNodeType.Add, expr, this.ValueFromObject(second, false, expr.SourceExpression));
        }

        internal SqlExpression AndAccumulate(SqlExpression left, SqlExpression right)
        {
            if (left == null)
            {
                return right;
            }
            if (right == null)
            {
                return left;
            }
            return this.Binary(SqlNodeType.And, left, right);
        }

        internal SqlBetween Between(SqlExpression expr, SqlExpression start, SqlExpression end, Expression source)
        {
            return new SqlBetween(typeof(bool), this.typeProvider.From(typeof(bool)), expr, start, end, source);
        }

        internal SqlBinary Binary(SqlNodeType nodeType, SqlExpression left, SqlExpression right)
        {
            return this.Binary(nodeType, left, right, null, null);
        }

        internal SqlBinary Binary(SqlNodeType nodeType, SqlExpression left, SqlExpression right, MethodInfo method)
        {
            return this.Binary(nodeType, left, right, method, null);
        }

        internal SqlBinary Binary(SqlNodeType nodeType, SqlExpression left, SqlExpression right, Type clrType)
        {
            return this.Binary(nodeType, left, right, null, clrType);
        }

        internal SqlBinary Binary(SqlNodeType nodeType, SqlExpression left, SqlExpression right, MethodInfo method, Type clrType)
        {
            IProviderType sqlType = null;
            if (nodeType.IsPredicateBinaryOperator())
            {
                if (clrType == null)
                {
                    clrType = typeof(bool);
                }
                sqlType = this.typeProvider.From(clrType);
            }
            else
            {
                IProviderType type2 = this.typeProvider.PredictTypeForBinary(nodeType, left.SqlType, right.SqlType);
                if (type2 == right.SqlType)
                {
                    if (clrType == null)
                    {
                        clrType = right.ClrType;
                    }
                    sqlType = right.SqlType;
                }
                else if (type2 == left.SqlType)
                {
                    if (clrType == null)
                    {
                        clrType = left.ClrType;
                    }
                    sqlType = left.SqlType;
                }
                else
                {
                    sqlType = type2;
                    if (clrType == null)
                    {
                        clrType = type2.GetClosestRuntimeType();
                    }
                }
            }
            return new SqlBinary(nodeType, clrType, sqlType, left, right, method);
        }

        internal SqlExpression Case(Type clrType, SqlExpression discriminator, List<SqlExpression> matches, List<SqlExpression> values, Expression sourceExpression)
        {
            if (values.Count == 0)
            {
                throw Error.EmptyCaseNotSupported();
            }
            bool flag = false;
            foreach (var expression in values)
            {
                flag |= expression.IsClientAidedExpression();
            }
            if (flag)
            {
                var list = new List<SqlClientWhen>();
                int num = 0;
                int num2 = matches.Count;
                while (num < num2)
                {
                    list.Add(new SqlClientWhen(matches[num], values[num]));
                    num++;
                }
                return new SqlClientCase(clrType, discriminator, list, sourceExpression);
            }
            var whens = new List<SqlWhen>();
            int num3 = 0;
            int count = matches.Count;
            while (num3 < count)
            {
                whens.Add(new SqlWhen(matches[num3], values[num3]));
                num3++;
            }
            return new SqlSimpleCase(clrType, discriminator, whens, sourceExpression);
        }

        internal SqlExpression CastTo(Type clrType, SqlExpression expr)
        {
            return this.UnaryCast(clrType, this.typeProvider.From(clrType), expr, expr.SourceExpression);
        }

        internal SqlExpression CLRLENGTH(SqlExpression expr)
        {
            return Unary(SqlNodeType.ClrLength, expr);
        }

        internal virtual SqlExpression Concat(SqlExpression[] expressions, Expression sourceExpression)
        {
            SqlExpression right = expressions[expressions.Length - 1];
            Debug.Assert(right.SqlType.IsString || right.SqlType.IsChar);

            for (int i = expressions.Length - 2; i >= 0; i--)
            {
                Debug.Assert(expressions[i].SqlType.IsString || expressions[i].SqlType.IsChar);
                right = this.Binary(SqlNodeType.Concat, expressions[i], right);
            }
            return right;
        }

        internal SqlExpression ConvertTo(Type clrType, SqlExpression expr)
        {
            return UnaryConvert(clrType, typeProvider.From(clrType), expr, expr.SourceExpression);
        }

        internal SqlExpression ConvertTo(Type clrType, IProviderType sqlType, SqlExpression expr)
        {
            return UnaryConvert(clrType, sqlType, expr, expr.SourceExpression);
        }

        internal SqlExpression ConvertToBigint(SqlExpression expr)
        {
            return this.ConvertTo(typeof(long), expr);
        }

        internal SqlExpression ConvertToBool(SqlExpression expr)
        {
            return this.ConvertTo(typeof(bool), expr);
        }

        internal SqlExpression ConvertToDouble(SqlExpression expr)
        {
            return this.ConvertTo(typeof(double), expr);
        }

        internal SqlExpression ConvertToInt(SqlExpression expr)
        {
            return this.ConvertTo(typeof(int), expr);
        }

        internal virtual SqlExpression DATALENGTH(SqlExpression expr)
        {
            return FunctionCall(typeof(int), "DATALENGTH", new[] { expr }, expr.SourceExpression);
        }

        internal SqlExpression DATEADD(string partName, SqlExpression value, SqlExpression expr)
        {
            return DATEADD(partName, value, expr, expr.SourceExpression, false);
        }

        internal SqlExpression DATEADD(string partName, SqlExpression value, SqlExpression expr, Expression sourceExpression, bool asNullable)
        {
            Type clrType = asNullable ? typeof(DateTime?) : typeof(DateTime);
            return this.FunctionCall(clrType, "DATEADD", new[] { new SqlVariable(typeof(void), null, partName, sourceExpression), value, expr }, sourceExpression);
        }

        internal virtual SqlExpression DATEPART(string partName, SqlExpression expr)
        {
            return FunctionCall(typeof(int), "DATEPART",
                new[] { new SqlVariable(typeof(void), null, partName, expr.SourceExpression), expr },
                        expr.SourceExpression);
        }

        internal IProviderType Default(MetaDataMember member)
        {
            if (member == null)
            {
                throw Error.ArgumentNull("member");
            }
            if (!string.IsNullOrEmpty(member.DbType))
            {
                return this.typeProvider.Parse(member.DbType);
            }
            return this.typeProvider.From(member.Type);
        }

        [System.Diagnostics.DebuggerStepThrough]
        internal IProviderType Default(Type clrType)
        {
            return this.typeProvider.From(clrType);
        }

        internal SqlExpression DiscriminatedType(SqlExpression discriminator, MetaType targetType)
        {
            return new SqlDiscriminatedType(this.typeProvider.From(typeof(Type)), discriminator, targetType, discriminator.SourceExpression);
        }

        internal SqlExpression Divide(SqlExpression first, SqlExpression second)
        {
            return this.Binary(SqlNodeType.Div, first, second);
        }

        internal SqlExpression Divide(SqlExpression expr, long second)
        {
            return this.Binary(SqlNodeType.Div, expr, this.ValueFromObject(second, false, expr.SourceExpression));
        }

        internal SqlDoNotVisitExpression DoNotVisitExpression(SqlExpression expr)
        {
            return new SqlDoNotVisitExpression(expr);
        }

        internal SqlExprSet ExprSet(SqlExpression[] exprs, Expression sourceExpression)
        {
            return new SqlExprSet(exprs[0].ClrType, exprs, sourceExpression);
        }

        internal SqlFunctionCall FunctionCall(Type clrType, string name, IEnumerable<SqlExpression> args, Expression source)
        {
            return new SqlFunctionCall(clrType, Default(clrType), name, args, source);
        }

        internal SqlFunctionCall FunctionCall(Type clrType, IProviderType sqlType, string name, IEnumerable<SqlExpression> args, Expression source)
        {
            return new SqlFunctionCall(clrType, sqlType, name, args, source);
        }

        internal SqlIn In(SqlExpression expr, IEnumerable<SqlExpression> values, Expression source)
        {
            return new SqlIn(typeof(bool), this.typeProvider.From(typeof(bool)), expr, values, source);
        }

        internal virtual SqlExpression String_Length(SqlExpression expr)
        {
            return this.FunctionCall(typeof(int), "LEN", new[] { expr }, expr.SourceExpression);
        }

        internal SqlLike Like(SqlExpression expr, SqlExpression pattern, SqlExpression escape, Expression source)
        {
            return new SqlLike(typeof(bool), this.typeProvider.From(typeof(bool)), expr, pattern, escape, source);
        }

        internal SqlMember Member(SqlExpression expr, MetaDataMember member)
        {
            return new SqlMember(member.Type, this.Default(member), expr, member.Member);
        }

        internal SqlMember Member(SqlExpression expr, MemberInfo member)
        {
            //if (member is IndexMemberInfo)
            //{
            //    Type memberType = TypeSystem.GetMemberType(member);
            //    MetaType metaType = this.model.GetMetaType(member.DeclaringType);
            //    //MetaDataMember dataMember = metaType.GetDataMember(member);
            //    var dataMember = new IndexPropertyDataMember(metaType, (IndexMemberInfo) member);
            //    //if ((metaType == null))
            //    //{
            //    //    return new MySqlMember(memberType, this.Default(memberType), expr, member, dataMember);
            //    //}
            //    return new MySqlMember(memberType, this.Default(dataMember), expr, member, dataMember);
            //}
            //else
            //{
            Type memberType = TypeSystem.GetMemberType(member);
            MetaType metaType = this.model.GetMetaType(member.DeclaringType);
            MetaDataMember dataMember = metaType.GetDataMember(member);

            if (dataMember != null)
            {
                return new SqlMember(memberType, this.Default(dataMember), expr, member);
            }
            return new SqlMember(memberType, this.Default(memberType), expr, member);
            //}

        }

        [System.Diagnostics.DebuggerStepThrough]
        internal SqlMethodCall MethodCall(MethodInfo method, SqlExpression obj, SqlExpression[] args, Expression sourceExpression)
        {
            return new SqlMethodCall(method.ReturnType, this.Default(method.ReturnType), method, obj, args, sourceExpression);
        }

        internal SqlMethodCall MethodCall(Type returnType, MethodInfo method, SqlExpression obj, SqlExpression[] args, Expression sourceExpression)
        {
            return new SqlMethodCall(returnType, this.Default(returnType), method, obj, args, sourceExpression);
        }

        internal SqlExpression Mod(SqlExpression expr, long second)
        {
            return this.Binary(SqlNodeType.Mod, expr, this.ValueFromObject(second, false, expr.SourceExpression));
        }

        internal SqlExpression Multiply(params SqlExpression[] expressions)
        {
            SqlExpression right = expressions[expressions.Length - 1];
            for (int i = expressions.Length - 2; i >= 0; i--)
            {
                right = this.Binary(SqlNodeType.Mul, expressions[i], right);
            }
            return right;
        }

        internal SqlExpression Multiply(SqlExpression expr, long second)
        {
            return this.Binary(SqlNodeType.Mul, expr, this.ValueFromObject(second, false, expr.SourceExpression));
        }

        internal SqlNew New(MetaType type, ConstructorInfo cons, IEnumerable<SqlExpression> args, IEnumerable<MemberInfo> argMembers, IEnumerable<SqlMemberAssign> bindings, Expression sourceExpression)
        {
            return new SqlNew(type, this.typeProvider.From(type.Type), cons, args, argMembers, bindings, sourceExpression);
        }

        internal SqlExpression ObjectType(SqlExpression obj, Expression sourceExpression)
        {
            return new SqlObjectType(obj, this.typeProvider.From(typeof(Type)), sourceExpression);
        }

        internal SqlExpression OrAccumulate(SqlExpression left, SqlExpression right)
        {
            if (left == null)
            {
                return right;
            }
            if (right == null)
            {
                return left;
            }
            return this.Binary(SqlNodeType.Or, left, right);
        }

        internal SqlExpression Parameter(object value, Expression source)
        {
            Type clrType = value.GetType();
            return this.Value(clrType, this.typeProvider.From(value), value, true, source);
        }

        internal SqlRowNumber RowNumber(List<SqlOrderExpression> orderBy, Expression sourceExpression)
        {
            return new SqlRowNumber(typeof(long), this.typeProvider.From(typeof(long)), orderBy, sourceExpression);
        }

        internal virtual SqlSearchedCase SearchedCase(SqlWhen[] whens, SqlExpression @else, Expression sourceExpression)
        {
            return new SqlSearchedCase(whens[0].Value.ClrType, whens, @else, sourceExpression);
        }

        internal SqlExpression StaticType(MetaType typeOf, Expression sourceExpression)
        {
            if (typeOf == null)
            {
                throw Error.ArgumentNull("typeOf");
            }
            if (typeOf.InheritanceCode == null)
            {
                return new SqlValue(typeof(Type), this.typeProvider.From(typeof(Type)), typeOf.Type, false, sourceExpression);
            }
            Type clrType = typeOf.InheritanceCode.GetType();
            SqlValue discriminator = new SqlValue(clrType, this.typeProvider.From(clrType), typeOf.InheritanceCode, true, sourceExpression);
            return this.DiscriminatedType(discriminator, typeOf);
        }

        internal SqlSubSelect SubSelect(SqlNodeType nt, SqlSelect select)
        {
            return this.SubSelect(nt, select, null);
        }

        internal SqlSubSelect SubSelect(SqlNodeType nt, SqlSelect select, Type clrType)
        {
            IProviderType sqlType = null;
            SqlNodeType type2 = nt;
            if (type2 <= SqlNodeType.Exists)
            {
                switch (type2)
                {
                    case SqlNodeType.Element:
                        goto Label_0022;

                    case SqlNodeType.Exists:
                        clrType = typeof(bool);
                        sqlType = this.typeProvider.From(typeof(bool));
                        goto Label_0098;
                }
                goto Label_0098;
            }
            if (type2 == SqlNodeType.Multiset)
            {
                if (clrType == null)
                {
                    clrType = typeof(List<>).MakeGenericType(new Type[] { select.Selection.ClrType });
                }
                sqlType = this.typeProvider.GetApplicationType(1);
                goto Label_0098;
            }
            if (type2 != SqlNodeType.ScalarSubSelect)
            {
                goto Label_0098;
            }
        Label_0022:
            clrType = select.Selection.ClrType;
            sqlType = select.Selection.SqlType;
        Label_0098:
            return new SqlSubSelect(nt, clrType, sqlType, select);
        }

        internal SqlExpression Subtract(SqlExpression first, SqlExpression second)
        {
            return this.Binary(SqlNodeType.Sub, first, second);
        }

        internal SqlExpression Subtract(SqlExpression expr, int second)
        {
            return this.Binary(SqlNodeType.Sub, expr, this.ValueFromObject(second, false, expr.SourceExpression));
        }

        internal SqlTable Table(MetaTable table, MetaType rowType, Expression sourceExpression)
        {
            return new SqlTable(table, rowType, this.typeProvider.GetApplicationType(0), sourceExpression);
        }

        internal SqlTableValuedFunctionCall TableValuedFunctionCall(MetaType rowType, Type clrType, string name, IEnumerable<SqlExpression> args, Expression source)
        {
            return new SqlTableValuedFunctionCall(rowType, clrType, this.Default(clrType), name, args, source);
        }

        internal SqlExpression TypeCase(Type clrType, MetaType rowType, SqlExpression discriminator, IEnumerable<SqlTypeCaseWhen> whens, Expression sourceExpression)
        {
            return new SqlTypeCase(clrType, this.typeProvider.From(clrType), rowType, discriminator, whens, sourceExpression);
        }

        public SqlExpression TypedLiteralNull(Type type, Expression sourceExpression)
        {
            return this.ValueFromObject(null, type, false, sourceExpression);
        }

        internal SqlUnary Unary(SqlNodeType nodeType, SqlExpression expression)
        {
            return this.Unary(nodeType, expression, expression.SourceExpression);
        }

        internal SqlUnary Unary(SqlNodeType nodeType, SqlExpression expression, Expression sourceExpression)
        {
            return this.Unary(nodeType, expression, null, sourceExpression);
        }

        internal SqlUnary Unary(SqlNodeType nodeType, SqlExpression expression, MethodInfo method, Expression sourceExpression)
        {
            Type clrType = null;
            IProviderType sqlType = null;
            if (nodeType == SqlNodeType.Count)
            {
                clrType = typeof(int);
                sqlType = this.typeProvider.From(typeof(int));
            }
            else if (nodeType == SqlNodeType.LongCount)
            {
                clrType = typeof(long);
                sqlType = this.typeProvider.From(typeof(long));
            }
            else if (nodeType == SqlNodeType.ClrLength)
            {
                clrType = typeof(int);
                sqlType = this.typeProvider.From(typeof(int));
            }
            else
            {
                if (nodeType.IsPredicateUnaryOperator())
                {
                    clrType = typeof(bool);
                }
                else
                {
                    clrType = expression.ClrType;
                }
                sqlType = this.typeProvider.PredictTypeForUnary(nodeType, expression.SqlType);
            }
            return new SqlUnary(nodeType, clrType, sqlType, expression, method, sourceExpression);
        }

        internal SqlUnary UnaryCast(Type targetClrType, IProviderType targetSqlType, SqlExpression expression, Expression sourceExpression)
        {
            return new SqlUnary(SqlNodeType.Cast, targetClrType, targetSqlType, expression, null, sourceExpression);
        }

        internal static SqlUnary UnaryConvert(Type targetClrType, IProviderType targetSqlType, SqlExpression expression, Expression sourceExpression)
        {
            return new SqlUnary(SqlNodeType.Convert, targetClrType, targetSqlType, expression, null, sourceExpression);
        }

        internal SqlUnary UnaryValueOf(SqlExpression expression, Expression sourceExpression)
        {
            return new SqlUnary(SqlNodeType.ValueOf, TypeSystem.GetNonNullableType(expression.ClrType), expression.SqlType, expression, null, sourceExpression);
        }

        internal SqlExpression Value(Type clrType, IProviderType sqlType, object value, bool isClientSpecified, Expression sourceExpression)
        {
            if (typeof(Type).IsAssignableFrom(clrType))
            {
                MetaType metaType = this.model.GetMetaType((Type)value);
                return this.StaticType(metaType, sourceExpression);
            }
            return new SqlValue(clrType, sqlType, value, isClientSpecified, sourceExpression);
        }

        internal SqlExpression ValueFromObject(object value, Expression sourceExpression)
        {
            return this.ValueFromObject(value, false, sourceExpression);
        }

        internal SqlExpression ValueFromObject(object value, bool isClientSpecified, Expression sourceExpression)
        {
            if (value == null)
            {
                throw Error.ArgumentNull("value");
            }
            Type clrType = value.GetType();
            return this.ValueFromObject(value, clrType, isClientSpecified, sourceExpression);
        }

        internal SqlExpression ValueFromObject(object value, Type clrType, bool isClientSpecified, Expression sourceExpression)
        {
            if (clrType == null)
            {
                throw Error.ArgumentNull("clrType");
            }
            IProviderType sqlType;
            if (value == null || clrType.IsValueType == false)
                sqlType = this.typeProvider.From(clrType);
            else
                sqlType = this.typeProvider.From(value);
            return this.Value(clrType, sqlType, value, isClientSpecified, sourceExpression);
        }

        // Properties
        internal ITypeSystemProvider TypeProvider
        {
            get
            {
                return this.typeProvider;
            }
        }

        private SqlExpression CreateDateTimeFromDateAndMs(SqlExpression sqlDate, SqlExpression ms, Expression source, bool asNullable)
        {
            SqlExpression expr = ConvertToBigint(ms);
            SqlExpression expression2 = DATEADD("day", Divide(expr, (long)0x5265c00L), sqlDate, source, asNullable);
            return DATEADD("ms", Mod(expr, 0x5265c00L), expression2, source, asNullable);
        }

        private SqlExpression CreateDateTimeFromDateAndMs(SqlExpression sqlDate, SqlExpression ms, Expression source)
        {
            return CreateDateTimeFromDateAndMs(sqlDate, ms, source, false);
        }

        internal virtual SqlExpression DateTime_AddDays(SqlMethodCall mc)
        {
            SqlExpression expression6 = Multiply(mc.Arguments[0], 0x5265c00L);
            var expression = this.CreateDateTimeFromDateAndMs(mc.Object, expression6, mc.SourceExpression, false);
            return expression;
        }

        internal virtual SqlExpression DateTime_AddHours(SqlMethodCall mc)
        {
            SqlExpression expression5 = Multiply(mc.Arguments[0], 0x36ee80L);
            return CreateDateTimeFromDateAndMs(mc.Object, expression5, mc.SourceExpression);
        }

        internal virtual SqlExpression DateTime_AddMinutes(SqlMethodCall mc)
        {
            SqlExpression expression4 = Multiply(mc.Arguments[0], 0xea60L);
            return this.CreateDateTimeFromDateAndMs(mc.Object, expression4, mc.SourceExpression);
        }

        internal virtual SqlExpression DateTime_AddSeconds(SqlMethodCall mc)
        {
            SqlExpression ms = Multiply(mc.Arguments[0], 0x3e8L);
            return this.CreateDateTimeFromDateAndMs(mc.Object, ms, mc.SourceExpression);
        }

        internal virtual SqlExpression DateTime_AddYears(SqlMethodCall mc)
        {
            return DATEADD("YEAR", mc.Arguments[0], mc.Object);
        }

        internal virtual SqlExpression DateTime_AddMonths(SqlMethodCall mc)
        {
            return DATEADD("MONTH", mc.Arguments[0], mc.Object); ;
        }

        internal virtual SqlExpression DateTime_ToString(SqlMethodCall mc)
        {
            throw new NotImplementedException();
        }

        internal virtual SqlExpression DateTime_Add(SqlMethodCall mc)
        {
            return CreateDateTimeFromDateAndTicks(mc.Object, mc.Arguments[0], mc.SourceExpression);
        }

        private SqlExpression CreateDateTimeFromDateAndTicks(SqlExpression sqlDate, SqlExpression sqlTicks, Expression source)
        {
            return this.CreateDateTimeFromDateAndTicks(sqlDate, sqlTicks, source, false);
        }

        private SqlExpression CreateDateTimeFromDateAndTicks(SqlExpression sqlDate, SqlExpression sqlTicks, Expression source, bool asNullable)
        {
            SqlExpression expr = DATEADD("day", Divide(sqlTicks, (long)0xc92a69c000L), sqlDate, source, asNullable);
            return DATEADD("ms", Mod(Divide(sqlTicks, (long)0x2710L), 0x5265c00L), expr, source, asNullable);
        }

        internal virtual SqlExpression Math_Truncate(SqlMethodCall mc)
        {
            var args = new[] { mc.Arguments[0], ValueFromObject(0, false, mc.SourceExpression),
                               ValueFromObject(1, false, mc.SourceExpression) };
            return FunctionCall(mc.Method.ReturnType, "ROUND", args, mc.SourceExpression);

        }

        internal virtual SqlExpression Math_Round(SqlMethodCall mc)
        {
            var expr = mc.Arguments[0];
            Type clrType = expr.ClrType;
            var sourceExpression = mc.SourceExpression;
            //if (mc.Arguments.Count == 1)
            //    return FunctionCall(clrType, "round", new[] { expr }, sourceExpression);

            int count = mc.Arguments.Count;
            if (mc.Arguments[count - 1].ClrType != typeof(MidpointRounding))
            {
                throw Error.MathRoundNotSupported();
            }

            SqlExpression expression15;
            if (count == 2)
            {
                expression15 = ValueFromObject(0, false, sourceExpression);
            }
            else
            {
                expression15 = mc.Arguments[1];
            }
            SqlExpression expression16 = mc.Arguments[count - 1];
            if (expression16.NodeType != SqlNodeType.Value)
            {
                throw Error.NonConstantExpressionsNotSupportedForRounding();
            }
            if (((MidpointRounding)SqlVisitor.Eval(expression16)) == MidpointRounding.AwayFromZero)
            {
                return FunctionCall(expr.ClrType, "round", new[] { expr, expression15 }, sourceExpression);
            }

            SqlExpression expression17 = this.FunctionCall(clrType, "round", new[] { expr, expression15 }, sourceExpression);
            SqlExpression expression18 = this.Multiply(expr, 2L);
            SqlExpression expression19 = this.FunctionCall(clrType, "round", new[] { expression18, expression15 }, sourceExpression);
            SqlExpression expression20 = this.AndAccumulate(this.Binary(SqlNodeType.EQ, expression18, expression19),
                                                           this.Binary(SqlNodeType.NE, expr, expression17));
            SqlExpression expression21 = this.Multiply(this.FunctionCall(clrType, "round", new[] { this.Divide(expr, 2L), expression15 },
                                                      sourceExpression), 2L);
            return this.SearchedCase(new[] { new SqlWhen(expression20, expression21) }, expression17, sourceExpression);
        }

        internal virtual SqlExpression String_Substring(SqlMethodCall mc)
        {
            SqlExpression[] args;

            if (mc.Arguments.Count != 1)
            {
                if (mc.Arguments.Count == 2)
                {
                    args = new[] { mc.Object, Add(mc.Arguments[0], 1), mc.Arguments[1] };
                    return FunctionCall(typeof(string), "SUBSTRING", args, mc.SourceExpression);
                }
                throw Error.MethodHasNoSupportConversionToSql(mc.Method);//GetMethodSupportException(mc);
            }
            args = new[] { mc.Object, Add(mc.Arguments[0], 1), CLRLENGTH(mc.Object) };
            return FunctionCall(typeof(string), "SUBSTRING", args, mc.SourceExpression);
        }

        internal virtual SqlExpression String_Insert(SqlMethodCall mc)
        {
            return null;
        }

        internal virtual SqlExpression Math_Max(SqlMethodCall mc)
        {
            SqlExpression left = mc.Arguments[0];
            SqlExpression right = mc.Arguments[1];
            SqlExpression match = Binary(SqlNodeType.LT, left, right);
            return new SqlSearchedCase(mc.Method.ReturnType, new[] { new SqlWhen(match, right) }, left, mc.SourceExpression);
        }

        internal virtual SqlExpression Math_Min(SqlMethodCall mc)
        {
            SqlExpression expression11 = mc.Arguments[0];
            SqlExpression expression12 = mc.Arguments[1];
            SqlExpression expression13 = this.Binary(SqlNodeType.LT, expression11, expression12);
            return this.SearchedCase(new[] { new SqlWhen(expression13, expression11) }, expression12, mc.SourceExpression);

        }

        internal virtual SqlExpression String_Trim(SqlMethodCall mc)
        {
            var args = new[] { FunctionCall(typeof(string), "RTRIM", new[] { mc.Object }, mc.SourceExpression) };
            return FunctionCall(typeof(string), "LTRIM", args, mc.SourceExpression);
        }

        internal virtual SqlExpression String_TrimEnd(SqlMethodCall mc)
        {
            throw Error.MethodHasNoSupportConversionToSql(mc);
        }

        internal virtual SqlExpression String_TrimStart(SqlMethodCall mc)
        {
            return FunctionCall(typeof(string), "LTRIM", new[] { mc.Object }, mc.SourceExpression);
        }

        internal SqlVariable VariableFromName(string name, Expression sourceExpression)
        {
            return new SqlVariable(typeof(void), null, name, sourceExpression);
        }

        internal virtual SqlExpression UNICODE(Type clrType, SqlUnary uo)
        {
            return FunctionCall(clrType, TypeProvider.From(typeof(int)), "UNICODE", new[] { uo.Operand }, uo.SourceExpression);
        }

        internal virtual MethodSupport GetConvertMethodSupport(SqlMethodCall mc)
        {
            //return MethodSupport.None;
            if ((mc.Method.IsStatic && (mc.Method.DeclaringType == typeof(Convert))) && (mc.Arguments.Count == 1))
            {
                switch (mc.Method.Name)
                {
                    case "ToBoolean":
                    case "ToDecimal":
                    case "ToByte":
                    case "ToChar":
                    case "ToDouble":
                    case "ToInt16":
                    case "ToInt32":
                    case "ToInt64":
                    case "ToSingle":
                    case "ToString":
                        return MethodSupport.Method;

                    case "ToDateTime":
                        if ((mc.Arguments[0].ClrType != typeof(string)) && (mc.Arguments[0].ClrType != typeof(DateTime)))
                        {
                            return MethodSupport.MethodGroup;
                        }
                        return MethodSupport.Method;
                }
            }
            return MethodSupport.None;
        }

        internal virtual SqlExpression String_Remove(SqlMethodCall mc)
        {
            var sourceExpression = mc.SourceExpression;
            if (mc.Arguments.Count != 1)
            {
                if (mc.Arguments.Count == 2)
                {
                    return this.FunctionCall(typeof(string), "STUFF", new SqlExpression[] { mc.Object, this.Add(mc.Arguments[0], 1), mc.Arguments[1], this.ValueFromObject("", false, sourceExpression) }, sourceExpression);
                }
                throw Error.MethodHasNoSupportConversionToSql(mc.Method);
            }
            return FunctionCall(typeof(string), "STUFF", new[] { mc.Object, this.Add(mc.Arguments[0], 1), this.CLRLENGTH(mc.Object), this.ValueFromObject("", false, sourceExpression) }, sourceExpression);

        }

        internal virtual SqlExpression DateTime_Date(SqlMember m, SqlExpression expr)
        {
            var expression = DATEPART("MILLISECOND", expr);
            var expression4 = DATEPART("SECOND", expr);
            var expression5 = DATEPART("MINUTE", expr);
            var expression6 = DATEPART("HOUR", expr);
            var expression7 = expr;
            expression7 = DATEADD("MILLISECOND", Unary(SqlNodeType.Negate, expression), expression7);
            expression7 = DATEADD("SECOND", Unary(SqlNodeType.Negate, expression4), expression7);
            expression7 = DATEADD("MINUTE", Unary(SqlNodeType.Negate, expression5), expression7);
            return DATEADD("HOUR", Unary(SqlNodeType.Negate, expression6), expression7);
        }

        internal virtual SqlExpression DateTime_Day(SqlMember m, SqlExpression expr)
        {
            string datePart = GetDatePart(m.Member.Name);
            return DATEPART(datePart, expr);
        }

        internal virtual SqlExpression DateTime_DayOfYear(SqlMember m, SqlExpression expr)
        {
            string datePart = GetDatePart(m.Member.Name);
            return DATEPART(datePart, expr);
        }

        internal virtual SqlNode DateTime_TimeOfDay(SqlMember member, SqlExpression expr)
        {
            SqlExpression expression8 = this.DATEPART("HOUR", expr);
            SqlExpression expression9 = this.DATEPART("MINUTE", expr);
            SqlExpression expression10 = this.DATEPART("SECOND", expr);
            SqlExpression expression11 = this.DATEPART("MILLISECOND", expr);
            SqlExpression expression12 = this.Multiply(this.ConvertToBigint(expression8), 0x861c46800L);
            SqlExpression expression13 = this.Multiply(this.ConvertToBigint(expression9), 0x23c34600L);
            SqlExpression expression14 = this.Multiply(this.ConvertToBigint(expression10), 0x989680L);
            SqlExpression expression15 = this.Multiply(this.ConvertToBigint(expression11), 0x2710L);
            return this.ConvertTo(typeof(TimeSpan), this.Add(new SqlExpression[] { expression12, expression13, expression14, expression15 }));

        }

        internal virtual SqlExpression String_IndexOf(SqlMethodCall mc, Expression sourceExpression)
        {
            if (mc.Arguments.Count == 1)
            {
                if ((mc.Arguments[0] is SqlValue) && (((SqlValue)mc.Arguments[0]).Value == null))
                {
                    throw Error.ArgumentNull("value");
                }
                var when = new SqlWhen(Binary(SqlNodeType.EQ, CLRLENGTH(mc.Arguments[0]), ValueFromObject(0, sourceExpression)), ValueFromObject(0, sourceExpression));
                SqlExpression expression9 = Subtract(FunctionCall(typeof(int), "CHARINDEX", new[] { mc.Arguments[0], mc.Object }, sourceExpression), 1);
                return SearchedCase(new SqlWhen[] { when }, expression9, sourceExpression);
            }
            if (mc.Arguments.Count == 2)
            {
                if ((mc.Arguments[0] is SqlValue) && (((SqlValue)mc.Arguments[0]).Value == null))
                {
                    throw Error.ArgumentNull("value");
                }
                if (mc.Arguments[1].ClrType == typeof(StringComparison))
                {
                    throw Error.IndexOfWithStringComparisonArgNotSupported();
                }
                SqlExpression expression10 = Binary(SqlNodeType.EQ, CLRLENGTH(mc.Arguments[0]), ValueFromObject(0, sourceExpression));
                SqlWhen when2 = new SqlWhen(AndAccumulate(expression10, Binary(SqlNodeType.LE, Add(mc.Arguments[1], 1), CLRLENGTH(mc.Object))), mc.Arguments[1]);
                SqlExpression expression11 = Subtract(FunctionCall(typeof(int), "CHARINDEX", new SqlExpression[] { mc.Arguments[0], mc.Object, Add(mc.Arguments[1], 1) }, sourceExpression), 1);
                return SearchedCase(new SqlWhen[] { when2 }, expression11, sourceExpression);
            }
            if (mc.Arguments.Count != 3)
            {
                //throw GetMethodSupportException(mc);
                Error.MethodHasNoSupportConversionToSql(mc);
                //goto Label_1B30;
            }
            if ((mc.Arguments[0] is SqlValue) && (((SqlValue)mc.Arguments[0]).Value == null))
            {
                throw Error.ArgumentNull("value");
            }
            if (mc.Arguments[2].ClrType == typeof(StringComparison))
            {
                throw Error.IndexOfWithStringComparisonArgNotSupported();
            }
            SqlExpression left = Binary(SqlNodeType.EQ, CLRLENGTH(mc.Arguments[0]), ValueFromObject(0, sourceExpression));
            SqlWhen when3 = new SqlWhen(AndAccumulate(left, Binary(SqlNodeType.LE, Add(mc.Arguments[1], 1), CLRLENGTH(mc.Object))), mc.Arguments[1]);
            SqlExpression expression13 = FunctionCall(typeof(string), "SUBSTRING", new SqlExpression[] { mc.Object, ValueFromObject(1, false, sourceExpression), Add(new SqlExpression[] { mc.Arguments[1], mc.Arguments[2] }) }, sourceExpression);
            SqlExpression @else = Subtract(FunctionCall(typeof(int), "CHARINDEX", new SqlExpression[] { mc.Arguments[0], expression13, Add(mc.Arguments[1], 1) }, sourceExpression), 1);
            return SearchedCase(new SqlWhen[] { when3 }, @else, sourceExpression);
        }

        internal virtual SqlExpression Math_Floor(SqlMethodCall mc)
        {
            return CreateFunctionCallStatic1(mc.Arguments[0].ClrType, "FLOOR", mc.Arguments, mc.SourceExpression);
        }

        protected SqlExpression CreateFunctionCallStatic2(Type type, string functionName, List<SqlExpression> arguments, Expression source)
        {
            return FunctionCall(type, functionName, new[] { arguments[0], arguments[1] }, source);
        }

        protected SqlExpression CreateFunctionCallStatic1(Type type, string functionName, List<SqlExpression> arguments, Expression source)
        {
            return FunctionCall(type, functionName, new[] { arguments[0] }, source);
        }

        internal virtual SqlExpression DateTime_DayOfWeek(SqlMember m, SqlExpression expr)
        {
            SqlExpression expression16 = DATEPART("dw", expr);
            var fun = Add(new SqlVariable(typeof(int), Default(typeof(int)), "@@DATEFIRST", expr.SourceExpression), 6);
            return ConvertTo(typeof(DayOfWeek), Mod(Add(new[] { expression16, fun }), 7L));
        }

        internal virtual bool IsSupportedDateTimeMember(SqlMember m)
        {
            if (m.Expression.ClrType == typeof(DateTime))
            {
                string str2;
                if (GetDatePart(m.Member.Name) != null)
                {
                    return true;
                }
                if (((str2 = m.Member.Name) != null) && (((str2 == "Date") || (str2 == "TimeOfDay")) || (str2 == "DayOfWeek")))
                {
                    return true;
                }
            }
            return false;
        }

        private static string GetDatePart(string memberName)
        {
            switch (memberName)
            {
                case "Year":
                case "Month":
                case "Day":
                case "DayOfYear":
                //case "DayOfWeek":
                case "Hour":
                case "Minute":
                case "Second":
                case "Millisecond":
                    return memberName;
            }
            return null;
        }

        internal virtual MethodSupport GetStringMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.DeclaringType == typeof(string))
            {
                if (mc.Method.IsStatic)
                {
                    if (mc.Method.Name == "Concat")
                    {
                        return MethodSupport.Method;
                    }
                }
                else
                {
                    switch (mc.Method.Name)
                    {
                        case "Contains":
                        case "StartsWith":
                        case "EndsWith":
                            if (mc.Arguments.Count != 1)
                            {
                                return MethodSupport.MethodGroup;
                            }
                            return MethodSupport.Method;

                        case "IndexOf":
                        case "LastIndexOf":
                            if (mc.Arguments[mc.Arguments.Count - 1].ClrType == typeof(StringComparison))
                                return MethodSupport.None;
                            if (((mc.Arguments.Count != 1) && (mc.Arguments.Count != 2)) && (mc.Arguments.Count != 3))
                            {
                                return MethodSupport.MethodGroup;
                            }
                            return MethodSupport.Method;
                        case "Insert":
                            if (mc.Arguments.Count != 2)
                            {
                                return MethodSupport.MethodGroup;
                            }
                            return MethodSupport.Method;

                        case "PadLeft":
                        case "PadRight":
                        case "Remove":
                        case "Substring":
                            if ((mc.Arguments.Count != 1) && (mc.Arguments.Count != 2))
                            {
                                return MethodSupport.MethodGroup;
                            }
                            return MethodSupport.Method;

                        case "Replace":
                            return MethodSupport.Method;

                        case "Trim":
                        case "TrimEnd":
                        case "ToLower":
                        case "ToUpper":
                            if (mc.Arguments.Count == 0)
                            {
                                return MethodSupport.Method;
                            }
                            return MethodSupport.MethodGroup;

                        case "get_Chars":
                        case "CompareTo":
                            if (mc.Arguments.Count != 1)
                            {
                                return MethodSupport.MethodGroup;
                            }
                            return MethodSupport.Method;
                    }
                }
            }
            return MethodSupport.None;
        }

        internal virtual SqlExpression TranslateConvertStaticMethod(SqlMethodCall mc)
        {
            //SqlExpression expression = null;
            if (mc.Arguments.Count != 1)
            {
                return null;
            }
            var expr = mc.Arguments[0];
            Type type;
            switch (mc.Method.Name)
            {
                case "ToBoolean":
                    type = typeof(bool);
                    break;

                case "ToDecimal":
                    type = typeof(decimal);
                    break;

                case "ToByte":
                    type = typeof(byte);
                    break;

                case "ToChar":
                    type = typeof(char);
                    if (expr.SqlType.IsChar)
                    {
                        TypeProvider.From(type, 1);
                    }
                    break;

                case "ToDateTime":
                    {
                        var nonNullableType = TypeSystem.GetNonNullableType(expr.ClrType);
                        if ((nonNullableType != typeof(string)) && (nonNullableType != typeof(DateTime)))
                        {
                            throw Error.ConvertToDateTimeOnlyForDateTimeOrString();
                        }
                        type = typeof(DateTime);
                        break;
                    }
                case "ToDouble":
                    type = typeof(double);
                    break;

                case "ToInt16":
                    type = typeof(short);
                    break;

                case "ToInt32":
                    type = typeof(int);
                    break;

                case "ToInt64":
                    type = typeof(long);
                    break;

                case "ToSingle":
                    type = typeof(float);
                    break;

                case "ToString":
                    type = typeof(string);
                    break;

                case "ToSByte":
                    type = typeof(sbyte);
                    break;
                default:
                    throw Error.MethodHasNoSupportConversionToSql(mc);
            }
            if ((this.TypeProvider.From(type) != expr.SqlType) || ((expr.ClrType == typeof(bool)) && (type == typeof(int))))
            {
                return this.ConvertTo(type, expr);
            }
            if (this.TypeProvider.From(type) != expr.SqlType)
            {
                return this.ConvertTo(type, expr);
            }
            if ((type != expr.ClrType) && (TypeSystem.GetNonNullableType(type) == TypeSystem.GetNonNullableType(expr.ClrType)))
            {
                return new SqlLift(type, expr, expr.SourceExpression);
            }
            return expr;
        }

        internal virtual SqlExpression String_GetChar(SqlMethodCall mc, Expression sourceExpression)
        {
            if (mc.Arguments.Count != 1)
            {
                //throw GetMethodSupportException(mc);
                Error.MethodHasNoSupportConversionToSql(mc);
            }
            var args = new[] { mc.Object, Add(mc.Arguments[0], 1), 
                               ValueFromObject(1, false, sourceExpression) };
            return FunctionCall(typeof(char), "SUBSTRING", args, sourceExpression);
        }

        internal virtual SqlExpression String_Replace(SqlMethodCall mc)
        {
            if ((mc.Arguments[0] is SqlValue) && (((SqlValue)mc.Arguments[0]).Value == null))
            {
                throw Error.ArgumentNull("old");
            }
            if ((mc.Arguments[1] is SqlValue) && (((SqlValue)mc.Arguments[1]).Value == null))
            {
                throw Error.ArgumentNull("new");
            }
            return FunctionCall(typeof(string), "REPLACE", new[] { mc.Object, mc.Arguments[0], mc.Arguments[1] }, mc.SourceExpression);

        }


        internal virtual SqlExpression Math_Atan(SqlMethodCall mc, Expression sourceExpression)
        {
            return CreateFunctionCallStatic1(typeof(double), "ATAN", mc.Arguments, sourceExpression);
        }

        internal virtual SqlExpression Math_Atan2(SqlMethodCall mc, Expression sourceExpression)
        {
            Debug.Assert(mc.Method.Name == "Atan2");
            return this.CreateFunctionCallStatic2(typeof(double), "ATN2", mc.Arguments, sourceExpression);
        }

        internal virtual SqlExpression Math_Cosh(SqlMethodCall mc, Expression sourceExpression)
        {
            SqlExpression expression = mc.Arguments[0];
            SqlExpression expression3 = FunctionCall(typeof(double), "EXP", new[] { expression }, sourceExpression);
            SqlExpression expression4 = Unary(SqlNodeType.Negate, expression, sourceExpression);
            SqlExpression expression5 = FunctionCall(typeof(double), "EXP", new[] { expression4 }, sourceExpression);
            return Divide(Add(new[] { expression3, expression5 }), 2L);
        }

        internal virtual SqlExpression Math_Sinh(SqlMethodCall mc, Expression sourceExpression)
        {
            SqlExpression expression22 = mc.Arguments[0];
            SqlExpression expression23 = FunctionCall(typeof(double), "EXP", new SqlExpression[] { expression22 }, sourceExpression);
            SqlExpression expression24 = Unary(SqlNodeType.Negate, expression22, sourceExpression);
            SqlExpression expression25 = FunctionCall(typeof(double), "EXP", new SqlExpression[] { expression24 }, sourceExpression);
            return Divide(Subtract(expression23, expression25), (long)2L);
        }

        internal virtual SqlExpression MakeCoalesce(SqlExpression left, SqlExpression right, Type type, Expression sourceExpression)
        {
            return FunctionCall(type, "ISNULL", new[] { left, right }, sourceExpression);
        }

        internal virtual SqlExpression DateTime_Hour(SqlMember m, SqlExpression expr)
        {
            string datePart = GetDatePart(m.Member.Name);
            return DATEPART(datePart, expr);
        }

        internal virtual SqlExpression DateTime_Minute(SqlMember m, SqlExpression expr)
        {
            string datePart = GetDatePart(m.Member.Name);
            return DATEPART(datePart, expr);
        }

        internal virtual SqlExpression DateTime_Month(SqlMember m, SqlExpression expr)
        {
            string datePart = GetDatePart(m.Member.Name);
            return DATEPART(datePart, expr);
        }

        internal virtual SqlExpression DateTime_Second(SqlMember m, SqlExpression expr)
        {
            string datePart = GetDatePart(m.Member.Name);
            return DATEPART(datePart, expr);
        }

        internal virtual SqlNode DateTime_Year(SqlMember m, SqlExpression expr)
        {
            string datePart = GetDatePart(m.Member.Name);
            return DATEPART(datePart, expr);
        }


        internal virtual SqlExpression Math_Log10(SqlMethodCall mc)
        {
            Debug.Assert(mc.Method.Name == "Log10");
            return CreateFunctionCallStatic1(typeof(double), "LOG10", mc.Arguments, mc.SourceExpression);
        }

        internal virtual SqlExpression Math_Log(SqlMethodCall mc)
        {
            //Debug.Assert(mc.Method.Name == "Log10");
            return CreateFunctionCallStatic1(typeof(double), "ln", mc.Arguments, mc.SourceExpression);
        }

        // Fields
        private static readonly string[] dateParts = new[] { "Year", "Month", "Day", "Hour", "Minute", "Second", "Millisecond" };
        internal virtual MethodSupport GetSqlMethodsMethodSupport(SqlMethodCall mc)
        {
            //MY CODE
            if (Attribute.IsDefined(mc.Method, typeof(FunctionAttribute)))
                return MethodSupport.Method;
            //================================
            if (mc.Method.IsStatic && (mc.Method.DeclaringType == typeof(SqlMethods)))
            {
                if (mc.Method.Name.StartsWith("DateDiff", StringComparison.Ordinal) && (mc.Arguments.Count == 2))
                {
                    foreach (string str in dateParts)
                    {
                        if (mc.Method.Name == ("DateDiff" + str))
                        {
                            if (mc.Arguments.Count == 2)
                            {
                                return MethodSupport.Method;
                            }
                            return MethodSupport.MethodGroup;
                        }
                    }
                }
                else
                {
                    if (mc.Method.Name == "Like")
                    {
                        if (mc.Arguments.Count == 2)
                        {
                            return MethodSupport.Method;
                        }
                        if (mc.Arguments.Count == 3)
                        {
                            return MethodSupport.Method;
                        }
                        return MethodSupport.MethodGroup;
                    }
                    if (mc.Method.Name == "RawLength")
                    {
                        return MethodSupport.Method;
                    }
                    if(mc.Method.Name == "Identity")
                    {
                        return MethodSupport.Method;
                    }
                    
                }
            }
            return MethodSupport.None;
        }

        internal virtual SqlExpression TranslateSqlMethodsMethod(SqlMethodCall mc)
        {
            Expression sourceExpression = mc.SourceExpression;
            const SqlExpression expression2 = null;
            string name = mc.Method.Name;
            if (name.StartsWith("DateDiff", StringComparison.Ordinal) && (mc.Arguments.Count == 2))
            {
                foreach (string str2 in dateParts)
                {
                    if (mc.Method.Name == ("DateDiff" + str2))
                    {
                        SqlExpression expression3 = mc.Arguments[0];
                        SqlExpression expression4 = mc.Arguments[1];
                        SqlExpression expression5 = new SqlVariable(typeof(void), null, str2, sourceExpression);
                        return this.FunctionCall(typeof(int), "DATEDIFF", new[] { expression5, expression3, expression4 }, sourceExpression);
                    }
                }
                return expression2;
            }
            if (name == "Like")
            {
                if (mc.Arguments.Count == 2)
                {
                    return this.Like(mc.Arguments[0], mc.Arguments[1], null, sourceExpression);
                }
                if (mc.Arguments.Count != 3)
                {
                    return expression2;
                }
                return this.Like(mc.Arguments[0], mc.Arguments[1], this.ConvertTo(typeof(string), mc.Arguments[2]), sourceExpression);
            }
            if (name == "RawLength")
            {
                return this.DATALENGTH(mc.Arguments[0]);
            }
            return expression2;
        }

        internal virtual MethodSupport GetDateTimeMethodSupport(SqlMethodCall mc)
        {
            if (!mc.Method.IsStatic && (mc.Method.DeclaringType == typeof(DateTime)))
            {
                switch (mc.Method.Name)
                {
                    case "CompareTo":
                    case "AddTicks":
                    case "AddMonths":
                    case "AddYears":
                    case "AddMilliseconds":
                    case "AddSeconds":
                    case "AddMinutes":
                    case "AddHours":
                    case "AddDays":
                        return MethodSupport.Method;

                    case "Add":
                        if ((mc.Arguments.Count == 1) && (mc.Arguments[0].ClrType == typeof(TimeSpan)))
                        {
                            return MethodSupport.Method;
                        }
                        return MethodSupport.MethodGroup;
                }
            }
            return MethodSupport.None;
        }

        internal virtual SqlExpression DateTime_Subtract(SqlMethodCall mc)
        {
            throw new NotImplementedException();
        }

        internal virtual SqlExpression DateTime_SubtractDays(SqlMethodCall mc)
        {
            throw new NotImplementedException();
        }
    }
}