using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ALinq;
using ALinq.Mapping;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class FirebirdProcedureTest
    {
        [TestMethod]
        public void CreateDatabase()
        {
            var db = new ProcedureDatabase();
            FbConnection.DropDatabase(db.Connection.ConnectionString);
            db.CreateDatabase();
        }

        [TestMethod]
        public void AddCategory()
        {
            var db = new ProcedureDatabase();
            var count = db.Categories.Count();
            var id = db.Categories.Max(o => o.CategoryID) + 1;
            db.AddCategory(id, "Name", "Description");
            Assert.AreEqual(count + 1, db.Categories.Count());
        }

        [TestMethod]
        public void GetCategoryCount()
        {
            var db = new ProcedureDatabase();
            int count;
            db.GetCategoriesCount(out count);
            Assert.AreEqual(db.Categories.Count(), count);

            count = db.GetCategoriesCount();
            Assert.AreEqual(db.Categories.Count(), count);
        }

        [TestMethod]
        public void GetCategoryName()
        {
            var db = new ProcedureDatabase();
            var id = db.Categories.Max(o => o.CategoryID);
            string name;
            db.GetCategoryName(id, out name);
            Assert.AreEqual(db.Categories.Where(o => o.CategoryID == id).Select(o => o.CategoryName).Single(), name);
        }

        [TestMethod]
        public void GetAllCategories()
        {
            var db = new ProcedureDatabase();
            string categoryName, categoryDescription;
            int categoryID;
            var items = db.GetAllCategories(out categoryID, out categoryName, out categoryDescription);
            Assert.AreEqual(db.Categories.Count(), items.Count());
        }

        [TestMethod]
        public void CreateProcedure1()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER PROCEDURE ADD_CATEGORY(CATEGORY_ID INTEGER, CATEGORY_NAME VARCHAR(30), CATEGORY_DESCRIPTION BLOB SUB_TYPE TEXT)");
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("INSERT INTO CATEGORIES (CategoryID,CategoryName, Description)");
            sb.AppendLine("Values (:CATEGORY_ID, :CATEGORY_NAME, :CATEGORY_DESCRIPTION);");
            sb.AppendLine("END");

            new ProcedureDatabase().ExecuteCommand(sb.ToString());
        }

        [TestMethod]
        public void CreateProcedure2()
        {
            var sb = new StringBuilder();
            sb.AppendLine("CREATE PROCEDURE GET_CATEGORIES_COUNT");
            sb.AppendLine("RETURNS(RETURN_VALUE INTEGER)");
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("SELECT COUNT(*) FROM CATEGORIES INTO RETURN_VALUE;");
            sb.AppendLine("END;");
            sb.AppendLine();

            new ProcedureDatabase().ExecuteCommand(sb.ToString());
        }

        [TestMethod]
        public void CreateProcedure3()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER PROCEDURE GET_CATEGORY_NAME(CATEGORY_ID INT)");
            sb.AppendLine("RETURNS(RETURN_VALUE VARCHAR(30))");
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("SELECT CATEGORYNAME");
            sb.AppendLine("FROM CATEGORIES");
            sb.AppendLine("WHERE CATEGORYID = :CATEGORY_ID");
            sb.AppendLine("INTO RETURN_VALUE;");
            sb.AppendLine("END;");
            sb.AppendLine();

            new ProcedureDatabase().ExecuteCommand(sb.ToString());
        }


        [TestMethod]
        public void CreateProcedure4()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ALTER PROCEDURE GET_ALL_CATEGORIES");
            sb.AppendLine("RETURNS(CATEGORYID INTEGER, CATEGORYNAME VARCHAR(30), DESCRIPTION BLOB SUB_TYPE TEXT)");
            sb.AppendLine("AS");
            sb.AppendLine("BEGIN");
            sb.AppendLine("FOR SELECT CATEGORYID, CATEGORYNAME, DESCRIPTION");
            sb.AppendLine("FROM CATEGORIES");
            sb.AppendLine("INTO :CATEGORYID, :CATEGORYNAME, :DESCRIPTION");
            sb.AppendLine("DO SUSPEND;");
            sb.AppendLine("END;");
            sb.AppendLine();

            new ProcedureDatabase().ExecuteCommand(sb.ToString());
        }

        [Database(Name = "PROCEDUREDATABASE")]
        [Provider(typeof(ALinq.Firebird.FirebirdProvider))]
        public class ProcedureDatabase : DataContext
        {
            public ProcedureDatabase()
                : base("User=SYSDBA;Password=masterkey;Database=ProcedureDatabase;DataSource=vpc1;ServerType=0")
            {
                Log = Console.Out;
            }

            public Table<Category> Categories
            {
                get { return GetTable<Category>(); }
            }

            [Function(Name = "ADD_CATEGORY")]
            public void AddCategory(int id, string name, string description)
            {
                ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), id, name, description);
            }

            [Function(Name = "GET_CATEGORIES_COUNT")]
            public void GetCategoriesCount(out int count)
            {
                count = 0;
                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), count);
                count = (int)result.GetParameterValue(0);
            }

            [Function(Name = "GET_CATEGORIES_COUNT")]
            public int GetCategoriesCount()
            {
                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod());
                return (int)result.ReturnValue;
            }

            [Function(Name = "GET_CATEGORY_NAME")]
            public void GetCategoryName(
                [Parameter(Name = "CATEGORY_ID", DbType = "INTEGER")]
                int id,
                [Parameter(Name = "RETURN_VALUE", DbType = "VARCHAR(30)")]
                out string name)
            {
                name = null;
                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), id, name);
                name = (string)result.GetParameterValue(1);
            }

            [Function(Name = "PKG1.GET_ALL_CATEGORIES")]
            public ISingleResult<Category> GetCategories1(
                [Parameter(DbType = "REF CURSOR")]
                out object myrc)
            {
                myrc = null;
                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), myrc);
                //myrc = result.GetParameterValue(0);
                return (ISingleResult<Category>)result.ReturnValue;
            }

            [Function(Name = "GET_ALL_CATEGORIES")]
            public ISingleResult<Category> GetAllCategories(
                out int categoryID, 
                out string categoryName, 
                out string categoryDescription)
            {
                categoryID = 0;
                categoryName = "";
                categoryDescription = "";

                var result = ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), categoryID, categoryName, categoryDescription);
      
                return (ISingleResult<Category>)result.ReturnValue;
            }
        }

        [Table(Name = "CATEGORIES")]
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

            [Column(DbType = "BLOB SUB_TYPE TEXT")]
            public string Description
            {
                get;
                set;
            }

            [Column(DbType = "BLOB")]
            public ALinq.Binary Picture
            {
                get;
                set;
            }
        }
    }
}
