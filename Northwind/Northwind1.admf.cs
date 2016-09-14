using System;
using System.Collections.Generic;
using System.ComponentModel;
using ALinq.Mapping;
using ALinq;
namespace NorthwindDemo
{
    partial class NorthwindDatabase
    {
        //partial void InsertOrder(Order order)
        //{
        //    Console.WriteLine("Insert Order");

        //}
   
        public const string DB_HOST = "vpc1";//"localhost";//

        public enum Enum
        {
            Item1,
            Item2,
            Item3
        }
    }

    partial class DataType
    {
        private Guid guid = Guid.Empty;

        //[Column]
        //public Guid Guid
        //{
        //    get { return guid; }
        //    set { guid = value; }
        //}
    }

    public interface IOrder
    {
        int OrderID { get; set; }
        DateTime? OrderDate { get; set; }
        int EmployeeID { get; set; }
    }


    partial class Order : IOrder
    {
        void SetDataContext(DataContext dataContext)
        {
            DataContext = (NorthwindDatabase)dataContext;
        }

        public NorthwindDatabase DataContext
        {
            get;
            set;
        }

        
        private Dictionary<string, object> properties = new Dictionary<string, object>();
        public object this[string key]
        {
            get
            {
                object value;
                properties.TryGetValue(key, out value);
                return value;
            }
            set
            {
                properties[key] = value;
            }
        }
        /**/
        [Function]
        public DateTime Now()
        {
            throw new NotSupportedException();
        }
    }

    partial class Customer
    {
        void SetDataContext(DataContext dataContext)
        {
            DataContext = (NorthwindDatabase)dataContext;
        }

        public NorthwindDatabase DataContext
        {
            get;
            set;
        }
    }

    partial class Contact //: ALinq.IChangedProperties
    {
        void SetDataContext(DataContext dataContext)
        {
            this.DataContext = (NorthwindDatabase)dataContext;
        }

        public NorthwindDatabase DataContext
        {
            get;
            private set;
        }
    }

    partial class Product
    {

    }

    partial class Employee
    {

        private Dictionary<string, object> values = new Dictionary<string, object>();
        //[Column]
        public object this[string name]
        {
            get
            {
                object item;
                if (values.TryGetValue(name, out item))
                    return item;

                return null;
            }
            set
            {
                values[name] = value;
            }
        }
    }
}