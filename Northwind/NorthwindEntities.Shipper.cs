using System;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Table(Name="Shippers")]
    public partial class Shipper
    {
        private System.Int32 _ShipperID;
        private System.String _CompanyName;
        private System.String _Phone;
        private EntitySet<Order> _Orders;

        public Shipper()
        {
            this._Orders = new EntitySet<Order>();
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, IsPrimaryKey=true, AutoSync=AutoSync.OnInsert, IsDbGenerated=true, Name="ShipperID")]
        public System.Int32 ShipperID
        {
            get
            {
                return _ShipperID;
            }
            set
            {
                _ShipperID = value;
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

        [Association(Storage="_Orders", ThisKey="ShipperID", OtherKey="ShipVia", IsForeignKey=false, Name="Shipper_Order")]
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
