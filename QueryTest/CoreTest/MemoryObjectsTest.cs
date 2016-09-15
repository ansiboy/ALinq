using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ALinq.Dynamic.Test.CoreTest
{
    [TestClass]
    public class MemoryObjectsTest : BaseTest
    {
        #region 集合测试
        [TestMethod]
        public void Except()
        {
            var esql = "{1,2,3} except {1,2}";
            var items = ef.CreateQuery<int>(esql).Execute(MergeOption.NoTracking).ToArray();
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual(3, items[0]);

            items = db.CreateQuery<int>(esql).ToArray();
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual(3, items[0]);

            new[] { 1, 2, 3 }.Intersect(new[] { 1, 2 });
        }

        [TestMethod]
        public void Union()
        {
            var esql = "{1,2,3} union {1,2,4}";
            var items = ef.CreateQuery<int>(esql).Execute(MergeOption.NoTracking).ToArray();
            Assert.AreEqual(4, items.Length);

            items = db.CreateQuery<int>(esql).ToArray();
            Assert.AreEqual(4, items.Length);
        }

        [TestMethod]
        public void Intersect()
        {
            var esql = "{1,2,3} Intersect {1,2}";
            var items = ef.CreateQuery<int>(esql).Execute(MergeOption.NoTracking).ToArray();
            Assert.AreEqual(2, items.Length);

            items = db.CreateQuery<int>(esql).ToArray();
            Assert.AreEqual(2, items.Length);
        }
        #endregion

        #region 查询运算符
        [TestMethod]
        public void From()
        {
            var esql = "select value o from {1,2,3} as o";
            var items = ef.CreateQuery<int>(esql).Execute(MergeOption.NoTracking).ToArray();
            Assert.AreEqual(3, items.Length);

            items = db.CreateQuery<int>(esql).ToArray();
            Assert.AreEqual(3, items.Length);
        }
        #endregion
    }
}
