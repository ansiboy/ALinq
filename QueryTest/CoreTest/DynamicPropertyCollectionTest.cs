using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ALinq.Dynamic.Test.CoreTest
{
    [TestClass]
    public class DynamicPropertyCollectionTest
    {
        [TestMethod]
        public void Add()
        {
            var c = new DynamicPropertyCollection();

            Assert.IsTrue(string.Compare("A", "B") < 0);
            Assert.IsTrue(string.Compare("B", "C") < 0);

            //以 A 打头
            c.Add(new DynamicProperty("A", typeof(string)));
            c.Add(new DynamicProperty("B", typeof(string)));
            c.Add(new DynamicProperty("C", typeof(string)));

            AssertSorted(c);

            c = new DynamicPropertyCollection();
            c.Add(new DynamicProperty("A", typeof(string)));
            c.Add(new DynamicProperty("C", typeof(string)));
            c.Add(new DynamicProperty("B", typeof(string)));

            //===================================

            //以 B 打头
            c = new DynamicPropertyCollection();
            c.Add(new DynamicProperty("B", typeof(string)));
            c.Add(new DynamicProperty("A", typeof(string)));
            c.Add(new DynamicProperty("C", typeof(string)));

            AssertSorted(c);

            c = new DynamicPropertyCollection();
            c.Add(new DynamicProperty("B", typeof(string)));
            c.Add(new DynamicProperty("C", typeof(string)));
            c.Add(new DynamicProperty("A", typeof(string)));

            AssertSorted(c);

            //=============================================

            //以 C 打头
            c = new DynamicPropertyCollection();
            c.Add(new DynamicProperty("C", typeof(string)));
            c.Add(new DynamicProperty("B", typeof(string)));
            c.Add(new DynamicProperty("A", typeof(string)));

            AssertSorted(c);

            c = new DynamicPropertyCollection();
            c.Add(new DynamicProperty("C", typeof(string)));
            c.Add(new DynamicProperty("A", typeof(string)));
            c.Add(new DynamicProperty("B", typeof(string)));

            AssertSorted(c);

        }

        void AssertSorted(DynamicPropertyCollection c)
        {
            var items = c.ToArray();
            Assert.AreEqual(items[0].Name, "A");
            Assert.AreEqual(items[1].Name, "B");
            Assert.AreEqual(items[2].Name, "C");
        }


    }
}
