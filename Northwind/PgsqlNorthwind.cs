using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
#if !FREE
     //[License("ansiboy", "P9FAD1B5DF021E8CD")]
#endif
    [Provider(typeof(ALinq.PostgreSQL.PgsqlProvider))]
    public class PgsqlNorthwind : NorthwindDatabase
    {
        public PgsqlNorthwind()
            : base("HOST=localhost;User ID=postgres;PASSWORD=test;DATABASE=northwind")
        {
        }

        public PgsqlNorthwind(IDbConnection connection)
            : base(connection)
        {
        }

        public PgsqlNorthwind(string connection, MappingSource mappingSource)
            : base(connection, mappingSource)
        {
        }

        public PgsqlNorthwind(IDbConnection connection, MappingSource mappingSource)
            : base(connection, mappingSource)
        {
        }
    }
}
