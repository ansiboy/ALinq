using System;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Table(Name="Temps")]
    public partial class Temp
    {
        private System.Int32 _ID;
        private Nullable<System.Boolean> _IsA;
        private Nullable<System.Boolean> _IsB;
        private System.String _Name;

        public Temp()
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

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="IsA")]
        public Nullable<System.Boolean> IsA
        {
            get
            {
                return _IsA;
            }
            set
            {
                _IsA = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="IsB")]
        public Nullable<System.Boolean> IsB
        {
            get
            {
                return _IsB;
            }
            set
            {
                _IsB = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Name", DbType="VarChar(30)")]
        public System.String Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }
    }
}
