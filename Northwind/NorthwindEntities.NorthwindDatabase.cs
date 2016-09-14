using ALinq;
using ALinq.Mapping;
using System;
using System.ComponentModel;
using System.Reflection;

namespace NorthwindDemo
{
    public partial class NorthwindDatabase : ALinq.DataContext
    {
        public NorthwindDatabase(string connection) : base(connection)
        {
        }
        public NorthwindDatabase(System.Data.IDbConnection connection) : base(connection)
        {
        }
         public NorthwindDatabase(string connection, MappingSource mapping) : base(connection, mapping)
        {
        }
        public NorthwindDatabase(System.Data.IDbConnection connection, MappingSource mapping) : base(connection, mapping)
        {
        }

        public ALinq.Table<User> Users
        {
            get
            {
                return this.GetTable<User>();
            }
        }

        public ALinq.Table<SqlMethod> SqlMethods
        {
            get
            {
                return this.GetTable<SqlMethod>();
            }
        }

        public ALinq.Table<Contact> Contacts
        {
            get
            {
                return this.GetTable<Contact>();
            }
        }

        public ALinq.Table<Category> Categories
        {
            get
            {
                return this.GetTable<Category>();
            }
        }

        public ALinq.Table<CustomerCustomerDemo> CustomerCustomerDemos
        {
            get
            {
                return this.GetTable<CustomerCustomerDemo>();
            }
        }

        public ALinq.Table<CustomerDemographic> CustomerDemographics
        {
            get
            {
                return this.GetTable<CustomerDemographic>();
            }
        }

        public ALinq.Table<Customer> Customers
        {
            get
            {
                return this.GetTable<Customer>();
            }
        }

        public ALinq.Table<Employee> Employees
        {
            get
            {
                return this.GetTable<Employee>();
            }
        }

        public ALinq.Table<EmployeeTerritory> EmployeeTerritories
        {
            get
            {
                return this.GetTable<EmployeeTerritory>();
            }
        }

        public ALinq.Table<OrderDetail> OrderDetails
        {
            get
            {
                return this.GetTable<OrderDetail>();
            }
        }

        public ALinq.Table<Order> Orders
        {
            get
            {
                return this.GetTable<Order>();
            }
        }

        public ALinq.Table<Product> Products
        {
            get
            {
                return this.GetTable<Product>();
            }
        }

        public ALinq.Table<Region> Regions
        {
            get
            {
                return this.GetTable<Region>();
            }
        }

        public ALinq.Table<Shipper> Shippers
        {
            get
            {
                return this.GetTable<Shipper>();
            }
        }

        public ALinq.Table<Supplier> Suppliers
        {
            get
            {
                return this.GetTable<Supplier>();
            }
        }

        public ALinq.Table<Territory> Territories
        {
            get
            {
                return this.GetTable<Territory>();
            }
        }

        public ALinq.Table<Temp> Temps
        {
            get
            {
                return this.GetTable<Temp>();
            }
        }

        public ALinq.Table<DataType> DataTypes
        {
            get
            {
                return this.GetTable<DataType>();
            }
        }

        public ALinq.Table<Class1> Class1s
        {
            get
            {
                return this.GetTable<Class1>();
            }
        }

        public ALinq.Table<Class2> Class2s
        {
            get
            {
                return this.GetTable<Class2>();
            }
        }
    }
}
