using System.Data;
using System.Linq;
using ALinq.Access;
using ALinq.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;

namespace Test
{


    /// <summary>
    ///This is a test class for AttributeMappingSourceTest and is intended
    ///to contain all AttributeMappingSourceTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AttributeMappingSourceTest
    {

        #region
        [ALinq.Mapping.Provider(typeof(AccessDbProvider))]
        [ALinq.Mapping.Database(Name = "Northwind")]
        class Database1 : ALinq.DataContext
        {
            public Database1()
                : base(AccessNorthwind.CreateConnection(@"C:/Northwind.mdb"))
            {
            }
        }

        class Database2 : Database1
        {
            ALinq.Table<Table1> Table1;
            ALinq.Table<Table2> Table2;
        }

        [Table(Name = "Table1")]
        class Table1
        {

        }

        [Table(Name = "Table2")]
        class Table2
        {

        }
        #endregion
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

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for AttributeMappingSource Constructor
        ///</summary>
        [TestMethod()]
        public void ProviderAndDatabaseNameTest()
        {
            //Test Provider
            var mappingSource = new AttributeMappingSource();
            var model = mappingSource.GetModel(typeof(Database1));
            Assert.AreSame(typeof(AccessDbProvider), model.ProviderType);
            //Test DatabaseName
            Assert.AreEqual<string>("Northwind", model.DatabaseName);
        }

        [TestMethod()]
        public void TableTest()
        {
            var mappingSource = new AttributeMappingSource();
            var model = mappingSource.GetModel(typeof(Database2));
            var metaTables = model.GetTables().ToArray();
            
            //Test meta tables Count
            Assert.AreEqual(2, metaTables.Count());
            
            //Test meta table name.
            Assert.AreEqual("Table1", metaTables[0].TableName);
            Assert.AreEqual("Table2", metaTables[1].TableName);
        }
    }
}
