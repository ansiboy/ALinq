using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using ALinq.Dynamic.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ALinq.Dynamic.Test.CoreTest
{
    [TestClass]
    public class LoadTest : BaseTest
    {
        [TestMethod]
        public void DelayLoad()
        {
            var sb = new StringBuilder();
            var writer = new StringWriter(sb);
            db.Log = writer;

            string esql;
            esql = "select min(p.UnitPrice) from Products as p";
            Queryable.Min(db.Products, p => p.UnitPrice);

            //确定 Log 生效
            sb.Length = 0;
            db.CreateQuery<IDataRecord>(esql).Execute();
            db.Log.Flush();
            Assert.AreNotEqual(0, sb.Length);


            sb.Length = 0;
            db.CreateQuery<IDataRecord>(esql);
            db.Log.Flush();
            Assert.AreEqual(0, sb.Length);
        }

        [TestMethod]
        public void ImmediatelyLoad1()
        {
            var sb = new StringBuilder();
            var writer = new StringWriter(sb);
            db.Log = writer;

            string esql = "select min(p.UnitPrice) from Products as p";

            sb.Length = 0;
            db.CreateQuery<IDataRecord>(esql).Execute();
            db.Log.Flush();
            Assert.IsTrue(sb.Length > 0);
        }

        [TestMethod]
        public void ImmediatelyLoad2()
        {
            var sb = new StringBuilder();
            var writer = new StringWriter(sb);
            db.Log = writer;

            string esql = "select value min(p.UnitPrice) from Products as p";

            sb.Length = 0;
            db.CreateQuery<int>(esql).Execute();
            db.Log.Flush();
            Assert.IsTrue(sb.Length > 0);
        }

        [TestMethod]
        public void ImmediatelyLoad3()
        {
            var sb = new StringBuilder();
            var writer = new StringWriter(sb);
            db.Log = writer;

            string esql = "select value min(p.UnitPrice) from Products as p";

            sb.Length = 0;
            db.CreateQuery(esql).Execute();
            db.Log.Flush();
            Assert.IsTrue(sb.Length > 0);
        }
    }
}
