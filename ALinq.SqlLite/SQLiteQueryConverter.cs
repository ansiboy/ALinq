using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ALinq;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq.SQLite
{
    class SQLiteQueryConverter : QueryConverter
    {
        public SQLiteQueryConverter(IDataServices services, ITypeSystemProvider typeProvider, Translator translator, SqlFactory sql)
            : base(services, typeProvider, translator, sql)
        {

        }

        protected override SqlExpression GetReturnIdentityExpression(ALinq.Mapping.MetaDataMember idMember, bool isOutputFromInsert)
        {
            //return new SqlVariable(typeof(int), typeProvider.From(typeof(int)), " LAST_INSERT_ROWID()", this.dominatingExpression);
            return sql.FunctionCall(typeof(int), "LAST_INSERT_ROWID", new SqlExpression[] { }, dominatingExpression);
        }

        protected override SqlSelect GenerateSkipTake(SqlSelect sequence, SqlExpression skipExp, SqlExpression takeExp)
        {
            var valueType = typeof(int);
            var sqlType = typeProvider.From(valueType);
            skipExp = skipExp ?? VisitExpression(Expression.Constant(-1));
            takeExp = takeExp ?? VisitExpression(Expression.Constant(-1));
            IEnumerable<SqlExpression> expressions = new[] { skipExp, takeExp };
            sequence.Top = new SqlFunctionCall(valueType, sqlType, "Limit", expressions, takeExp.SourceExpression);
            return sequence;
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
                var valueType = typeof(int);
                var sqlType = typeProvider.From(valueType);
                var skipExp = VisitExpression(Expression.Constant(-1));
                var takeExp = VisitExpression(Expression.Constant(1));
                IEnumerable<SqlExpression> expressions = new[] { skipExp, takeExp };
                select.Top = new SqlFunctionCall(valueType, sqlType, "Limit", expressions, takeExp.SourceExpression);//this.sql.ValueFromObject(1, false, this.dominatingExpression);
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
