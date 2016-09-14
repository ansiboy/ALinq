namespace ALinq.SqlClient
{
    class MsSqlFormatter : SqlFormatter
    {
        public MsSqlFormatter(SqlProvider sqlProvider)
            : base(sqlProvider)
        {

        }

        internal override Visitor CreateVisitor()
        {
            return new MyVisitor();
        }

        class MyVisitor : SqlFormatter.Visitor
        {
            internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
            {
                //if (uo.NodeType == SqlNodeType.Convert)
                //{
                //    base.Visit(uo.Operand);
                //    return uo;
                //}
                if (uo.NodeType == SqlNodeType.Min || uo.NodeType == SqlNodeType.Max ||
                    uo.NodeType == SqlNodeType.Avg || uo.NodeType == SqlNodeType.Sum)
                {
                    //if (!TypeSystem.IsNullableType(uo.ClrType))
                    //{
                    //IFNULL
                    sb.Append("ISNULL(");
                    sb.Append(GetOperator(uo.NodeType));
                    sb.Append("(");
                    if (uo.Operand == null)
                        sb.Append("*");
                    else
                        Visit(uo.Operand);
                    sb.Append("),0)");
                    return uo;
                    //}
                }
                return base.VisitUnaryOperator(uo);
            }
        }
    }
}