using System;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Table(Name="Orders")]
    public partial class Order
    {
        private System.Int32 _OrderID;
        private System.String _CustomerID;
        private System.Int32 _EmployeeID;
        private Nullable<System.DateTime> _OrderDate;
        private Nullable<System.DateTime> _RequiredDate;
        private Nullable<System.DateTime> _ShippedDate;
        private Nullable<System.Int32> _ShipVia;
        private Nullable<System.Decimal> _Freight;
        private System.String _ShipName;
        private System.String _ShipAddress;
        private System.String _ShipCity;
        private System.String _ShipRegion;
        private System.String _ShipPostalCode;
        private System.String _ShipCountry;
        private EntitySet<OrderDetail> _OrderDetails;
        private EntityRef<Customer> _Customer;
        private EntityRef<Employee> _Employee;
        private EntityRef<Shipper> _Shipper;

        public Order()
        {
            this._OrderDetails = new EntitySet<OrderDetail>();
            this._Customer = default(EntityRef<Customer>);
            this._Employee = default(EntityRef<Employee>);
            this._Shipper = default(EntityRef<Shipper>);
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, IsPrimaryKey=true, AutoSync=AutoSync.Never, Name="OrderID")]
        public System.Int32 OrderID
        {
            get
            {
                return _OrderID;
            }
            set
            {
                _OrderID = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="CustomerID", DbType="VarChar(5)")]
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

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="EmployeeID")]
        public System.Int32 EmployeeID
        {
            get
            {
                return _EmployeeID;
            }
            set
            {
                _EmployeeID = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="OrderDate")]
        public Nullable<System.DateTime> OrderDate
        {
            get
            {
                return _OrderDate;
            }
            set
            {
                _OrderDate = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="RequiredDate")]
        public Nullable<System.DateTime> RequiredDate
        {
            get
            {
                return _RequiredDate;
            }
            set
            {
                _RequiredDate = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="ShippedDate")]
        public Nullable<System.DateTime> ShippedDate
        {
            get
            {
                return _ShippedDate;
            }
            set
            {
                _ShippedDate = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="ShipVia")]
        public Nullable<System.Int32> ShipVia
        {
            get
            {
                return _ShipVia;
            }
            set
            {
                _ShipVia = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Freight")]
        public Nullable<System.Decimal> Freight
        {
            get
            {
                return _Freight;
            }
            set
            {
                _Freight = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="ShipName", DbType="VarChar(40)")]
        public System.String ShipName
        {
            get
            {
                return _ShipName;
            }
            set
            {
                _ShipName = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="ShipAddress", DbType="VarChar(60)")]
        public System.String ShipAddress
        {
            get
            {
                return _ShipAddress;
            }
            set
            {
                _ShipAddress = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="ShipCity", DbType="VarChar(15)")]
        public System.String ShipCity
        {
            get
            {
                return _ShipCity;
            }
            set
            {
                _ShipCity = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="ShipRegion", DbType="VarChar(15)")]
        public System.String ShipRegion
        {
            get
            {
                return _ShipRegion;
            }
            set
            {
                _ShipRegion = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="ShipPostalCode", DbType="VarChar(10)")]
        public System.String ShipPostalCode
        {
            get
            {
                return _ShipPostalCode;
            }
            set
            {
                _ShipPostalCode = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="ShipCountry", DbType="VarChar(15)")]
        public System.String ShipCountry
        {
            get
            {
                return _ShipCountry;
            }
            set
            {
                _ShipCountry = value;
            }
        }

        [Newtonsoft.Json.JsonIgnore()]
        [Association(Storage="_OrderDetails", ThisKey="OrderID", OtherKey="OrderID", IsForeignKey=false, Name="Order_OrderDetail")]
        public EntitySet<OrderDetail> OrderDetails
        {
            get
            {
                return this._OrderDetails;
            }
            set
            {
                this._OrderDetails.Assign(value);
            }
        }

        [Association(Storage="_Customer", ThisKey="CustomerID", OtherKey="CustomerID", IsForeignKey=true, Name="Customer_Order")]
        public Customer Customer
        {
            get
            {
                return this._Customer.Entity;
            }
            set
            {
                this._Customer.Entity = value;
            }
        }

        [Newtonsoft.Json.JsonIgnore()]
        [Association(Storage="_Employee", ThisKey="EmployeeID", OtherKey="EmployeeID", IsForeignKey=true, Name="Employee_Order")]
        public Employee Employee
        {
            get
            {
                return this._Employee.Entity;
            }
            set
            {
                this._Employee.Entity = value;
            }
        }

        [Newtonsoft.Json.JsonIgnore()]
        [Association(Storage="_Shipper", ThisKey="ShipVia", OtherKey="ShipperID", IsForeignKey=true, Name="Shipper_Order")]
        public Shipper Shipper
        {
            get
            {
                return this._Shipper.Entity;
            }
            set
            {
                this._Shipper.Entity = value;
            }
        }
    }
}
