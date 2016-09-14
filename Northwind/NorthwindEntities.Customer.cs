using System;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Table(Name="Customers")]
    public partial class Customer
    {
        private System.String _CustomerID;
        private System.String _CompanyName;
        private System.String _ContactName;
        private System.String _ContactTitle;
        private System.String _Address;
        private System.String _City;
        private System.String _Region;
        private System.String _PostalCode;
        private System.String _Country;
        private System.String _Phone;
        private System.String _Fax;
        private EntitySet<CustomerCustomerDemo> _CustomerCustomerDemos;
        private EntitySet<Order> _Orders;

        public Customer()
        {
            this._CustomerCustomerDemos = new EntitySet<CustomerCustomerDemo>();
            this._Orders = new EntitySet<Order>();
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, IsPrimaryKey=true, AutoSync=AutoSync.Never, Name="CustomerID", DbType="VarChar(5)")]
        public System.String CustomerID
        {
            get
            {
                return _CustomerID;
            }
            set
            {
                _CustomerID = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="CompanyName", DbType="VarChar(40)")]
        public System.String CompanyName
        {
            get
            {
                return _CompanyName;
            }
            set
            {
                _CompanyName = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="ContactName", DbType="VarChar(30)")]
        public System.String ContactName
        {
            get
            {
                return _ContactName;
            }
            set
            {
                _ContactName = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="ContactTitle", DbType="VarChar(30)")]
        public System.String ContactTitle
        {
            get
            {
                return _ContactTitle;
            }
            set
            {
                _ContactTitle = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Address", DbType="VarChar(60)")]
        public System.String Address
        {
            get
            {
                return _Address;
            }
            set
            {
                _Address = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="City", DbType="VarChar(15)")]
        public System.String City
        {
            get
            {
                return _City;
            }
            set
            {
                _City = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Region", DbType="VarChar(15)")]
        public System.String Region
        {
            get
            {
                return _Region;
            }
            set
            {
                _Region = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="PostalCode", DbType="VarChar(10)")]
        public System.String PostalCode
        {
            get
            {
                return _PostalCode;
            }
            set
            {
                _PostalCode = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Country", DbType="VarChar(15)")]
        public System.String Country
        {
            get
            {
                return _Country;
            }
            set
            {
                _Country = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Phone", DbType="VarChar(24)")]
        public System.String Phone
        {
            get
            {
                return _Phone;
            }
            set
            {
                _Phone = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Fax", DbType="VarChar(24)")]
        public System.String Fax
        {
            get
            {
                return _Fax;
            }
            set
            {
                _Fax = value;
            }
        }

        [Newtonsoft.Json.JsonIgnore()]
        [Association(Storage="_CustomerCustomerDemos", ThisKey="CustomerID", OtherKey="CustomerID", IsForeignKey=false, Name="C_CCD")]
        public EntitySet<CustomerCustomerDemo> CustomerCustomerDemos
        {
            get
            {
                return this._CustomerCustomerDemos;
            }
            set
            {
                this._CustomerCustomerDemos.Assign(value);
            }
        }

        [Newtonsoft.Json.JsonIgnore()]
        [Association(Storage="_Orders", ThisKey="CustomerID", OtherKey="CustomerID", IsForeignKey=false, Name="Customer_Order")]
        public EntitySet<Order> Orders
        {
            get
            {
                return this._Orders;
            }
            set
            {
                this._Orders.Assign(value);
            }
        }
    }
}
