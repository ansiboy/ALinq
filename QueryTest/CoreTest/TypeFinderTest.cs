using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ALinq.Dynamic.Test
{
    [TestClass]
    public class TypeFinderTest : BaseTest
    {
        [TestMethod]
        public void FindType1()
        {
            var typeFinder = new TypeFinder();
            var type = typeFinder.FindType("NorthwindDemo.Product", new string[] { });
            Assert.IsNotNull(type);
        }

        [TestMethod]
        public void FindType2()
        {
            var typeFinder = new TypeFinder();

            EntitySqlException exc = null;
            try
            {
                typeFinder.FindType("Product", new string[] { });

            }
            catch (EntitySqlException e)
            {
                exc = e;
            }

            Assert.IsNotNull(exc);
            AssertException(() => Res.AmbiguousTypeReference, exc);

            //type = typeFinder.FindType("Product");
        }
    }
}
