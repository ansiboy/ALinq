using System;
using System.Linq.Expressions;
using ALinq.SqlClient;
using System.Diagnostics;

namespace ALinq.SQLite
{
    class SQLitePostBindDotNetConverter : PostBindDotNetConverter
    {
        public SQLitePostBindDotNetConverter(SqlFactory sqlFactory)
            : base(sqlFactory)
        {
        }

        internal override SqlVisitor CreateVisitor(SqlFactory factory, SqlProvider sqlProvider)
        {
            return new MyVisitor(factory, sqlProvider);
        }

        internal override string GetDatePart(string memberName)
        {
            switch (memberName)
            {
                case "Year":
                case "Month":
                case "Day":
                case "DayOfYear":
                case "Hour":
                case "Minute":
                case "Second":
                //case "Millisecond":
                    return memberName;
            }
            return null;
        }

        internal override MethodSupport GetMethodSupport(SqlMethodCall mc)
        {
            if (mc.Object != null)
            {
                if (mc.Object.ClrType == typeof(DateTime))
                {
                    switch (mc.Method.Name)
                    {
                        case "Add":
                        //case "AddDays":
                        case "AddHours":
                        case "AddMinutes":
                        case "AddMonths":
                        case "AddSeconds":
                            //case "AddYears":
                            return MethodSupport.None;
                    }
                }
            }
            if (mc.Method.Name == "Truncate")
                return MethodSupport.None;
            return base.GetMethodSupport(mc);
        }

        private class MyVisitor : Visitor
        {
            public MyVisitor(SqlFactory sql, SqlProvider sqlProvider)
                : base(sql, sqlProvider)
            {
            }

            protected override SqlExpression TranslateStringInsertMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                var clrType = mc.Method.ReturnType;
                var startIndex = (int)((SqlValue)mc.Arguments[0]).Value;
                var insertString = (string)((SqlValue)mc.Arguments[1]).Value;

                Debug.Assert(clrType == typeof(string));
                Debug.Assert(startIndex >= 0);

                var arg1 = mc.Object;//VisitExpression(mc.Object);
                var arg2 = sql.ValueFromObject(startIndex, sourceExpression);
                var left = new SqlFunctionCall(clrType, sql.TypeProvider.From(clrType), "leftstr",
                                               new[] { arg1, arg2 }, sourceExpression);
                //return left;
                var result = new SqlBinary(SqlNodeType.Add, typeof(string), sql.TypeProvider.From(typeof(string)),
                                           left, sql.ValueFromObject(insertString, sourceExpression));

                var len = new SqlFunctionCall(typeof(int), sql.TypeProvider.From(typeof(int)), "length",
                                              new[] { VisitExpression(mc.Object) }, sourceExpression);
                var binary = new SqlBinary(SqlNodeType.Sub, typeof(int), sql.TypeProvider.From(typeof(int)), len, arg2);
                var right = new SqlFunctionCall(typeof(string), sql.TypeProvider.From(typeof(string)), "rightstr",
                                                new[] { arg1, binary }, sourceExpression);
                result = new SqlBinary(SqlNodeType.Add, typeof(string), sql.TypeProvider.From(typeof(string)), result, right);
                return result;
            }

            protected override SqlExpression TranslateStringGetCharsMethod(SqlMethodCall mc, Expression sourceExpression)
            {
                if (mc.Arguments.Count != 1)
                {
                    throw GetMethodSupportException(mc);
                }
                return sql.FunctionCall(typeof(char), "SUBSTR", new[] { mc.Object, sql.Add(mc.Arguments[0], 1), 
                                        sql.ValueFromObject(1, false, sourceExpression) }, sourceExpression);
            }
        }


    }
}
