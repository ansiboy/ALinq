using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.DynamicData;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace ALinq.Web.DynamicData
{
    internal static class Misc
    {
        // Fields
        private static bool? s_isNonUpdatablePrecompiledApp;

        // Methods
        private static long AddHashCode(long currentHash, object o)
        {
            if (o == null)
            {
                return currentHash;
            }
            return (((currentHash << 5) + currentHash) ^ o.GetHashCode());
        }

        internal static long CombineHashCodes(object o1, object o2)
        {
            long currentHash = 0x1505L;
            return AddHashCode(AddHashCode(currentHash, o1), o2);
        }

        internal static long CombineHashCodes(object o1, object o2, object o3)
        {
            long currentHash = 0x1505L;
            return AddHashCode(AddHashCode(AddHashCode(currentHash, o1), o2), o3);
        }

        internal static bool EqualDataKeys(DataKey dataKey1, DataKey dataKey2)
        {
            foreach (DictionaryEntry entry in dataKey1.Values)
            {
                string a = Convert.ToString(entry.Value, CultureInfo.InvariantCulture);
                string b = Convert.ToString(dataKey2.Values[entry.Key], CultureInfo.InvariantCulture);
                if (!string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }

        internal static void ExtractValuesFromBindableControls(IOrderedDictionary dictionary, Control container)
        {
            IBindableControl control = container as IBindableControl;
            if (control != null)
            {
                control.ExtractValues(dictionary);
            }
            foreach (Control control2 in container.Controls)
            {
                ExtractValuesFromBindableControls(dictionary, control2);
            }
        }

        public static void FillListItemCollection(System.Web.DynamicData.MetaTable table, ListItemCollection listItemCollection)
        {
            Func<object, object> func2 = null;
            IQueryable query = table.GetQuery();
            IEnumerable enumerable = null;
            MetaColumn sortColumn = table.SortColumn;
            if (sortColumn == null)
            {
                enumerable = query;
            }
            else if (sortColumn.IsCustomProperty)
            {
                IEnumerable<object> source = query.Cast<object>().AsEnumerable<object>();
                if (func2 == null)
                {
                    func2 = delegate(object row)
                                {
                                    return DataBinder.GetPropertyValue(row, sortColumn.Name);
                                };
                }
                Func<object, object> keySelector = func2;
                if (table.SortDescending)
                {
                    enumerable = source.OrderByDescending<object, object>(keySelector);
                }
                else
                {
                    enumerable = source.OrderBy<object, object>(keySelector);
                }
            }
            else
            {
                ParameterExpression expression = Expression.Parameter(table.EntityType, "row");
                LambdaExpression expression2 = null;
                if (sortColumn is MetaForeignKeyColumn)
                {
                    MetaColumn column = (sortColumn as MetaForeignKeyColumn).ParentTable.SortColumn;
                    expression2 = Expression.Lambda(Expression.Property(Expression.Property(expression, sortColumn.Name), column.Name), new ParameterExpression[] { expression });
                }
                else
                {
                    expression2 = Expression.Lambda(Expression.Property(expression, sortColumn.Name), new ParameterExpression[] { expression });
                }
                string methodName = table.SortDescending ? "OrderByDescending" : "OrderBy";
                MethodCallExpression expression3 = Expression.Call(typeof(Queryable), methodName, new Type[] { query.ElementType, expression2.Body.Type }, new Expression[] { query.Expression, expression2 });
                enumerable = query.Provider.CreateQuery(expression3);
            }
            foreach (object obj2 in enumerable)
            {
                string displayString = table.GetDisplayString(obj2);
                string primaryKeyString = table.GetPrimaryKeyString(obj2);
                listItemCollection.Add(new ListItem(displayString, primaryKeyString.TrimEnd(new char[0])));
            }
        }

        public static Control FindControl(Control control, string controlID)
        {
            Control namingContainer = control;
            Control control3 = null;
            if (control != control.Page)
            {
                while ((control3 == null) && (namingContainer != control.Page))
                {
                    namingContainer = namingContainer.NamingContainer;
                    if (namingContainer == null)
                    {
                        throw new HttpException(string.Format(CultureInfo.CurrentCulture, DynamicDataResources.Misc_NoNamingContainer, new object[] { control.GetType().Name, control.ID }));
                    }
                    control3 = namingContainer.FindControl(controlID);
                }
                return control3;
            }
            return control.FindControl(controlID);
        }

        public static object[] GetKeyValues(IList<MetaColumn> keyMembers, object entity)
        {
            object[] objArray = new object[keyMembers.Count];
            int num = 0;
            foreach (MetaColumn column in keyMembers)
            {
                objArray[num++] = DataBinder.GetPropertyValue(entity, column.Name);
            }
            return objArray;
        }

        public static string GetRouteValue(string key)
        {
            object obj2;
            if (!DynamicDataRouteHandler.GetRequestContext(HttpContext.Current).RouteData.Values.TryGetValue(key, out obj2))
            {
                return null;
            }
            return (obj2 as string);
        }

        internal static bool IsNonUpdatablePrecompiledApp()
        {
            if (!s_isNonUpdatablePrecompiledApp.HasValue)
            {
                try
                {
                    s_isNonUpdatablePrecompiledApp = new bool?(IsNonUpdatablePrecompiledAppNoCache());
                }
                catch
                {
                    s_isNonUpdatablePrecompiledApp = false;
                }
            }
            return s_isNonUpdatablePrecompiledApp.Value;
        }

        private static bool IsNonUpdatablePrecompiledAppNoCache()
        {
            if (HostingEnvironment.VirtualPathProvider == null)
            {
                return false;
            }
            string virtualPath = VirtualPathUtility.ToAbsolute("~/PrecompiledApp.config");
            if (!HostingEnvironment.VirtualPathProvider.FileExists(virtualPath))
            {
                return false;
            }
            XmlDocument document = new XmlDocument();
            document.Load(VirtualPathProvider.OpenFile(virtualPath));
            XmlNode documentElement = document.DocumentElement;
            if ((documentElement == null) || (documentElement.Name != "precompiledApp"))
            {
                return false;
            }
            XmlNode namedItem = documentElement.Attributes.GetNamedItem("updatable");
            return ((namedItem != null) && (namedItem.Value == "false"));
        }

        public static string[] ParseCommaSeparatedString(string stringList)
        {
            return stringList.Split(new char[] { ',' });
        }

        public static string PersistListToCommaSeparatedString(IList<object> list)
        {
            if ((list == null) || (list.Count == 0))
            {
                return string.Empty;
            }
            if (list.Count == 1)
            {
                if (list[0] != null)
                {
                    return list[0].ToString().TrimEnd(new char[0]);
                }
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            bool flag2 = false;
            foreach (object obj2 in list)
            {
                if (!flag)
                {
                    builder.Append(",");
                }
                if (obj2 != null)
                {
                    builder.Append(obj2.ToString().TrimEnd(new char[0]));
                    flag2 = true;
                }
                flag = false;
            }
            if (!flag2)
            {
                return string.Empty;
            }
            return builder.ToString();
        }

        public static Type RemoveNullableFromType(Type type)
        {
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                return type.GetGenericArguments()[0];
            }
            return type;
        }

        public static string SanitizeQueryStringValue(object value)
        {
            if (value == null)
            {
                return null;
            }
            return value.ToString().TrimEnd(new char[0]);
        }
    }
}