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
 
        //public const string DB_HOST = "localhost";//"vpc1";

        //public enum Enum
        //{
        //    Item1,
        //    Item2,
        //    Item3
        //}
    }

    partial class DataType
    {
        //private Guid guid = Guid.Empty;

        //[Column]
        //public Guid Guid
        //{
        //    get { return guid; }
        //    set { guid = value; }
        //}
    }

    //partial class Order
    //{
    //    void SetDataContext(DataContext dataContext)
    //    {
    //        DataContext = (NorthwindDatabase)dataContext;
    //    }

    //    [Newtonsoft.Json.JsonIgnore()]
    //    public NorthwindDatabase DataContext
    //    {
    //        get;
    //        set;
    //    }
    //}

    //partial class Customer
    //{
    //    void SetDataContext(DataContext dataContext)
    //    {
    //        DataContext = (NorthwindDatabase)dataContext;
    //    }

    //    [Newtonsoft.Json.JsonIgnore]
    //    public NorthwindDatabase DataContext
    //    {
    //        get;
    //        set;
    //    }
    //}

    //partial class Contact //: ALinq.IChangedProperties
    //{
    //    void SetDataContext(DataContext dataContext)
    //    {
    //        this.DataContext = (NorthwindDatabase)dataContext;
    //    }

    //    public NorthwindDatabase DataContext
    //    {
    //        get;
    //        private set;
    //    }
    //}

    partial class Product
    {

    }
}