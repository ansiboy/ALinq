#if DEBUG
using System;
using System.ComponentModel;
using ALinq.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ALinq.Mapping;
using NorthwindDemo;

namespace Test
{


    /// <summary>
    ///This is a test class for DatabaseSqlBuilderTest and is intended
    ///to contain all DatabaseSqlBuilderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AccessSqlBuilderTest
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
        ///A test for GetCreateTableCommand
        ///</summary>
        [TestMethod()]
        public void GetCreateTableCommandTest()
        {
            LicenseManager.CurrentContext.SetSavedLicenseKey(typeof(ALinq.Access.AccessDbProvider), "ansiboy" + Environment.NewLine + "QO77626437CFA2FE9E");
            //var obj = LicenseManager.CreateWithContext(typeof (ALinq.Access.AccessDbProvider), new LicenseContext());
            //Console.Write(obj);
            var mapping = XmlMappingSource.FromStream(GetType().Assembly.GetManifestResourceStream("Test.Northwind.Access.map"));
            var db = new AccessNorthwind("c:/nrothwind.mdb", mapping);

            var target = new DatabaseSqlBuilder((SqlProvider) db.Provider);

            MetaTable table = db.Mapping.GetTable(typeof(Category));
            //string expected = string.Empty; 
            string actual;
            actual = target.GetCreateTableCommand(table);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
            Console.WriteLine(actual);
        }
    }
}

#endif