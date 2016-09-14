using System;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Table(Name="CustomerCustomerDemo")]
    public partial class CustomerCustomerDemo
    {
        private System.String _CustomerID;
        private System.String _CustomerTypeID;
        private EntityRef<CustomerDemographic> _CustomerDemographic;
        private EntityRef<Customer> _Customer;

        public CustomerCustomerDemo()
        {
            this._CustomerDemographic = default(EntityRef<CustomerDemographic>);
            this._Customer = default(EntityRef<Customer>);
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

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, IsPrimaryKey=true, AutoSync=AutoSync.Never, Name="CustomerTypeID", DbType="VarChar(10)")]
        public System.String CustomerTypeID
        {
            get
            {
                return _CustomerTypeID;
            }
            set
            {
                _CustomerTypeID = value;
            }
        }

        [Association(Storage="_CustomerDemographic", ThisKey="CustomerTypeID", OtherKey="CustomerTypeID", IsForeignKey=true, Name="CD_CCD")]
        public CustomerDemographic CustomerDemographic
        {
            get
            {
                return this._CustomerDemographic.Entity;
            }
            set
            {
                this._CustomerDemographic.Entity = value;
            }
        }

        [System.Web.Script.Serialization.ScriptIgnore()]
        [Association(Storage="_Customer", ThisKey="CustomerID", OtherKey="CustomerID", IsForeignKey=true, Name="C_CCD")]
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
    }
}
