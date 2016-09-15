#if L2S
using ALinq.Dynamic.Test.L2S;
#else
using System.Data;
using System.Data.Objects;
using System.Linq;
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
    public partial class ExpressionParserTest : BaseTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 單值測試
        [TestMethod()]
        public void StringParseTest()
        {
            var esql = "'AAA'";
            db.CreateQuery<string>(esql).Execute();
            //var obj2 = ef.CreateQuery<string>(esql).Execute(MergeOption.NoTracking).First();
            var obj1 = db.CreateQuery<string>(esql).Execute().First();
            //db.CreateQuery<string>(esql).First();
            //var q = new[] { "AAA", "BBB" }.AsQueryable<string>();
            //var value = q.Provider.Execute(q.Expression);
            //Console.WriteLine(q.GetType().Name);
            //Console.WriteLine(q.First());

            //Assert.AreEqual(obj1, obj2);
        }

        [TestMethod]
        public void IntParseTest()
        {
            var esql = "10";
            var obj1 = db.CreateQuery<int>(esql).Execute().First();

            //Assert.AreEqual(obj1, obj2);
        }
        #endregion

        #region 關鍵字測試
        [TestMethod]
        public void RowTest()
        {
            var esql = "Row('AAA' as A, 'BBB' as B)";

            var obj1 = db.CreateQuery<IDataRecord>(esql).First();

            Assert.IsNotNull(obj1);

            Assert.AreEqual("AAA", obj1["A"]);
            Assert.AreEqual("BBB", obj1["B"]);

        }

        [TestMethod]
        public void Table()
        {
            var esql = "Orders";
            //var items1 = ef.CreateQuery<EF.Order>(esql).Execute(MergeOption.NoTracking);

            //esql = "From Orders";
            //var items2 = ef.CreateQuery<EF.Order>(esql).Execute(MergeOption.NoTracking);

        }

        [TestMethod]
        public void MultiSet1()
        {
            var esql = "MULTISET(1, 2, 3)";
            var q2 = db.CreateQuery<int>(esql);
            foreach (var item in q2)
                Console.WriteLine(item);

        }

        [TestMethod]
        public void MultiSet2()
        {
            var esql = "{1, 2, 3}";
            var q2 = db.CreateQuery<int>(esql);
            foreach (var item in q2)
                Console.WriteLine(item);
        }

        [TestMethod]
        public void ValueTest()
        {
            //var value1 = ef.CreateQuery<string>("Select Value e.FirstName From NorthwindEntities.Employees as e").Execute(MergeOption.NoTracking).First();
        }
        #endregion

        #region 邏輯運算測試
        [TestMethod]
        public void StringContactParseTest()
        {
            var queryText = "'AAA' + 'BBB'";
            var value1 = db.CreateQuery<string>(queryText).Execute().First(); //ExpressionHelper.CalculateValue(expr);
            var value2 = ef.CreateQuery<string>(queryText).Execute(MergeOption.NoTracking).First();
            Assert.AreEqual(value1, value2);

            queryText = "'AAA' + 'BBB' + 'CCC'";
            value1 = db.CreateQuery<string>(queryText).Execute().First();
            value2 = ef.CreateQuery<string>(queryText).Execute(MergeOption.NoTracking).First();
            Assert.AreEqual(value1, value2);
        }

        [TestMethod]
        public void IntArithmeticTest()
        {
            var queryText = "10 + 5";
            var value1 = db.CreateQuery<int>(queryText).Execute().First();
            var value2 = ef.CreateQuery<int>(queryText).Execute(MergeOption.NoTracking).First();
            Assert.AreEqual(value1, value2);

            queryText = "10 + 5 - 4";
            value1 = db.CreateQuery<int>(queryText).Execute().First();
            value2 = ef.CreateQuery<int>(queryText).Execute(MergeOption.NoTracking).First();
            Assert.AreEqual(value1, value2);

            queryText = "10 + (5 - 4)";
            value1 = db.CreateQuery<int>(queryText).Execute().First();
            value2 = ef.CreateQuery<int>(queryText).Execute(MergeOption.NoTracking).First();
            Assert.AreEqual(value1, value2);

            queryText = "10 * 5";
            value1 = db.CreateQuery<int>(queryText).Execute().First();
            value2 = ef.CreateQuery<int>(queryText).Execute(MergeOption.NoTracking).First();
            Assert.AreEqual(value1, value2);

            queryText = "10 / 5";
            value1 = db.CreateQuery<int>(queryText).Execute().First();
            value2 = ef.CreateQuery<int>(queryText).Execute(MergeOption.NoTracking).First();
            Assert.AreEqual(value1, value2);

            queryText = "10 * 5 - 4";
            value1 = db.CreateQuery<int>(queryText).Execute().First();
            value2 = ef.CreateQuery<int>(queryText).Execute(MergeOption.NoTracking).First();
            Assert.AreEqual(value1, value2);

            queryText = "10 * (5 - 4)";
            value1 = db.CreateQuery<int>(queryText).Execute().First();
            value2 = ef.CreateQuery<int>(queryText).Execute(MergeOption.NoTracking).First();
            Assert.AreEqual(value1, value2);
        }
        #endregion

        [TestMethod]
        public void Exists1()
        {
            IQueryable<int> items = new[] { 1, 2, 3, 4, 5 }.AsQueryable();
            var esql = "exists(select i from @0 as i where i > 2)";
            var q = db.CreateQuery(esql, items).Execute();
            //foreach (var i in q)
            //    Console.WriteLine(i);

           
        }

        [TestMethod]
        public void Exists2()
        {
            IQueryable<int> items = new[] { 1, 2, 3, 4, 5 }.AsQueryable();
            var esql = "select i from @0 as i";
            var q = db.CreateQuery(esql, items).Execute();
            foreach (var i in q)
                Console.WriteLine(i);
        }

        [TestMethod]
        public void Exists3()
        {
            IQueryable<int> items = new[] { 1, 2, 3, 4, 5 }.AsQueryable();
            var esql = "exists(select value i from @0 as i where i > 2)";
            var q = db.CreateQuery(esql, items).Execute();
            //foreach (var i in q)
            //    Console.WriteLine(i);
        }

    }
}
