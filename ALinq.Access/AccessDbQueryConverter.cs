using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using ALinq;
using ALinq.Mapping;
using ALinq.SqlClient;

namespace ALinq.Access
{
    class AccessDbQueryConverter : QueryConverter
    {
        public AccessDbQueryConverter(IDataServices services, ITypeSystemProvider typeProvider, Translator translator, SqlFactory sql)
            : base(services, typeProvider, translator, sql)
        {
        }

        protected override SqlExpression GetReturnIdentityExpression(MetaDataMember idMember, bool isOutputFromInsert)
        {
            return new SqlVariable(typeof(int), typeProvider.From(typeof(int)), "@@IDENTITY", dominatingExpression);
        }

     

        protected override SqlSelect GenerateSkipTake(SqlSelect sequence, SqlExpression skipExp, SqlExpression takeExp)
        {

            if (skipExp == null)
                return base.GenerateSkipTake(sequence, skipExp, takeExp);
            Debug.Assert(skipExp != null);

            var skipCount = (int)((SqlValue)skipExp).Value;

            var alias = new SqlAlias(sequence);
            var selection = new SqlAliasRef(alias);
            var select = new SqlSelect(selection, alias, dominatingExpression);

            if (takeExp == null)
            {
                return base.GenerateSkipTake(sequence, skipExp, takeExp);
            }

            var type = sequence.SourceExpression.Type;
            if (type.IsGenericType)
            {
                var args = type.GetGenericArguments();
                if (args.Length == 1)
                {
                    var mappingTable = Services.Model.GetTable(args[0]);
                    if (mappingTable != null)
                        return base.GenerateSkipTake(sequence, skipExp, takeExp);
                }
            }

            var takeCount = (int)((SqlValue)takeExp).Value;
            select.Top = sql.Value(typeof(int), typeProvider.From(typeof(int)), takeCount, false,
                                   dominatingExpression);
            sequence.Top = sql.Value(typeof(int), typeProvider.From(typeof(int)), takeCount + skipCount, false,
                                     dominatingExpression);
            var finder = new OrderByFinder();
            var orderby = finder.GetOrderBy(sequence);

            foreach (var item in orderby)
            {
                if (item.OrderType == SqlOrderType.Descending)
                    select.OrderBy.Add(new SqlOrderExpression(SqlOrderType.Ascending, orderby[0].Expression));
                else
                    select.OrderBy.Add(new SqlOrderExpression(SqlOrderType.Descending, orderby[0].Expression));
            }
            alias = new SqlAlias(select);
            selection = new SqlAliasRef(alias);
            select = new SqlSelect(selection, alias, dominatingExpression);
            foreach (var item in orderby)
            {
                select.OrderBy.Add(new SqlOrderExpression(item.OrderType, item.Expression));
            }
            return select;
        }

        private class OrderByFinder : SqlVisitor
        {
            private IList<SqlOrderExpression> orderby = new List<SqlOrderExpression>();

            public IList<SqlOrderExpression> GetOrderBy(SqlNode sqlNode)
            {
                Visit(sqlNode);
                return orderby;
            }

            internal override SqlSelect VisitSelect(SqlSelect sqlSelect)
            {
                if (orderby == null || orderby.Count == 0)
                    orderby = sqlSelect.OrderBy;
                return base.VisitSelect(sqlSelect);
            }
        }


    }

}
