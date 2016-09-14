using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq.PostgreSQL
{
    class PgsqlQueryConverter : QueryConverter
    {
        public PgsqlQueryConverter(IDataServices services, ITypeSystemProvider typeProvider, Translator translator, SqlFactory sql)
            : base(services, typeProvider, translator, sql)
        {
        }

        protected override SqlExpression GetReturnIdentityExpression(MetaDataMember idMember, bool isOutputFromInsert)
        {
            var name = "CURRVAL('" + PgsqlBuilder.GetSequenceName(idMember, translator.Provider.SqlIdentifier) + "') FROM " + idMember.DeclaringType.Table.TableName;//OracleSqlBuilder.GetSequenceName(idMember, translator.Provider.SqlIdentifier) + ".CURRVAL";
            return new SqlVariable(idMember.Type, typeProvider.From(idMember.Type), name, this.dominatingExpression);
        }

        protected override SqlSelect GenerateSkipTake(SqlSelect sequence, SqlExpression skipExp, SqlExpression takeExp)
        {
            var valueType = typeof(int);
            var sqlType = typeProvider.From(valueType);
            //skipExp = skipExp != null ? sql.Value(typeof(int), typeProvider.From(typeof(int)), ((SqlValue)skipExp).Value, false, dominatingExpression)
            //                          : sql.Value(typeof(int), typeProvider.From(typeof(int)), 0, false, dominatingExpression);
            //takeExp = takeExp != null ? sql.Value(typeof(int), typeProvider.From(typeof(int)), ((SqlValue)takeExp).Value, false, dominatingExpression)
            //                          : sql.Value(typeof(int), typeProvider.From(typeof(int)), 0, false, dominatingExpression);

            //IEnumerable<SqlExpression> expressions = new[] { takeExp, sql.VariableFromName("OFFSET", dominatingExpression), skipExp };
            //sequence.Top = new SqlFunctionCall(valueType, sqlType, "LIMIT", expressions, takeExp.SourceExpression)
            //                   {
            //                       Brackets = false,
            //                       Comma = false
            //                   };
            //return sequence;
            if (takeExp != null && skipExp != null)
            {
                IEnumerable<SqlExpression> expressions = new[] { takeExp, sql.VariableFromName("OFFSET", dominatingExpression), skipExp };
                sequence.Top = new SqlFunctionCall(valueType, sqlType, "LIMIT", expressions, takeExp.SourceExpression)
                                   {
                                       Brackets = false,
                                       Comma = false
                                   };
            }
            else if (takeExp != null)
            {
                IEnumerable<SqlExpression> expressions = new[] { takeExp };
                sequence.Top = new SqlFunctionCall(valueType, sqlType, "LIMIT", expressions, takeExp.SourceExpression)
                {
                    Brackets = false,
                    Comma = false
                };
            }
            else if (skipExp != null)
            {
                IEnumerable<SqlExpression> expressions = new[] { skipExp };
                sequence.Top = new SqlFunctionCall(valueType, sqlType, "OFFSET", expressions, skipExp.SourceExpression)
                {
                    Brackets = false,
                    Comma = false
                };
            }
            return sequence;
        }

        protected override SqlNode VisitFirst(Expression sequence, LambdaExpression lambda, bool isFirst)
        {
            SqlSelect select = this.LockSelect(this.VisitSequence(sequence));
            if (lambda != null)
            {
                this.map[lambda.Parameters[0]] = select.Selection;
                select.Where = this.VisitExpression(lambda.Body);
            }
            if (isFirst)
            {
                var f = sql.FunctionCall(typeof(int), "LIMIT", new[] { sql.ValueFromObject(1, false, dominatingExpression) },
                                         dominatingExpression);
                f.Brackets = false;
                f.Comma = false;
                select.Top = f;
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

        protected override SqlExpression GetInsertIdentityExpression(MetaDataMember member)
        {
            var exp = new SqlVariable(member.Type, typeProvider.From(member.Type),
                                      "NEXTVAL('" + PgsqlBuilder.GetSequenceName(member, translator.Provider.SqlIdentifier) + "')", dominatingExpression);
            //return new SqlMemberAssign(member.Member, exp);
            return exp;
        }

        //protected override SqlNode VisitInsert(Expression item, LambdaExpression resultSelector)
        //{
        //    if (item == null)
        //    {
        //        throw SqlClient.Error.ArgumentNull("item");
        //    }
        //    if (item.NodeType == ExpressionType.Lambda)
        //    {
        //        return CreateInsertExpression((LambdaExpression)item, resultSelector);
        //        //return VisitInsert1(item, resultSelector);
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
        //    foreach (MetaDataMember member in inheritanceType.PersistentDataMembers)
        //    {
        //        if (!member.IsAssociation && !member.IsVersion)
        //        {
        //            if (member.IsDbGenerated)
        //            {
        //                var exp = new SqlVariable(member.Type, typeProvider.From(member.Type),
        //                                          "NEXTVAL('" + PgsqlBuilder.GetSequenceName(member, translator.Provider.SqlIdentifier) + "')", dominatingExpression);
        //                bindings.Add(new SqlMemberAssign(member.Member, exp));
        //            }
        //            else
        //                bindings.Add(new SqlMemberAssign(member.Member, sql.Member(expression3, member.Member)));
        //        }
        //    }
        //    ConstructorInfo constructor = inheritanceType.Type.GetConstructor(Type.EmptyTypes);
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
        //    SqlSelect select;
        //    SqlSelect select2 = null;
        //    var alias = new SqlAlias(table2);
        //    var ref2 = new SqlAliasRef(alias);
        //    map.Add(resultSelector.Parameters[0], ref2);
        //    SqlExpression selection = VisitExpression(resultSelector.Body);
        //    SqlExpression expression5;
        //    if (dBGeneratedIdentityMember != null)
        //    {
        //        expression5 = sql.Binary(SqlNodeType.EQ, sql.Member(ref2, dBGeneratedIdentityMember.Member),
        //                                      GetIdentityExpression(dBGeneratedIdentityMember, insert.OutputKey != null));
        //    }
        //    else
        //    {
        //        SqlExpression right = VisitExpression(item);
        //        expression5 = sql.Binary(SqlNodeType.EQ2V, ref2, right);
        //    }
        //    select = new SqlSelect(selection, alias, resultSelector) { Where = expression5 };
        //    if ((dBGeneratedIdentityMember != null) && flag)
        //    {
        //        if (insert.OutputKey == null)
        //        {
        //            SqlExpression identityExpression = GetIdentityExpression(dBGeneratedIdentityMember, false);
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
    }
}
