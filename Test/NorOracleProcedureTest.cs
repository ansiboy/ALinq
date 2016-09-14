using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ALinq;
using ALinq.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.DataAccess.Types;
using Test.Properties;

namespace Test
{
    [TestClass]
    public class NorOracleProcedureTest
    {
        [TestMethod]
        public void AddCategory()
        {
            var db = new ProcedureDatabase();
            var id = db.Categories.Max(o => o.CategoryID) + 1;
            db.AddCategory(id, "Name" + id, "Description" + id);
        }

        [TestMethod]
        public void GetCategoryName()
        {
            var db = new ProcedureDatabase();
            var id = db.Categories.Max(o => o.CategoryID);
            string name;
            db.GetCategoryName(id, out name);
            Assert.AreEqual(db.Categories.Where(o => o.CategoryID == id).Single().CategoryName, name);
        }

        //[TestMethod]
        //public void GetCategoryPicture1()
        //{
        //    var db = new ProcedureDatabase();
        //    var id = 2;

        //    db.Connection.Open();
        //    var pic = db.GetCategoryPicture(id);
        //    db.Connection.Close();
        //    Assert.IsNotNull(pic);
        //}

        //[TestMethod]
        //public void GetCategoryPicture2()
        //{
        //    var db = new ProcedureDatabase();

        //    db.Connection.Open();
        //    var pics = db.Categories.Select(o => GetCategoryPicture(o.CategoryID)).ToArray();
        //    db.Connection.Close();
        //}

        //[TestMethod]
        //public void GetCategoryPicture3()
        //{
        //    var db = new ProcedureDatabase();
        //    var id = 2;
        //    db.Connection.Open();
        //    var pic = db.DUAL.Select(o => GetCategoryPicture(id)).Single();
        //    db.Connection.Close();
        //}

        [TestMethod]
        public void GetCategoriesCount()
        {
            var db = new ProcedureDatabase();
            int count;
            db.GetCategoriesCount(out count);
            Assert.AreEqual(db.Categories.Count(), count);
        }

        [Function(Name = "GET_CATEGORY_PIC")]
        static byte[] GetCategoryPicture(int id)
        {
            throw new NotSupportedException("Could not call direct.");
        }

        [TestMethod]
        public void Test()
        {

        }


        [TestMethod]
        public void GetAllCategories()
        {
            var db = new ProcedureDatabase();
            object myrc;
            var items = db.GetAllCategories(out myrc);
            Assert.AreEqual(db.Categories.Count(), items.Count());
        }

        [TestMethod]
        public void GetCategoriesAndProducts()
        {
            var db = new ProcedureDatabase();
            object myrc1;
            object myrc2;
            var result = db.GetCategoriesAndProducts(out myrc1, out myrc2);
            var categories = result.GetResult<Category>();
            Assert.AreEqual(db.Categories.Count(), categories.Count());

            var products = result.GetResult<Product>();
            Assert.AreEqual(db.Products.Count(), products.Count());
        }


        [Provider(typeof(ALinq.Oracle.OracleProvider))]
        public class ProcedureDatabase : DataContext
        {
            public ProcedureDatabase()
                : base("DATA SOURCE=vpc1;PERSIST SECURITY INFO=True;USER ID=PROCEDUREDATABASE;PASSWORD=TEST")
            {
                Log = Console.Out;
            }

            public Table<Category> Categories
            {
                get { return GetTable<Category>(); }
            }

            public Table<Product> Products
            {
                get { return GetTable<Product>(); }
            }


            public Table<Dual> DUAL
            {
                get { return GetTable<Dual>(); }
            }

            [Function(Name = "ADD_CATEGORY")]
            public void AddCategory(
                [Parameter(Name = "CATEGORY_ID")]int id,
                [Parameter(Name = "CATEGORY_NAME")]string name,
                [Parameter(Name = "CATEGORY_DESCRIPTION")]string description)
            {
                ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), id, name, description);
            }

            [Function(Name = "GET_CATEGORIES_COUNT")]
            public void GetCategoriesCount(
                [Parameter(Name = "RETURN_VALUE")]
                out int count)
            {
                count = 0;
                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), count);
                count = (int)result.GetParameterValue(0);
            }

            [Function(Name = "GET_CATEGORY_NAME")]
            public void GetCategoryName(
                [Parameter(Name = "CATEGORY_ID", DbType = "INTEGER")]           int id,
                [Parameter(Name = "RETURN_VALUE", DbType = "VARCHAR(30)")]      out string name)
            {
                name = null;
                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), id, name);
                name = (string)result.GetParameterValue(1);
            }

            [Function(Name = "PKG1.GET_ALL_CATEGORIES")]
            public virtual ISingleResult<Category> GetAllCategories(
                [Parameter(Name = "MYCS", DbType = "CURSOR")] out object myrc)
            {
                myrc = null;
                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), myrc);
                return (ISingleResult<Category>)result.ReturnValue;
            }

            [Function(Name = "PKG3.GET_CATEGORIES_AND_PRODUCTS")]
            [ResultType(typeof(Category))]
            [ResultType(typeof(Product))]
            public virtual IMultipleResults GetCategoriesAndProducts(
                [Parameter(Name = "MYCS1", DbType = "CURSOR")]    out object myrc1,
                [Parameter(Name = "MYCS2", DbType = "CURSOR")]    out object myrc2)
            {
                myrc1 = null;
                myrc2 = null;

                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), myrc1, myrc2);
                return (IMultipleResults)result.ReturnValue;
            }


            [Function(Name = "PKG2.GET_ALL_CATEGORIES")]
            public ISingleResult<Category> GetAllCategories()
            {
                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod());
                return (ISingleResult<Category>)result.ReturnValue;
            }

            [Function(Name = "GET_CATEGORY_PIC")]
            public byte[] GetCategoryPicture(int id)
            {
                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), id);
                return (byte[])result.ReturnValue;
            }
        }

        [Table(Name = "CATEGORIES")]
        public class Category
        {
            [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "NUMBER")]
            public int CategoryID { get; private set; }

            [Column]
            public string CategoryName { get; set; }

            [Column(DbType = "CLOB")]
            public string Description { get; set; }

            [Column(DbType = "BLOB")]
            public ALinq.Binary Picture { get; set; }
        }


        [Table(Name = "PRODUCTS")]
        public class Product
        {
            [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "NUMBER")]
            public int ProductID { get; set; }

            [Column(DbType = "VarChar(30)")]
            public string ProductName { get; set; }
        }

        [Table]
        public class Dual
        {

        }

    }
}