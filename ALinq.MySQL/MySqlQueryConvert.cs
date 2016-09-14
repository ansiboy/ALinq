using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ALinq;
using ALinq.SqlClient;

namespace ALinq.MySQL
{
    class MySqlQueryConverter : QueryConverter
    {
        public MySqlQueryConverter(IDataServices services, ITypeSystemProvider typeProvider, Translator translator, SqlFactory sql)
            : base(services, typeProvider, translator, sql)
        {
        }

        protected override SqlExpression GetReturnIdentityExpression(Mapping.MetaDataMember id, bool isOutputFromInsert)
        {
            return new SqlVariable(typeof(int), typeProvider.From(typeof(int)), "LAST_INSERT_ID()", dominatingExpression);
        }

        //protected override SqlNode VisitTake(Expression sequence, Expression count)
        //{
        //    var takeExp = VisitExpression(count);
        //    return GenerateSkipTake(VisitSequence(sequence), null, takeExp);
        //}

        protected override SqlSelect GenerateSkipTake(SqlSelect sequence, SqlExpression skipExp, SqlExpression takeExp)
        {
            var valueType = typeof(int);
            var sqlType = typeProvider.From(valueType);
            skipExp = skipExp ?? sql.ValueFromObject(-1, dominatingExpression);
            takeExp = takeExp ?? sql.ValueFromObject(-1, dominatingExpression);

            if (skipExp is SqlValue)
                ((SqlValue)skipExp).IsClientSpecified = false;

            if (takeExp is SqlValue)
                ((SqlValue)takeExp).IsClientSpecified = false;

            var expressions = new[] { takeExp, skipExp };

            sequence.Top = new SqlFunctionCall(valueType, sqlType, "Limit", expressions, dominatingExpression);
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
                var valueType = typeof(int);
                var sqlType = typeProvider.From(valueType);
                var skipExp = sql.ValueFromObject(0, false, dominatingExpression);
                var takeExp = sql.ValueFromObject(1, false, dominatingExpression);
                IEnumerable<SqlExpression> expressions = new[] { takeExp, skipExp };
                select.Top = new SqlFunctionCall(valueType, sqlType, "Limit", expressions, takeExp.SourceExpression);
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

    }
}

