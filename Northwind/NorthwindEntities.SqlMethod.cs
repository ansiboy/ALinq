using System;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Table(Name="SqlMethod")]
    public partial class SqlMethod
    {
        private System.Int32 _ID;
        private System.String _Name;

        public SqlMethod()
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

        [Column(CanBeNull=false, UpdateCheck=UpdateCheck.Never, AutoSync=AutoSync.Never, Name="Name")]
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
