using System;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Table(Name="Region")]
    public partial class Region
    {
        private System.Int32 _RegionID;
        private System.String _RegionDescription;
        private EntitySet<Territory> _Territories;

        public Region()
        {
            this._Territories = new EntitySet<Territory>();
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, IsPrimaryKey=true, AutoSync=AutoSync.Never, Name="RegionID")]
        public System.Int32 RegionID
        {
            get
            {
                return _RegionID;
            }
            set
            {
                _RegionID = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="RegionDescription", DbType="VarChar(50)")]
        public System.String RegionDescription
        {
            get
            {
                return _RegionDescription;
            }
            set
            {
                _RegionDescription = value;
            }
        }

        [Association(Storage="_Territories", ThisKey="RegionID", OtherKey="RegionID", IsForeignKey=false, Name="Region_Territory")]
        public EntitySet<Territory> Territories
        {
            get
            {
                return this._Territories;
            }
            set
            {
                this._Territories.Assign(value);
            }
        }
    }
}
