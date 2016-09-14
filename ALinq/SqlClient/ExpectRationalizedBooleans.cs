namespace ALinq.SqlClient
{
    internal class ExpectRationalizedBooleans : SqlBooleanMismatchVisitor
    {
        // Methods
        internal ExpectRationalizedBooleans()
        {
        }

        internal override SqlExpression ConvertPredicateToValue(SqlExpression predicateExpression)
        {
            throw Error.ExpectedBitFoundPredicate();
        }

        internal override SqlExpression ConvertValueToPredicate(SqlExpression bitExpression)
        {
            throw Error.ExpectedPredicateFoundBit();
        }
    }
}