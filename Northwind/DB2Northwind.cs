using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using ALinq;
using ALinq.DB2;
using ALinq.Mapping;

namespace NorthwindDemo
{
#if !FREE
    //[License("ansiboy", "UUGHC5BB4C652A207BBA5")]
#endif
    [Provider(typeof(DB2Provider))]
    public class DB2Northwind : NorthwindDatabase
    {
        public DB2Northwind()
            : base("DataBase=SAMPLE;USER ID=db2admin;Password=test;Server=localhost")
        {

        }
        public DB2Northwind(string connection)
            : base(connection)
        {
        }

        public DB2Northwind(IDbConnection connection)
            : base(connection)
        {
        }

        public DB2Northwind(string connection, MappingSource mappingSource)
            : base(connection, mappingSource)
        {
        }

        public DB2Northwind(IDbConnection connection, MappingSource mappingSource)
            : base(connection, mappingSource)
        {
        }

        public DB2Northwind(DbConnection connection, Type providerType)
            : base(connection, providerType)
        {
        }

        public DB2Northwind(string fileOrServerOrConnection, Type providerType)
            : base(fileOrServerOrConnection, providerType)
        {
        }

        public override void CreateDatabase()
        {
            //foreach (var metaTable in this.Mapping.GetTables())
            //{
            //    if (this.TableExists(metaTable))
            //        this.DeleteTable(metaTable);
            //    this.CreateTable(metaTable);
            //}
            //foreach (var metaTable in this.Mapping.GetTables())
            //{
            //    this.CreateForeignKeys(metaTable);
            //}
            //ImportData();
        }

        public new bool DatabaseExists()
        {
            return false;
        }

        [Function]
        public new DateTime Now()
        {
            throw new NotSupportedException();
        }
    }
}
