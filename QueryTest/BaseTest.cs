using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
#if FULL_TEST
using ALinq.Dynamic.Test.EF;
#endif
using Microsoft.VisualStudio.TestTools.UnitTesting;

#if L2S
using System.Data.Linq;
using Employee = NorthwindDemo.Employee;
using Product = NorthwindDemo.Product;
#else
using NorthwindDemo;
using Employee = NorthwindDemo.Employee;
using Product = NorthwindDemo.Product;
#endif

namespace ALinq.Dynamic.Test
{
    [TestClass]
    public abstract class BaseTest
    {
#if L2S
        protected NorthwindDemo.NorthwindDataContext db = new NorthwindDemo.NorthwindDataContext(ConfigurationManager.ConnectionStrings["sqlceDB"].ConnectionString);
#else
        protected NorthwindDataContext db = new NorthwindDataContext(SQLiteDbConnectionString);
#endif
        protected Table<Employee> employees;
        protected Table<Product> products;

        static string SQLiteDbConnectionString
        {
            get
            {

                var dllPath = typeof(BaseTest).Assembly.Location;
                var fileInfo = new System.IO.FileInfo(dllPath);
                var dbPath = System.IO.Path.Combine(fileInfo.Directory.FullName, "Northwind.db3");
                var conn = string.Format("data source={0}", dbPath); //@"data source=c:\Northwind.db3";
                return conn;
            }
        }
#if FULL_TEST
        protected NorthwindEntities ef = new NorthwindEntities();
        internal void AssertException(Expression<Func<string>> msgExpr, EntitySqlException exc)
        {
            var memberExpr = (MemberExpression)msgExpr.Body;
            var member = (PropertyInfo)memberExpr.Member;
            var errorName = member.Name;
            Assert.AreEqual(errorName, exc.ErrorName);
        }
#endif
        [TestInitialize]
        public void TestInitialize()
        {
            //var xmlMapping = XmlMappingSource.FromStream(typeof(SQLiteTest).Assembly.GetManifestResourceStream("Test.Northwind.Access.map"));
            //db = new AccessNorthwind("C:/Northwind.mdb");
            //db = new AccessNorthwind("C:/Northwind.mdb", xmlMapping);
            //db = new SQLiteNorthwind("C:/Northwind.db3");
            db.Log = Console.Out;
            employees = db.GetTable<Employee>();
            products = db.GetTable<Product>();

        }


    }
}
