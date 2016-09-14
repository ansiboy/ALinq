using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ALinq.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ALinq;
using NorthwindDemo;

namespace Test
{
    [TestClass]
    public class AccessProcedureTest
    {
        [TestMethod]
        public void CreateDatabase()
        {
            var db = new ProcedureDatabase() { Log = Console.Out };
            if (db.DatabaseExists())
                db.DeleteDatabase();

            db.CreateDatabase();
        }


        [TestMethod]
        public void CreateProcedure1()
        {
            var db = new AccessNorthwind("C:/Northwind.mdb");
            //db.ExecuteCommand("Drop PROCEDURE GetCategories");
            var command = @"Create PROCEDURE GetCategories
                            AS
                            Select CategoryID, CategoryName, Description
                            From Categories";
            db.ExecuteCommand(command);
        }

        [TestMethod]
        public void GetCategories()
        {
            var db = new ProcedureDatabase { Log = Console.Out };
            var categories = db.GetCategories();
            Assert.Equals(db.Categories.Count(), categories.Count());
        }

        [TestMethod]
        public void AddCategory()
        {
            var db = new ProcedureDatabase { Log = Console.Out };
            var count = db.Categories.Count();
            db.AddCategory("name", "description");
            Assert.AreEqual(count + 1, db.Categories.Count());
        }

        [TestMethod]
        public void GetCategoryName()
        {
            var db = new ProcedureDatabase { Log = Console.Out };
            var maxID = db.Categories.Max(c => c.CategoryID);
            var category = db.Categories.Where(o => o.CategoryID == maxID).Single();
            var categoryName = db.GetCategoryName(category.CategoryID);
            Assert.AreEqual(category.CategoryName, categoryName);
        }

        [Function]
        public static int Max(int id)
        {
            throw new NotSupportedException();
        }

        [TestMethod]
        public void CreateProcedure2()
        {
            //new ProcedureDatabase().ExecuteCommand("Drop PROCEDURE AddCategory");
            var command = @"Create PROCEDURE AddCategory
                            AS
                            Insert Into [Categories]
                                   (CategoryName, Description)
                            Values ([@name], [@description])";
            new ProcedureDatabase { Log = Console.Out }.ExecuteCommand(command);
        }

        [TestMethod]
        public void CreateProcedure3()
        {
            var command = @"Create PROCEDURE GetCategoryName
                            AS
                            Select CategoryName From Categories
                            Where CategoryID = [@categoryID]";
            var db = new ProcedureDatabase();
            db.ExecuteCommand(command);
        }

        [Provider(typeof(ALinq.Access.AccessDbProvider))]
        public class ProcedureDatabase : DataContext
        {
            public ProcedureDatabase()
                : base("C:/ProcedureDatabase.mdb")
            {
            }

            public Table<Category> Categories
            {
                get { return GetTable<Category>(); }
            }

            public Table<Product> Products
            {
                get { return GetTable<Product>(); }
            }

            [Function]
            public ISingleResult<Category> GetCategories()
            {
                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod());
                return (ISingleResult<Category>)result.ReturnValue;
            }

            [Function]
            public void AddCategory(string name, string description)
            {
                ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), name, description);
            }

            [Function]
            public string GetCategoryName(int categoryID)
            {
                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), categoryID);
                return (string)result.ReturnValue;
            }
        }

        [Table(Name = "Categories")]
        public class Category
        {
            [Column(IsPrimaryKey = true, IsDbGenerated = true)]
            public int CategoryID
            {
                get;
                private set;
            }

            [Column(DbType = "VarChar(30)")]
            public string CategoryName
            {
                get;
                set;
            }

            [Column(DbType = "Text")]
            public string Description
            {
                get;
                set;
            }

            [Column(DbType = "BINARY")]
            public ALinq.Binary Picture
            {
                get;
                set;
            }
        }

        [Table(Name = "Products")]
        public class Product
        {
            [Column(IsPrimaryKey = true, IsDbGenerated = true)]
            public int ProductID
            {
                get;
                set;
            }

            [Column(DbType = "VarChar(30)")]
            public string ProductName
            {
                get;
                set;
            }
        }
    }
}
