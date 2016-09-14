using System;
using System.Collections.Generic;
using ALinq.Mapping;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using ALinq;
using ALinq.SqlClient;

namespace ALinq.Oracle
{
    class OracleQueryConverter : QueryConverter
    {
        public OracleQueryConverter(IDataServices services, ITypeSystemProvider typeProvider, Translator translator, SqlFactory sql)
            : base(services, typeProvider, translator, sql)
        {
        }

        protected override SqlSelect GenerateSkipTake(SqlSelect sequence, SqlExpression skipExp, SqlExpression takeExp)
        {
            //return base.GenerateSkipTake(sequence, skipExp, takeExp);
            SqlSelect node = LockSelect(sequence);
            var value2 = skipExp as SqlValue;
            if ((skipExp == null) || ((value2 != null) && (((int)value2.Value) <= 0)))
            {
                skipExp = sql.ValueFromObject(0, dominatingExpression);
            }
            var alias = new SqlAlias(node);
            var selection = new SqlAliasRef(alias);
            if (UseConverterStrategy(ConverterStrategy.SkipWithRowNumber))
            {
                var col = new SqlColumn("ROW_NUMBER",
                                              this.sql.RowNumber(new List<SqlOrderExpression>(),
                                                                 this.dominatingExpression));
                var expr = new SqlColumnRef(col);
                node.Row.Columns.Add(col);
                var select2 = new SqlSelect(selection, alias, this.dominatingExpression);
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
                throw SqlClient.Error.SkipNotSupportedForSequenceTypes();
            }
            var visitor = new SingleTableQueryVisitor();
            visitor.Visit(node);
            if (!visitor.IsValid)
            {
                throw ALinq.SqlClient.Error.SkipRequiresSingleTableQueryWithPKs();
            }
            var select3 = (SqlSelect)SqlDuplicator.Copy(node);
            select3.Top = skipExp;
            var alias2 = new SqlAlias(select3);
            var ref4 = new SqlAliasRef(alias2);
            var select = new SqlSelect(ref4, alias2, this.dominatingExpression);
            select.Where = this.sql.Binary(SqlNodeType.EQ2V, selection, ref4);
            SqlSubSelect expression = this.sql.SubSelect(SqlNodeType.Exists, select);
            var select6 = new SqlSelect(selection, alias, this.dominatingExpression);
            select6.Where = this.sql.Unary(SqlNodeType.Not, expression, this.dominatingExpression);
            select6.Top = takeExp;
            return select6;
        }


        protected override SqlNode VisitMethodCall(MethodCallExpression mc)
        {
            //´¦Àí×Ö·û´®º¯Êý¡£
            if (mc.Object != null && mc.Object.Type == typeof(string))
            {
                switch (mc.Method.Name)
                {

                    case "Trim":
                        {
                            var clrType = mc.Method.ReturnType;
                            var expressions = new[] { VisitExpression(mc.Object) };
                            var node = new SqlFunctionCall(clrType, typeProvider.From(clrType), "Trim", expressions,
                                                           dominatingExpression);
                            return node;
                        }
                    case "TrimStart":
                        {
                            var clrType = mc.Method.ReturnType;
                            var expressions = new[] { VisitExpression(mc.Object) };
                            var node = new SqlFunctionCall(clrType, typeProvider.From(clrType), "LTrim", expressions,
                                                           dominatingExpression);
                            return node;
                        }
                    case "TrimEnd":
                        {
                            var clrType = mc.Method.ReturnType;
                            var expressions = new[] { VisitExpression(mc.Object) };
                            var node = new SqlFunctionCall(clrType, typeProvider.From(clrType), "RTrim", expressions,
                                                           dominatingExpression);
                            return node;
                        }

                    case "Remove":
                        {
                            var clrType = mc.Method.ReturnType;
                            Debug.Assert(clrType == typeof(string));
                            var startIndex = (int)((ConstantExpression)mc.Arguments[0]).Value;

                            var arg1 = VisitExpression(mc.Object);
                            var arg2 = VisitExpression(Expression.Constant(1));
                            var arg3 = VisitExpression(Expression.Constant(startIndex));
                            var left = new SqlFunctionCall(typeof(string), typeProvider.From(typeof(string)), "SUBSTR",
                                                           new[] { arg1, arg2, arg3 }, dominatingExpression);

                            if (mc.Arguments.Count == 2)
                            {
                                var count = (int)((ConstantExpression)mc.Arguments[1]).Value;
                                arg2 = VisitExpression(Expression.Constant(startIndex + count));
                                SqlExpression right = new SqlFunctionCall(typeof(string), typeProvider.From(typeof(string)),
                                                                          "SUBSTR", new[] { arg1, arg2 }, dominatingExpression);
                                var result = new SqlBinary(SqlNodeType.Add, clrType, typeProvider.From(clrType), left, right);
                                return result;
                            }
                            Debug.Assert(mc.Arguments.Count == 1);
                            return left;
                        }
                    case "Replace":
                        {
                            var clrType = mc.Method.ReturnType;
                            var sqlType = typeProvider.From(clrType);
                            Debug.Assert(clrType == typeof(string));
                            Debug.Assert(mc.Arguments.Count == 2);

                            var sourceObject = VisitExpression(mc.Object);
                            var oldValue = VisitExpression(mc.Arguments[0]);
                            var newValue = VisitExpression(mc.Arguments[1]);

                            var result = new SqlFunctionCall(clrType, sqlType, "Replace",
                                                             new[] { sourceObject, oldValue, newValue }, dominatingExpression);
                            return result;
                        }
                    default:
                        return base.VisitMethodCall(mc);

                }
            }
            return base.VisitMethodCall(mc);
        }

        protected override SqlExpression GetReturnIdentityExpression(MetaDataMember idMember, bool isOutputFromInsert)
        {
            var name = OracleSqlBuilder.GetSequenceName(idMember, translator.Provider.SqlIdentifier) + ".CURRVAL";
            return new SqlVariable(typeof(decimal), typeProvider.From(typeof(decimal)), name, this.dominatingExpression);
        }

        protected override SqlExpression GetInsertIdentityExpression(MetaDataMember member)
        {
            var exp = new SqlVariable(member.Type, typeProvider.From(member.Type),
                                                 OracleSqlBuilder.GetSequenceName(member, translator.Provider.SqlIdentifier) + ".NEXTVAL", dominatingExpression);
            return exp;
        }





        protected override QueryConverter.ConversionMethod ChooseConversionMethod(Type fromType, Type toType)
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

                //if (type3.IsNumeric && type4.IsNumeric)
                //    return ConversionMethod.Ignore;

                if (((nonNullableType != seqType) && (!type3.IsString || !type3.Equals(type4))) &&
                    (!nonNullableType.IsEnum && !seqType.IsEnum))
                {
                    return ConversionMethod.Convert;
                }
            }
            return ConversionMethod.Ignore;
        }
    }
}
