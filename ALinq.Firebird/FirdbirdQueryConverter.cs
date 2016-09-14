using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq.Firebird
{
    class FirdbirdQueryConverter : ALinq.SqlClient.QueryConverter
    {
        public FirdbirdQueryConverter(IDataServices services, ITypeSystemProvider typeProvider, Translator translator, SqlFactory sql)
            : base(services, typeProvider, translator, sql)
        {
        }

        protected override SqlSelect GenerateSkipTake(SqlSelect sequence, SqlExpression skipExp, SqlExpression takeExp)
        {
            var valueType = typeof(int);
            var sqlType = typeProvider.From(valueType);
            skipExp = skipExp != null ? sql.Value(typeof(int), typeProvider.From(typeof(int)), ((SqlValue)skipExp).Value, false, dominatingExpression)
                                      : sql.Value(typeof(int), typeProvider.From(typeof(int)), -1, false, dominatingExpression); //VisitExpression(Expression.Constant(0));
            takeExp = takeExp != null ? sql.Value(typeof(int), typeProvider.From(typeof(int)), ((SqlValue)takeExp).Value, false, dominatingExpression)
                                      : sql.Value(typeof(int), typeProvider.From(typeof(int)), -1, false, dominatingExpression); //VisitExpression(Expression.Constant(Int64.MaxValue));
            IEnumerable<SqlExpression> expressions = new[] { skipExp, takeExp };
            sequence.Top = new SqlFunctionCall(valueType, sqlType, "SkipTake", expressions, takeExp.SourceExpression);
            return sequence;
        }

        protected override SqlExpression GetReturnIdentityExpression(MetaDataMember idMember, bool isOutputFromInsert)
        {
            var name = string.Format("GEN_ID({0},0)", FirebirdSqlBuilder.GetSequenceName(idMember));
            return new SqlVariable(typeof(int), typeProvider.From(typeof(int)), name, dominatingExpression);
            //var selection = new SqlVariable(typeof(int), typeProvider.From(typeof(int)), name, dominatingExpression);
            //var from = new SqlVariable(typeof(int), typeProvider.From(typeof(int)), "$$FFF", dominatingExpression);
            //return new SqlSelect(selection, new SqlSource(SqlNodeType.Variable, from), dominatingExpression);
        }

        protected override SqlExpression GetInsertIdentityExpression(MetaDataMember member)
        {
            var exp = new SqlVariable(member.Type, typeProvider.From(member.Type),
                                                 "NEXT VALUE FOR " + FirebirdSqlBuilder.GetSequenceName(member), dominatingExpression);
            return exp;
        }

        protected override SqlNode VisitFirst(Expression sequence, LambdaExpression lambda, bool isFirst)
        {
            //var takeExp = VisitExpression(Expression.Constant(1));
            //return GenerateSkipTake(VisitSequence(sequence), null, takeExp);
            SqlSelect select = this.LockSelect(this.VisitSequence(sequence));
            if (lambda != null)
            {
                this.map[lambda.Parameters[0]] = select.Selection;
                select.Where = this.VisitExpression(lambda.Body);
            }
            if (isFirst)
            {
                //select.Top = this.sql.ValueFromObject(1, false, this.dominatingExpression);
                var skipExp = sql.Value(typeof(int), typeProvider.From(typeof(int)), -1, false, dominatingExpression);
                var takeExp = sql.Value(typeof(int), typeProvider.From(typeof(int)), 1, false, dominatingExpression);
                IEnumerable<SqlExpression> expressions = new[] { skipExp, takeExp };
                select.Top = new SqlFunctionCall(typeof(int), typeProvider.From(typeof(int)), "SkipTake", expressions, takeExp.SourceExpression);
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

        #region MyRegion
        //protected override SqlNode VisitInsert(Expression item, LambdaExpression resultSelector)
        //{
        //    if (item == null)
        //    {
        //        throw SqlClient.Error.ArgumentNull("item");
        //    }
        //    dominatingExpression = item;
        //    MetaTable table = Services.Model.GetTable(item.Type);
        //    Expression sourceExpression = Services.Context.GetTable(table.RowType.Type).Expression;
        //    var expression2 = item as ConstantExpression;
        //    if (expression2 == null)
        //    {
        //        throw SqlClient.Error.InsertItemMustBeConstant();
        //    }
        //    if (expression2.Value == null)
        //    {
        //        throw SqlClient.Error.ArgumentNull("item");
        //    }
        //    var bindings = new List<SqlMemberAssign>();
        //    MetaType inheritanceType = table.RowType.GetInheritanceType(expression2.Value.GetType());
        //    SqlExpression expression3 = sql.ValueFromObject(expression2.Value, true, sourceExpression);
        //    foreach (var member in inheritanceType.PersistentDataMembers)
        //    {
        //        if (!member.IsAssociation && !member.IsVersion)
        //        {
        //            if (member.IsDbGenerated)
        //            {
        //                var exp = new SqlVariable(member.Type, typeProvider.From(member.Type),
        //                                          "NEXT VALUE FOR " + FirebirdSqlBuilder.GetSequenceName(member), dominatingExpression);
        //                bindings.Add(new SqlMemberAssign(member.Member, exp));
        //            }
        //            else
        //                bindings.Add(new SqlMemberAssign(member.Member, sql.Member(expression3, member.Member)));
        //        }
        //    }
        //    var constructor = inheritanceType.Type.GetConstructor(Type.EmptyTypes);
        //    SqlNew expr = sql.New(inheritanceType, constructor, null, null, bindings, item);
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
        //        flag = IsDbGeneratedKeyProjectionOnly(resultSelector.Body, dBGeneratedIdentityMember);
        //        if ((dBGeneratedIdentityMember.Type == typeof(Guid)) &&
        //            ((ConverterStrategy & ConverterStrategy.CanOutputFromInsert) != ConverterStrategy.Default))
        //        {
        //            insert.OutputKey = new SqlColumn(dBGeneratedIdentityMember.Type,
        //                                            sql.Default(dBGeneratedIdentityMember), dBGeneratedIdentityMember.Name,
        //                                            dBGeneratedIdentityMember, null, dominatingExpression);
        //            if (!flag)
        //            {
        //                insert.OutputToLocal = true;
        //            }
        //        }
        //    }
        //    SqlSelect select2 = null;
        //    var alias = new SqlAlias(table2);
        //    var ref2 = new SqlAliasRef(alias);
        //    map.Add(resultSelector.Parameters[0], ref2);
        //    SqlExpression selection = VisitExpression(resultSelector.Body);
        //    SqlExpression expression5;
        //    if (dBGeneratedIdentityMember != null)
        //    {
        //        expression5 = sql.Binary(SqlNodeType.EQ, sql.Member(ref2, dBGeneratedIdentityMember.Member),
        //                                      GetReturnIdentityExpression(dBGeneratedIdentityMember, insert.OutputKey != null));
        //    }
        //    else
        //    {
        //        SqlExpression right = VisitExpression(item);
        //        expression5 = sql.Binary(SqlNodeType.EQ2V, ref2, right);
        //    }
        //    var select = new SqlSelect(selection, alias, resultSelector) { Where = expression5 };
        //    if ((dBGeneratedIdentityMember != null) && flag)
        //    {
        //        if (insert.OutputKey == null)
        //        {
        //            SqlExpression identityExpression = GetReturnIdentityExpression(dBGeneratedIdentityMember, false);
        //            select2 = new SqlSelect(identityExpression, null, resultSelector);
        //        }
        //        select.DoNotOutput = true;
        //    }
        //    var block = new SqlBlock(dominatingExpression);
        //    block.Statements.Add(insert);
        //    if (select2 != null)
        //    {
        //        block.Statements.Add(select2);
        //    }
        //    block.Statements.Add(select);
        //    return block;

        //} 
        #endregion

        /*
        protected override SqlNode VisitConstant(ConstantExpression expression)
        {
            Type type = expression.Type;
            SqlExpression result;
            if (expression.Value == null)
            {
                result = sql.TypedLiteralNull(type, dominatingExpression);
                return result;
            }
            if (type == typeof(object))
            {
                type = expression.Value.GetType();
            }
            if (type == typeof(string) || type.IsValueType)
                return new SqlValue(type, typeProvider.From(expression.Value), expression.Value, false, dominatingExpression);
            
            result = sql.ValueFromObject(expression.Value, type, true, dominatingExpression);
            return result;
        }
        */


    }
}