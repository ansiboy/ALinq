using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ALinq.Dynamic.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ALinq.Dynamic.Test.CoreTest
{
    [TestClass]
    public class UnaryExpressionParserTest
    {
        [TestMethod]
        public void StringLiteral()
        {
            string esql = "'Hello World'";
            var parser = CreateParser(esql);
            var expr = parser.ParseExpression();

            Console.WriteLine(expr);
            Assert.AreEqual(typeof(string), expr.Type);
            Assert.AreEqual(ExpressionType.Constant, expr.NodeType);
            Assert.AreEqual("Hello World", ((ConstantExpression)expr).Value);
        }

        [TestMethod]
        public void IntegerLiteral()
        {
            var esql = "111";
            var parser = CreateParser(esql);
            var expr = parser.ParseExpression();

            Console.WriteLine(expr);
            Assert.AreEqual(typeof(int), expr.Type);
            Assert.AreEqual(ExpressionType.Constant, expr.NodeType);
            Assert.AreEqual(111, ((ConstantExpression)expr).Value);


        }

        [TestMethod]
        public void ParseMember()
        {
            //var esql = "111 + @1";
            //var parser = CreateParser(esql);
            //var expr = parser.ParseExpression();
            //Console.WriteLine(expr);
        }

        UnaryExpressionParser CreateParser(string esql)
        {
            var tokenCursor = new TokenCursor(esql);
            var parser = new UnaryExpressionParser(tokenCursor);
            return parser;
        }
    }
}
