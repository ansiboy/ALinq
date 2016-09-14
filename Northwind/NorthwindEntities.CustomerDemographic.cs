using System;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Table(Name="CustomerDemographics")]
    public partial class CustomerDemographic
    {
        private System.String _CustomerTypeID;
        private System.String _CustomerDesc;
        private EntitySet<CustomerCustomerDemo> _CustomerCustomerDemos;

        public CustomerDemographic()
        {
            this._CustomerCustomerDemos = new EntitySet<CustomerCustomerDemo>();
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

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="CustomerDesc")]
        public System.String CustomerDesc
        {
            get
            {
                return _CustomerDesc;
            }
            set
            {
                _CustomerDesc = value;
            }
        }

        [Association(Storage="_CustomerCustomerDemos", ThisKey="CustomerTypeID", OtherKey="CustomerTypeID", IsForeignKey=false, Name="CD_CCD")]
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
    }
}
