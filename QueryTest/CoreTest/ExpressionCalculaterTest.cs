#if L2S
using ALinq.Dynamic.Test.L2S;
#else
using System.Data;
using NorthwindDemo;
#endif
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ALinq.Dynamic.Parsers;

namespace ALinq.Dynamic.Test
{
    [TestClass]
    public class ExpressionCalculaterTest
    {


        Expression CreateExpression(string esql)
        {
            var expressionParser = new QueryParser(esql);
            var expr = expressionParser.ParseExpression();
            return expr;
        }

        object Execute(string esql)
        {
            var expressionParser = new QueryParser(esql);
            var expr = expressionParser.ParseExpression();
            var provider = new ExpressionQueryProvider();
            return provider.Execute(expr);
        }

        #region 邏輯運算測試
        [TestMethod]
        public void StringContactParseTest()
        {
            var esql = "'AAA' + 'BBB'";
            var expr = CreateExpression(esql);
            var value = ExpressionCalculater.Eval(expr);
            Assert.AreEqual("AAABBB", value);

            esql = "'AAA' + 'BBB' + 'CCC'";
            expr = CreateExpression(esql);
            value = ExpressionCalculater.Eval(expr);

            Assert.AreEqual("AAABBBCCC", value);
        }

        [TestMethod]
        public void IntArithmeticTest()
        {
            var esql = "10 + 5";
            var value = Execute(esql);

            Assert.AreEqual(10 + 5, value);

            esql = "10 + 5 - 4";
            value = Execute(esql);
            Assert.AreEqual(10 + 5 - 4, value);

            esql = "10 + (5 - 4)";
            value = Execute(esql);
            Assert.AreEqual(10 + (5 - 4), value);

            esql = "10 * 5";
            value = Execute(esql);
            Assert.AreEqual(10 * 5, value);

            esql = "10 / 5";
            value = Execute(esql);
            Assert.AreEqual(10 / 5, value);

            esql = "10 * 5 - 4";
            value = Execute(esql);
            Assert.AreEqual(10 * 5 - 4, value);

            esql = "10 * (5 - 4)";
            value = Execute(esql);
            Assert.AreEqual(10 * (5 - 4), value);
        }

        #endregion

        #region Literal
        [TestMethod]
        public void String()
        {
            Expression<Func<string>> expr = () => "hello world!";

            var value = ExpressionCalculater.Eval(expr);
            Assert.AreEqual("hello world!", value);


        }

        [TestMethod]
        public void Interger()
        {
            Expression<Func<int>> expr = () => 5;
            Console.WriteLine(expr);

            var value = ExpressionCalculater.Eval(expr);
            Assert.AreEqual(5, value);

            expr = () => -5;
            Console.WriteLine(expr);

            value = ExpressionCalculater.Eval(expr);
            Assert.AreEqual(-5, value);
        }

        [TestMethod]
        public void Boolean()
        {
            Expression<Func<bool>> expr = () => true;
            var value = ExpressionCalculater.Eval(expr);
            Assert.AreEqual(true, value);

            expr = () => false;
            value = ExpressionCalculater.Eval(expr);
            Assert.AreEqual(false, value);
        }

        [TestMethod]
        public void Datetime()
        {
            Expression<Func<DateTime>> expr = () => DateTime.Parse("2012-5-25");
            var value = ExpressionCalculater.Eval(expr);
            Assert.AreEqual(new DateTime(2012, 5, 25), value);
        }

        #endregion

        [TestMethod]
        public void MemberAccess()
        {
            var esql = "@e.FirstName";
            var employee = new Employee { FirstName = "Mike" };
            var p = new ObjectParameter("e", employee);
            var expressionParser = new QueryParser(esql, new[] { p });
            var expr = expressionParser.ParseExpression();
            Console.WriteLine(expr);




            var value = ExpressionCalculater.Eval(expr, new ObjectParameter("e", employee));
            Assert.AreEqual("Mike", value);
        }

        [TestMethod]
        public void New()
        {

            Expression<Func<Employee, object>> expr = e => new { e.FirstName, e.LastName };
            Console.WriteLine(expr);

            var employee = new Employee { FirstName = "Mike", LastName = "Mak" };
            var value = ExpressionCalculater.Eval(expr, new ObjectParameter("e", employee)) as dynamic;

            Assert.IsNotNull(value);
            Assert.AreEqual(employee.FirstName, value.FirstName);
            Assert.AreEqual(employee.LastName, value.LastName);
        }

        [TestMethod]
        public void Row1()
        {
            var employee = new Employee { FirstName = "Mike", LastName = "Mak" };
            var esql = "row(FirstName, LastName)";
            //var expressionParser = new QueryParser(employee, esql);
            var parser = new QueryParser(employee, esql);
            var expr = parser.ParseExpression();
            Console.WriteLine(expr);

            var a = ExpressionCalculater.Eval(expr, new ObjectParameter("", employee));
            var value = a as IDataRecord;

            Assert.IsNotNull(value);
            Assert.AreEqual(employee.FirstName, value["FirstName"]);
            Assert.AreEqual(employee.LastName, value["LastName"]);
        }

        [TestMethod]
        public void Row2()
        {

            var employee = new Employee { FirstName = "Mike", LastName = "Mak" };
            var esql = "row(@e.FirstName as F, @e.LastName as L)";

            //var expressionParser = new QueryParser(employee, esql);
            var parser = new QueryParser(esql, new ObjectParameter("e", employee));
            var expr = parser.ParseExpression();
            Console.WriteLine(expr);

            var record = ExpressionCalculater.Eval(expr, new ObjectParameter("", employee)) as IDataRecord;

            Assert.IsNotNull(record);
            Assert.AreEqual(employee.FirstName, record["F"]);
            Assert.AreEqual(employee.LastName, record["L"]);

            var value = record as dynamic;
            Assert.AreEqual(employee.FirstName, value.F);
            Assert.AreEqual(employee.LastName, value.L);

        }

        [TestMethod]
        public void Multiset()
        {
            var esql = "MULTISET(1, 2, 3)";
            var expressionParser = new QueryParser(esql);
            var expr = expressionParser.ParseExpression();
            Console.WriteLine(expr);

            esql = "{1, 2, 3}";
            expressionParser = new QueryParser(esql);
            expr = expressionParser.ParseExpression();
            Console.WriteLine(expr);
        }

        [TestMethod]
        public void Temp()
        {
            var esql = "5 > 2 ? 'A' : 'B' ";
            var parser = new QueryParser(esql);
            var expr = parser.Parse();
            var a = expr.Execute();
        }


    }
}
