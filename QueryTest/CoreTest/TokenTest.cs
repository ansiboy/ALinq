using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ALinq.Dynamic.Test.CoreTest
{
    [TestClass]
    public class TokenCursorTest
    {
        [TestMethod]
        public void NextTokenTest()
        {
            string esql;
            esql = "select p from Products as p where p.ProductId Between 10 and 100";
            var tokenCursor = new TokenCursor(esql);

            var token1 = tokenCursor.Current;

            tokenCursor.NextToken();
            var token2 = tokenCursor.Current;

            Assert.AreNotEqual(token1, token2);

            Assert.AreEqual(token1.Text, "select");
            Assert.AreEqual(token2.Text, "p");
        }
    }
}
