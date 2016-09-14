using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ALinq.Mapping;

namespace ALinq.MySQL
{
    partial class MySqlProvider : IProviderExtend
    {
        public void CreateTable(MetaTable metaTable)
        {
            string createTableCommand = MySqlBuilder.GetCreateTableCommand(metaTable);
            Execute(Connection,Transaction, createTableCommand);
        }

        public void CreateColumn(MetaDataMember metaMember)
        {
            throw new System.NotImplementedException();
        }
    }
}
