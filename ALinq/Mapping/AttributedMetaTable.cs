using System;
using System.Diagnostics;
using System.Reflection;
using ALinq.Mapping;

namespace ALinq.Mapping
{
    internal sealed class AttributedMetaTable : MetaTable
    {
        // Fields
        private MethodInfo deleteMethod;
        private bool hasMethods;
        private MethodInfo insertMethod;
        private AttributedMetaModel model;
        private MetaType rowType;
        private string tableName;
        private MethodInfo updateMethod;

        // Methods
        internal AttributedMetaTable(AttributedMetaModel model, TableAttribute attr, Type rowType)
        {
            Debug.Assert(attr != null);
            this.model = model;
            this.tableName = string.IsNullOrEmpty(attr.Name) ? rowType.Name : attr.Name;
            this.rowType = new AttributedRootType(model, this, rowType);
        }

        private void InitMethods()
        {
            if (!this.hasMethods)
            {
                this.insertMethod = MethodFinder.FindMethod(this.model.ContextType, "Insert" + this.rowType.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, new Type[] { this.rowType.Type });
                this.updateMethod = MethodFinder.FindMethod(this.model.ContextType, "Update" + this.rowType.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, new Type[] { this.rowType.Type });
                this.deleteMethod = MethodFinder.FindMethod(this.model.ContextType, "Delete" + this.rowType.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, new Type[] { this.rowType.Type });
                this.hasMethods = true;
            }
        }

        // Properties
        public override MethodInfo DeleteMethod
        {
            get
            {
                this.InitMethods();
                return this.deleteMethod;
            }
        }

        public override MethodInfo InsertMethod
        {
            get
            {
                this.InitMethods();
                return this.insertMethod;
            }
        }

        public override MetaModel Model
        {
            get
            {
                return this.model;
            }
        }

        public override MetaType RowType
        {
            get
            {
                return this.rowType;
            }
        }

        public override string TableName
        {
            get
            {
                return this.tableName;
            }
        }

        public override MethodInfo UpdateMethod
        {
            get
            {
                this.InitMethods();
                return this.updateMethod;
            }
        }
    }



}