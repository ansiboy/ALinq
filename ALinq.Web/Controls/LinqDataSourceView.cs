using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI.WebControls;

namespace ALinq.Web.Controls
{
    public class ALinqDataSourceView : System.Web.UI.WebControls.LinqDataSourceView
    {
        private readonly ALinqDataSource owner;

        public ALinqDataSourceView(ALinqDataSource owner, string name, HttpContext context)
            : base(owner, name, context)
        {
            this.owner = owner;
        }

        protected override void ValidateContextType(Type contextType, bool selecting)
        {
            if (!selecting && !typeof(DataContext).IsAssignableFrom(contextType))
            {
                throw new InvalidOperationException("InvalidContextType");
            }
        }

        protected override void ValidateTableType(Type tableType, bool selecting)
        {
            if (!selecting && (!tableType.IsGenericType ||
                               !tableType.GetGenericTypeDefinition().Equals(typeof(Table<>))))
            {
                throw new InvalidOperationException("InvalidTablePropertyType");
            }
        }

        protected override void UpdateDataObject(object dataContext, object table, object oldDataObject, object newDataObject)
        {
            ((ITable)table).Attach(oldDataObject);
            Dictionary<string, Exception> innerExceptions = SetDataObjectProperties(oldDataObject, newDataObject);
            if (innerExceptions != null)
            {
                throw new LinqDataSourceValidationException("ValidationFailed", innerExceptions);
            }
            ((DataContext)dataContext).SubmitChanges();
        }

        protected override void DeleteDataObject(object dataContext, object table, object oldDataObject)
        {
            ((ITable)table).Attach(oldDataObject);
            ((ITable)table).DeleteOnSubmit(oldDataObject);
            ((DataContext)dataContext).SubmitChanges();
        }

        protected override void InsertDataObject(object dataContext, object table, object newDataObject)
        {
            ((ITable)table).InsertOnSubmit(newDataObject);
            ((DataContext)dataContext).SubmitChanges();
        }

        protected override object CreateContext(Type contextType)
        {
            var result = base.CreateContext(contextType);
            if (owner.Log != null)
                ((DataContext)result).Log = owner.Log;
            return result;
        }

        protected override void ValidateUpdateSupported(System.Collections.IDictionary keys, System.Collections.IDictionary values, System.Collections.IDictionary oldValues)
        {
            base.ValidateUpdateSupported(keys, values, oldValues);
        }

        #region SetDataObjectProperties
        private static Dictionary<string, Exception> SetDataObjectProperties(object oldDataObject, object newDataObject)
        {
            Dictionary<string, Exception> dictionary = null;
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(oldDataObject))
            {
                if (descriptor.PropertyType.IsSerializable && !descriptor.IsReadOnly)
                {
                    object obj2 = descriptor.GetValue(newDataObject);
                    try
                    {
                        descriptor.SetValue(oldDataObject, obj2);
                        continue;
                    }
                    catch (Exception exception)
                    {
                        if (dictionary == null)
                        {
                            dictionary = new Dictionary<string, Exception>(StringComparer.OrdinalIgnoreCase);
                        }
                        dictionary[descriptor.Name] = exception;
                        continue;
                    }
                }
            }
            return dictionary;
        }

        #endregion

    }
}