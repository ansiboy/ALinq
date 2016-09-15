using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ALinq.Dynamic.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ALinq.Dynamic.Test.CoreTest
{
    [TestClass]
    public class ArithmeticExpressionParserTest
    {
        [TestMethod]
        public void Literal()
        {
            var esql = "1";
            var tokenCursor = new TokenCursor(esql);

            //var parser = new ArithmeticExpressionParser(tokenCursor null);
            //var expr = parser.ParseExpression();
        }
    }
}
