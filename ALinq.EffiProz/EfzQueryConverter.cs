using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq.EffiProz
{
    class EfzQueryConverter : ALinq.SqlClient.QueryConverter
    {
        public EfzQueryConverter(IDataServices services, ITypeSystemProvider typeProvider, Translator translator, SqlFactory sql)
            : base(services, typeProvider, translator, sql)
        {
        }

        protected override SqlExpression GetReturnIdentityExpression(MetaDataMember idMember, bool isOutputFromInsert)
        {
            return sql.FunctionCall(typeof(int), "IDENTITY", new SqlExpression[] { }, dominatingExpression);
        }

        protected override SqlSelect GenerateSkipTake(SqlSelect sequence, SqlExpression skipExp, SqlExpression takeExp)
        {
            var valueType = typeof(int);
            var sqlType = typeProvider.From(valueType);
            skipExp = skipExp ?? VisitExpression(Expression.Constant(0));
            takeExp = takeExp ?? VisitExpression(Expression.Constant(0));
            IEnumerable<SqlExpression> expressions = new[] { skipExp, takeExp };
            sequence.Top = new SqlFunctionCall(valueType, sqlType, "Limit", expressions, dominatingExpression);
            return sequence;
        }

    }
}
