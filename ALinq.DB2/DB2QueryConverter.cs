using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ALinq.Mapping;
using ALinq.Oracle;
using ALinq.SqlClient;

namespace ALinq.DB2
{
    class DB2QueryConverter : QueryConverter
    {
        public DB2QueryConverter(IDataServices services, ITypeSystemProvider typeProvider, Translator translator, SqlFactory sql)
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
            var select = new SqlSelect(ref4, alias2, this.dominatingExpression)
                             {
                                 Where = this.sql.Binary(SqlNodeType.EQ2V, selection, ref4)
                             };
            SqlSubSelect expression = this.sql.SubSelect(SqlNodeType.Exists, select);
            var select6 = new SqlSelect(selection, alias, this.dominatingExpression)
                              {
                                  Where = this.sql.Unary(SqlNodeType.Not, expression, this.dominatingExpression),
                                  Top = takeExp
                              };
            return select6;
        }

        protected override SqlExpression GetReturnIdentityExpression(MetaDataMember idMember, bool isOutputFromInsert)
        {
            var name = "PREVVAL FOR " + DB2Builder.GetSequenceName(idMember, translator.Provider.SqlIdentifier) + " FROM " + idMember.DeclaringType.Table.TableName;//OracleSqlBuilder.GetSequenceName(idMember, translator.Provider.SqlIdentifier) + ".CURRVAL";
            return new SqlVariable(idMember.Type, typeProvider.From(idMember.Type), name, this.dominatingExpression);
        }

        protected override SqlExpression GetInsertIdentityExpression(MetaDataMember member)
        {
            var exp = new SqlVariable(member.Type, typeProvider.From(member.Type),
                                      "NEXTVAL FOR " + DB2Builder.GetSequenceName(member, translator.Provider.SqlIdentifier), dominatingExpression);
            return exp;
        }

    }
}
