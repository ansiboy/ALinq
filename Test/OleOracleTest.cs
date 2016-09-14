using System;
using System.Data.Common;
using System.Data.OleDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDemo;

namespace Test
{
    //[TestClass]
    public class OleOracleTest : NorOracleTest
    {
        public override NorthwindDatabase CreateDataBaseInstace()
        {
            return new OracleNorthwind(CreateConnection(), base.CreateDataBaseInstace().Mapping.MappingSource) { Log = Console.Out };
        }

        protected DbConnection CreateConnection()
        {
            var conn = new OleDbConnection("Provider=OraOLEDB.Oracle.1;Data Source=localhost;User ID=Northwind;Password=Test");
            return conn;
        }
    }
}