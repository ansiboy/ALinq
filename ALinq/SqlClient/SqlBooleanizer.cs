using ALinq.Mapping;

namespace ALinq.SqlClient
{
    internal class SqlBooleanizer
    {
        // Methods
        internal static SqlNode Rationalize(SqlProvider sqlProvider, SqlNode node, ITypeSystemProvider typeProvider, MetaModel model)
        {
            return new Booleanizer(sqlProvider,typeProvider, model).Visit(node);
        }

        // Nested Types
        private class Booleanizer : SqlBooleanMismatchVisitor
        {
            // Fields
            private SqlFactory sql;
            private SqlProvider sqlProvider;

            // Methods
            internal Booleanizer(SqlProvider sqlProvider, ITypeSystemProvider typeProvider, MetaModel model)
            {
                this.sqlProvider = sqlProvider;
                this.sql = new SqlFactory(typeProvider, model);
                //this.sql = sqlProvider.sqlFactory;
            }

            internal override SqlExpression ConvertPredicateToValue(SqlExpression predicateExpression)
            {
                SqlExpression expression = this.sql.ValueFromObject(true, false, predicateExpression.SourceExpression);
                SqlExpression expression2 = this.sql.ValueFromObject(false, false, predicateExpression.SourceExpression);
                //if (SqlExpressionNullability.CanBeNull(predicateExpression) != false)
                if (SqlExpressionNullability.CanBeNull(predicateExpression) == true)
                {
                    return new SqlSearchedCase(predicateExpression.ClrType,
                                               new[]{ new SqlWhen(predicateExpression, expression),
                                                      new SqlWhen(new SqlUnary(SqlNodeType.Not, predicateExpression.ClrType,
                                                                        predicateExpression.SqlType, predicateExpression,
                                                                        predicateExpression.SourceExpression), expression2)
                                                   },
                                               sql.Value(expression.ClrType, expression.SqlType, null, false,
                                                              predicateExpression.SourceExpression),
                                               predicateExpression.SourceExpression);
                }
                return new SqlSearchedCase(predicateExpression.ClrType,
                                           new SqlWhen[] { new SqlWhen(predicateExpression, expression) }, expression2,
                                           predicateExpression.SourceExpression);
            }

            internal override SqlExpression ConvertValueToPredicate(SqlExpression valueExpression)
            {
                return new SqlBinary(SqlNodeType.EQ, valueExpression.ClrType, this.sql.TypeProvider.From(typeof(bool)),
                                     valueExpression,
                                     this.sql.Value(typeof(bool), valueExpression.SqlType, true, false,
                                                    valueExpression.SourceExpression));
            }
        }
    }
}