
using System;
using System.Globalization;
using ALinq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Test
{


    /// <summary>
    ///This is a test class for SRTest and is intended
    ///to contain all SRTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SRTest
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

        # if DEBUG
        /// <summary>
        ///A test for GetString
        ///</summary>
        [TestMethod()]
        public void GetStringTest()
        {
            string name = ALinq.SR.CannotAddChangeConflicts; 
            var actual = SR.GetString(name);
            name = ALinq.Mapping.SR.CouldNotFindElementTypeInModel;
            actual = ALinq.Mapping.SR.GetString(name);
            name = ALinq.SqlClient.SR.ArgumentWrongType;
            var cu = CultureInfo.CurrentCulture;
            actual = ALinq.SqlClient.SR.GetString(name);
            var txt = ALinq.Resources.ALinq.CannotAddChangeConflicts;
        }
        #endif
    }
}
