using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ALinq.Dynamic.Test.CoreTest
{
    /// <summary>
    /// 测试一些奇怪的 Selection 。
    /// </summary>
    [TestClass]
    public class SelectionTest : BaseTest
    {
        [TestMethod]
        public void SameFiles()
        {
            var esql = "select top 1 p.ProductId, p.ProductId from Products as p";
            var item = db.CreateQuery<IDataRecord>(esql).Single();
            Assert.AreEqual(2, item.FieldCount);
            Assert.IsNotNull(item[0]);
            Assert.AreEqual(item[0], item[1]);
        }
    }
}
