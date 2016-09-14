using System;
using System.Collections.Generic;
using System.Collections;

namespace LinqToSqlShared.Mapping
{
    #region MyRegion
    //internal class DatabaseMapping : SqlMetalObject
    //{
    //    private const string TypeName = "LinqToSqlShared.Mapping.DatabaseMapping";

    //    internal DatabaseMapping()
    //        : base(CreateSource(TypeName, null))
    //    {
    //    }

    //    public DatabaseMapping(object source)
    //        : base(source)
    //    {
    //    }

    //    public string DatabaseName
    //    {
    //        get { return (string)GetProperty("DatabaseName", false); }
    //        set { SetProperty("DatabaseName", value, false); }
    //    }

    //    public string Provider
    //    {
    //        get { return (string)GetProperty("Provider", false); }
    //        set { SetProperty("Provider", value, false); }
    //    }

    //    private ReflectObjectList<TableMapping> tables;
    //    public IList<TableMapping> Tables
    //    {
    //        get
    //        {
    //           if(tables == null)
    //           {
    //               var result = (IList)GetProperty("Tables", false);
    //               tables = new ReflectObjectList<TableMapping>(result);
    //           }
    //           return tables;
    //        }
    //    }

    //    private ReflectObjectList<FunctionMapping> functions;
    //    public IList<FunctionMapping> Functions
    //    {
    //        get
    //        {
    //            if(functions == null)
    //            {
    //                var result = (IList)GetProperty("Functions", false);
    //                functions = new ReflectObjectList<FunctionMapping>(result);
    //            }
    //            return functions;
    //        }
    //    }
    //} 
    #endregion

    internal class DatabaseMapping
    {
        // Fields
        private string databaseName;
        private List<FunctionMapping> functions = new List<FunctionMapping>();
        private string provider;
        private List<TableMapping> tables = new List<TableMapping>();

        // Methods
        internal DatabaseMapping()
        {
        }

        internal FunctionMapping GetFunction(string functionName)
        {
            foreach (FunctionMapping mapping in this.functions)
            {
                if (string.Compare(mapping.Name, functionName, StringComparison.Ordinal) == 0)
                {
                    return mapping;
                }
            }
            return null;
        }

        internal TableMapping GetTable(string tableName)
        {
            foreach (TableMapping mapping in this.tables)
            {
                if (string.Compare(mapping.TableName, tableName, StringComparison.Ordinal) == 0)
                {
                    return mapping;
                }
            }
            return null;
        }

        internal TableMapping GetTable(Type rowType)
        {
            foreach (TableMapping mapping in this.tables)
            {
                if (this.IsType(mapping.RowType, rowType))
                {
                    return mapping;
                }
            }
            return null;
        }

        private bool IsType(TypeMapping map, Type type)
        {
            if (((string.Compare(map.Name, type.Name, StringComparison.Ordinal) == 0) || (string.Compare(map.Name, type.FullName, StringComparison.Ordinal) == 0)) || (string.Compare(map.Name, type.AssemblyQualifiedName, StringComparison.Ordinal) == 0))
            {
                return true;
            }
            foreach (TypeMapping mapping in map.DerivedTypes)
            {
                if (this.IsType(mapping, type))
                {
                    return true;
                }
            }
            return false;
        }

        // Properties
        internal string DatabaseName
        {
            get
            {
                return this.databaseName;
            }
            set
            {
                this.databaseName = value;
            }
        }

        internal List<FunctionMapping> Functions
        {
            get
            {
                return this.functions;
            }
        }

        internal string Provider
        {
            get
            {
                return this.provider;
            }
            set
            {
                this.provider = value;
            }
        }

        internal List<TableMapping> Tables
        {
            get
            {
                return this.tables;
            }
        }
    }
}
