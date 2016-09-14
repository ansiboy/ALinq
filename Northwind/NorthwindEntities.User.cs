using System;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Table(Name="User")]
    public partial class User
    {
        private System.Int32 _ID;
        private System.String _Manager;
        private System.String _Assistant;
        private System.String _Department;

        public User()
        {
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, IsPrimaryKey=true, AutoSync=AutoSync.OnInsert, IsDbGenerated=true, Name="ID")]
        public System.Int32 ID
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Manager", DbType="VarChar(30)")]
        public System.String Manager
        {
            get
            {
                return _Manager;
            }
            set
            {
                _Manager = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Assistant", DbType="VarChar(30)")]
        public System.String Assistant
        {
            get
            {
                return _Assistant;
            }
            set
            {
                _Assistant = value;
            }
        }

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Department", DbType="VarChar(30)")]
        public System.String Department
        {
            get
            {
                return _Department;
            }
            set
            {
                _Department = value;
            }
        }
    }
}
