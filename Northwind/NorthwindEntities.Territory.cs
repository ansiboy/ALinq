using System;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Table(Name="Territories")]
    public partial class Territory
    {
        private System.String _TerritoryID;
        private System.String _TerritoryDescription;
        private System.Int32 _RegionID;
        private EntitySet<EmployeeTerritory> _EmployeeTerritories;
        private EntityRef<Region> _Region;

        public Territory()
        {
            this._EmployeeTerritories = new EntitySet<EmployeeTerritory>();
            this._Region = default(EntityRef<Region>);
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, IsPrimaryKey=true, AutoSync=AutoSync.Never, Name="TerritoryID", DbType="VarChar(20)")]
        public System.String TerritoryID
        {
            get
            {
                return _TerritoryID;
            }
            set
            {
                _TerritoryID = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="TerritoryDescription", DbType="VarChar(50)")]
        public System.String TerritoryDescription
        {
            get
            {
                return _TerritoryDescription;
            }
            set
            {
                _TerritoryDescription = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="RegionID")]
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

        [Association(Storage="_EmployeeTerritories", ThisKey="TerritoryID", OtherKey="TerritoryID", IsForeignKey=false, Name="Territory_EmployeeTerritory")]
        public EntitySet<EmployeeTerritory> EmployeeTerritories
        {
            get
            {
                return this._EmployeeTerritories;
            }
            set
            {
                this._EmployeeTerritories.Assign(value);
            }
        }

        [Association(Storage="_Region", ThisKey="RegionID", OtherKey="RegionID", IsForeignKey=true, Name="Region_Territory")]
        public Region Region
        {
            get
            {
                return this._Region.Entity;
            }
            set
            {
                this._Region.Entity = value;
            }
        }
    }
}
