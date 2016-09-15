#if DEBUG
using System;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ALinq.Dynamic.Test.CoreTest
{
    #region Person2
    public class Person2
    {
        public int EmployeeId { get; private set; }

        public string FirstName { get; private set; }

        private string LastName;
    }
    #endregion

    [TestClass]
    public class ExceptionTest : BaseTest
    {
        enum Gender
        {
            Male,
            Female
        }
        #region 參數
        [TestMethod]
        public void Parameter()
        {
            string esql;
            esql = "select p from Products as p where p.ProdcutName = @0";
            //ef.CreateQuery<object>(esql, new System.Data.Objects.ObjectParameter("0", "xxxx"));

            try
            {
                esql = "select p from Products as p where p.ProductName = @0A";
                ef.CreateQuery<object>(esql, new System.Data.Objects.ObjectParameter("0A", "xxxx"));
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }

            try
            {
                esql = "select p from Products as p where p.ProductName = @!0A";
                db.CreateQuery<object>(esql, new ObjectParameter("0A", "xxxx"));
            }
            catch (EntitySqlException exc)
            {
                Console.WriteLine(exc.Message);

                AssertAreEqual(() => Res.ParameterNameRequried, exc.ErrorName);
            }
        }

        #endregion



        [TestMethod]
        public void InvalidSelectValueList()
        {

            var esql = "select value p.ProductId, p.Name from Products as p";
            //var count = ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking).Single();
            //Console.WriteLine(count[0]);

            try
            {
                db.CreateQuery<object>(esql).Execute();
            }
            catch (EntitySqlException exc)
            {
                Console.WriteLine(exc.Message);
                Assert.AreEqual("InvalidSelectValueList", exc.ErrorName);
            }

            //esql = "select value count() from Products as p";
            //var q = db.CreateQuery<int>(esql).Execute();
            //Console.WriteLine(q.Count());
            //db.Products.Count(o => o.CategoryId == 10);
        }

        [TestMethod]
        public void InvalidGroupIdentifierReference()
        {
            string esql;
            esql = @"select OrderId, sum(d.UnitPrice)
                     from Orders as o
                          inner join OrderDetails as d on o.OrderId = d.OrderId
                     group by o.OrderId";
            ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking).ToArray();

            esql = "select count(), p.ProductId from Products as p";
            try
            {
                ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking).Single();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }

            try
            {
                db.CreateQuery<object>(esql).Execute();
            }
            catch (EntitySqlException exc)
            {
                Console.WriteLine(exc.Message);
                Assert.AreEqual("InvalidGroupIdentifierReference", exc.ErrorName);
            }

            esql = @"select d.Quantity from OrderDetails as d
                    where d.UnitPrice > 1000
                    group by d.OrderId";

            try
            {
                ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking).Single();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }

            try
            {
                db.CreateQuery<object>(esql).Execute();
            }
            catch (EntitySqlException exc)
            {
                Console.WriteLine(exc.Message);
                Assert.AreEqual("InvalidGroupIdentifierReference", exc.ErrorName);
            }

            //esql = "select value count() from Products as p";
            //var q = db.CreateQuery<int>(esql).Execute();
            //Console.WriteLine(q.Count());
            //db.Products.Count(o => o.CategoryId == 10);
        }

        [TestMethod]
        public void GroupPartitionException()
        {
            var q1 = db.Products.GroupBy(o => o.CategoryId).Select(o => new { o.Key, Products = o.Select(b => b) });
            new ExpressionVisitor().Visit(q1.Expression);

            string esql;
            esql = @"select CategoryId, GroupPartition() as Products
                     from Products as p 
                     group by p.CategoryId";

            try
            {
                ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking).ToArray();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }

            try
            {
                db.CreateQuery<object>(esql).Execute();
            }
            catch (EntitySqlException exc)
            {
                Console.WriteLine(exc.Message);
                AssertException(() => Res.GenericSyntaxError, exc);
                //Assert.AreEqual("InvalidGroupIdentifierReference", exc.ErrorName);
            }


            esql = @"select CategoryId, GroupPartition(p, 1) as Products
                     from Products as p 
                     group by p.CategoryId";

            try
            {
                ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking).ToArray();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }

            try
            {
                db.CreateQuery<object>(esql).Execute();
            }
            catch (EntitySqlException exc)
            {
                Console.WriteLine(exc.Message);
                AssertException(() => Res.GenericSyntaxError, exc);
                //Assert.AreEqual("InvalidGroupIdentifierReference", exc.ErrorName);
            }

        }


        [TestMethod]
        public void ParameterWasNotDefined()
        {
            var esql = @"select e 
                         from Employees as e
                         where e.FirstName = @f1";

            try
            {
                ef.CreateQuery<IDataRecord>(esql, new System.Data.Objects.ObjectParameter("f", "mike")).Execute(MergeOption.NoTracking).Single();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }

            try
            {
                db.CreateQuery<object>(esql, new Dynamic.ObjectParameter("f", "mike")).Execute();
            }
            catch (EntitySqlException exc)
            {
                Console.WriteLine(exc.Message);
                Assert.AreEqual("ParameterWasNotDefined", exc.ErrorName);
            }
        }

        [TestMethod]
        public void String()
        {
            var esql = "\"hello world\"";
            var item = ef.CreateQuery<object>(esql).Execute(MergeOption.NoTracking).Single();
            Console.WriteLine(item);
        }


        [TestMethod]
        public void MissType()
        {
            var esql = "select p from Products as p where p is of ";
            try
            {
                ef.CreateQuery<EF.Product>(esql).Execute(MergeOption.NoTracking);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }

            try
            {
                db.CreateQuery(esql).Execute();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }

        }

        [TestMethod]
        public void TypeError()
        {
            var esql = "select p from Products as p where p is of Pro";
            try
            {
                ef.CreateQuery<EF.Product>(esql).Execute(MergeOption.NoTracking);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }

        [TestMethod]
        public void T1()
        {
            string esql;
            esql = "1 in {1,2,3}";
            var a = ef.CreateQuery<bool>(esql).Execute(MergeOption.NoTracking).Single();
            Console.WriteLine(a);
            //var items = ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking);
            //foreach (var item in items)
            //    Console.WriteLine(item["SupplierId"]);

            //db.CreateQuery<IDataRecord>(esql).Execute();
            //var esql = "System.DateTime(2012,10,5)";
            //ef.CreateQuery<DateTime>(esql).Execute(MergeOption.NoTracking);
        }

        [TestMethod]
        public void NoCanonicalAggrFunctionOverloadMatch()
        {
            var esql = @"select value c from Products as p 
                        group by p.CategoryId as c 
                        having sum(p.ProductName) > 1000";

            try
            {
                ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.GetType().Name);
                Console.WriteLine(exc.Message);
            }

            try
            {
                db.CreateQuery<object>(esql).Execute();
            }
            catch (EntitySqlException exc)
            {
                Console.WriteLine(exc.Message);
                Assert.AreEqual("NoCanonicalAggrFunctionOverloadMatch", exc.ErrorName);
            }
        }

        [TestMethod]
        public void RequirePublicPropertySetter()
        {
            var esql = @"using ALinq.Dynamic.Test.CoreTest;
                         select value Person2{ e.FirstName } 
                         from Employees as e";

            try
            {
                db.CreateQuery<object>(esql).Execute();
            }
            catch (EntitySqlException exc)
            {
                Console.WriteLine(exc.Message);
                AssertException(() => Res.RequirePublicPropertySetter, exc);
            }

        }

        [TestMethod]
        public void NoPublicPropertyOrField()
        {
            var esql = @"using ALinq.Dynamic.Test.CoreTest;
                         select value Person2{ e.LastName } 
                         from Employees as e";

            EntitySqlException e = null;

            try
            {
                db.CreateQuery<object>(esql).Execute();
            }
            catch (EntitySqlException exc)
            {
                Console.WriteLine(exc.Message);
                e = exc;
            }

            Assert.IsNotNull(e);
            AssertException(() => Res.NoPublicPropertyOrField, e);

        }

        #region 日期函數
        [TestMethod]
        public void Day1()
        {
            string esql;
            EntitySqlException e = null;

            esql = "Day()";
            try
            {
                db.CreateQuery(esql);
            }
            catch (EntitySqlException exc)
            {
                Console.WriteLine(exc.Message);
                e = exc;
            }

            Assert.IsNotNull(e);
            AssertException(() => Res.NoCanonicalFunctionOverloadMatch, e);

        }

        [TestMethod]
        public void Day2()
        {
            string esql;
            EntitySqlException e = null;

            esql = "Day(2012)";
            try
            {
                db.CreateQuery(esql);
            }
            catch (EntitySqlException exc)
            {
                Console.WriteLine(exc.Message);
                e = exc;
            }

            Assert.IsNotNull(e);
            AssertException(() => Res.NoCanonicalFunctionOverloadMatch, e);
        }
        #endregion

        [TestMethod]
        public void ExpressionTypeMustBeBoolean()
        {
            string esql = @"SELECT e.FirstName, e.LastName 
                            From Employees as e
                            Where e.FirstName + 'xxxx'";
            try
            {
                ef.CreateQuery<object>(esql).Execute(MergeOption.NoTracking);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.GetType().Name);
                Console.WriteLine(exc.Message);
            }

            try
            {
                db.CreateQuery<object>(esql).Execute();
            }
            catch (EntitySqlException exc)
            {
                Assert.AreEqual("ExpressionTypeMustBeBoolean", exc.ErrorName);
                Console.WriteLine(exc.Message);
            }
        }

        [TestMethod]
        public void GenericSyntaxError1()
        {
            string esql = @"SELECT e.FirstName, e.LastName 
                            From Employees as e
                            Where ";

            try
            {
                ef.CreateQuery<object>(esql).Execute(MergeOption.NoTracking);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.GetType().Name);
                Console.WriteLine(exc.Message);
            }

            Exception e = null;
            try
            {
                db.CreateQuery<object>(esql).Execute();
            }
            catch (EntitySqlException exc)
            {
                Assert.AreEqual("GenericSyntaxError", exc.ErrorName);
                Console.WriteLine(exc.Message);
                e = exc;
            }
            Assert.IsNotNull(e);
        }


        [TestMethod]
        public void GenericSyntaxError2()
        {
            string esql = @"SELECT e.FirstName, e.LastName 
                            From Employees as e
                            ,Where ";

            try
            {
                ef.CreateQuery<object>(esql).Execute(MergeOption.NoTracking);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.GetType().Name);
                Console.WriteLine(exc.Message);
            }

            try
            {
                db.CreateQuery<object>(esql).Execute();
            }
            catch (EntitySqlException exc)
            {
                Assert.AreEqual("ADP_KeywordNotSupported", exc.ErrorName);
                Console.WriteLine(exc.Message);
            }
        }

        [TestMethod]
        public void GenericSyntaxError()
        {
            string esql = @"SELECT 
                            From Employees as e";

            try
            {
                ef.CreateQuery<object>(esql).Execute(MergeOption.NoTracking);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.GetType().Name);
                Console.WriteLine(exc.Message);
            }

            try
            {
                db.CreateQuery<object>(esql).Execute();
            }
            catch (EntitySqlException exc)
            {
                Assert.AreEqual("GenericSyntaxError", exc.ErrorName);
                Console.WriteLine(exc.Message);
            }
        }

        [TestMethod]
        public void GenericSyntaxError4()
        {
            string esql = @"SELEC 
                            From Employees as e";

            try
            {
                ef.CreateQuery<object>(esql).Execute(MergeOption.NoTracking);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.GetType().Name);
                Console.WriteLine(exc.Message);
            }

            try
            {
                db.CreateQuery<object>(esql).Execute();
            }
            catch (EntitySqlException exc)
            {
                Assert.AreEqual("CouldNotResolveIdentifier", exc.ErrorName);
                Console.WriteLine(exc.Message);
            }
        }

        [TestMethod]
        public void GenericSyntaxError5()
        {
            string esql = @"'AAA' as FistName";

            try
            {
                ef.CreateQuery<object>(esql).Execute(MergeOption.NoTracking);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.GetType().Name);
                Console.WriteLine(exc.Message);
            }

            try
            {
                db.CreateQuery<object>(esql).Execute();
            }
            catch (EntitySqlException exc)
            {
                Assert.AreEqual("GenericSyntaxError", exc.ErrorName);
                Console.WriteLine(exc.Message);
            }
        }

        [TestMethod]
        public void GenericSyntaxError3()
        {
            string esql = @"SELEC ";

            try
            {
                ef.CreateQuery<object>(esql).Execute(MergeOption.NoTracking);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.GetType().Name);
                Console.WriteLine(exc.Message);
            }

            try
            {
                db.CreateQuery<object>(esql).Execute();
            }
            catch (EntitySqlException exc)
            {
                //Assert.AreEqual("GenericSyntaxError", exc.ErrorName);
                Console.WriteLine(exc.Message);
            }
        }

        [TestMethod]
        public void InExpressionMustBeCollection()
        {
            var esql = "select o from Orders as o where o.OrderId in 5 ";
            try
            {
                ef.CreateQuery<EF.Order>(esql).Execute(MergeOption.NoTracking);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }


            try
            {
                db.CreateQuery<object>(esql).Execute();
            }
            catch (EntitySqlException exc)
            {
                Console.WriteLine(exc.Message);
                AssertAreEqual(() => Res.InExpressionMustBeCollection, exc.ErrorName);
            }
        }

        [TestMethod]
        public void BetweenMissAnd()
        {
            var esql = "select p from Products as p where p.ProductId between 10";
            try
            {
                ef.CreateQuery<EF.Product>(esql).Execute(MergeOption.NoTracking);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }

            Exception e = null;
            try
            {
                db.CreateQuery<NorthwindDemo.Product>(esql).Execute();
            }
            catch (EntitySqlException exc)
            {
                Console.WriteLine(exc.Message);
                AssertAreEqual(() => Res.TokenExpected, exc.ErrorName);
                e = exc;
            }
            Assert.IsNotNull(e);

        }

        [TestMethod]
        public void DuplicateParameterName()
        {
            var esql = "select p from Products as p where p.ProductId = @pid and p.CategoryId = @cid";
            try
            {
                ef.CreateQuery<EF.Product>(esql, new System.Data.Objects.ObjectParameter("pid", 10),
                                           new System.Data.Objects.ObjectParameter("pid", 10),
                                           new System.Data.Objects.ObjectParameter("cid", 10))
                    .Execute(MergeOption.NoTracking);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }

            try
            {
                db.CreateQuery<NorthwindDemo.Product>(esql, new ObjectParameter("pid", 10),
                                           new ObjectParameter("pid", 10),
                                           new ObjectParameter("cid", 10)).Execute();
            }
            catch (EntitySqlException exc)
            {
                Console.WriteLine(exc.Message);
                AssertAreEqual(() => Res.ObjectParameterCollection_DuplicateParameterName, exc.ErrorName);
            }
        }

        [TestMethod]
        public void InvalidQueryCast()
        {
            try
            {
                var esql = "select e from Employees as e";
                var q = db.CreateQuery<IDataRecord>(esql, employees);
                q.Execute();
            }
            catch (EntitySqlException exc)
            {
                Console.WriteLine(exc.Message);
                AssertAreEqual(() => Res.InvalidQueryCast, exc.ErrorName);
            }

        }

        [TestMethod]
        public void AliasNameAlreadyUsed1()
        {
            var esql = "select p.ProductId, p.ProductName as ProductId from Products as p";
            try
            {
                var q = ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking).ToArray();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }

            Exception e = null;
            try
            {
                var q = db.CreateQuery<IDataRecord>(esql).Execute().ToArray();
            }
            catch (EntitySqlException exc)
            {
                Console.WriteLine(exc.Message);
                AssertAreEqual(() => Res.AliasNameAlreadyUsed, exc.ErrorName);
                e = exc;
            }
            Assert.IsNotNull(e);
        }

        [TestMethod]
        public void AliasNameAlreadyUsed2()
        {
            var esql = "select p as ProductId from Products as p, Categories as p";
            try
            {
                var q = ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking).ToArray();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }

            Exception e = null;
            try
            {
                var q = db.CreateQuery<IDataRecord>(esql).Execute().ToArray();
            }
            catch (EntitySqlException exc)
            {
                Console.WriteLine(exc.Message);
                AssertAreEqual(() => Res.AliasNameAlreadyUsed, exc.ErrorName);
                e = exc;
            }
            Assert.IsNotNull(e);
        }

        [TestMethod]
        public void ParseBinaryError()
        {
            Exception e = null;
            try
            {
                var esql = "x'00ffaabb'";
                var bytes = db.CreateQuery<byte[]>(esql).Single();
                Assert.IsTrue(bytes.Length > 0);
            }
            catch (EntitySqlException exc)
            {
                e = exc;
            }
            Assert.IsNotNull(e);
        }

        #region 分组
        [TestMethod]
        public void GroupSum1()
        {
            var esql = "select o.OrderId, sum(o.UnitPrice) from OrderDetails as o";
            Exception ee = null;
            try
            {
                ef.CreateQuery<IDataRecord>(esql).Execute(MergeOption.NoTracking);
            }
            catch (Exception exc)
            {
                ee = exc;
                Console.WriteLine(exc.Message);
            }
            Assert.IsNotNull(ee);

            EntitySqlException e = null;
            try
            {
                db.CreateQuery(esql).Execute();
            }
            catch (EntitySqlException exc)
            {
                e = exc;
                Console.WriteLine(exc.Message);
            }

            Assert.IsNotNull(e);
            AssertAreEqual(() => Res.InvalidGroupIdentifierReference, e.ErrorName);
        }
        #endregion

        void AssertAreEqual(Expression<Func<string>> expr, string errorName)
        {
            var expected = ErrorName(expr);
            var actual = errorName;
            Assert.AreEqual(expected, actual);
        }

        public string ErrorName(Expression<Func<string>> expr)
        {
            var m = (MemberExpression)expr.Body;
            return m.Member.Name;
        }
    }
}
#endif