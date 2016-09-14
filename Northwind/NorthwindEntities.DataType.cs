using System;
using ALinq;
using ALinq.Mapping;

namespace NorthwindDemo
{
    [Table(Name="DataType")]
    public partial class DataType
    {
        private System.Int32 _ID;
        private Nullable<System.Int64> _Int64;
        private Nullable<System.UInt64> _UInt64;
        private NorthwindDatabase.Enum _Enum;
        private Nullable<System.Char> _Char;
        private System.Xml.Linq.XDocument _XDocument;
        private System.Xml.Linq.XElement _XElement;
        private Guid _Guid;
        private Nullable<System.DateTime> _DateTime;

        public DataType()
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

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, Name="Int64")]
        public Nullable<System.Int64> Int64
        {
            get
            {
                return _Int64;
            }
            set
            {
                _Int64 = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, Name="UInt64")]
        public Nullable<System.UInt64> UInt64
        {
            get
            {
                return _UInt64;
            }
            set
            {
                _UInt64 = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, Name="Enum")]
        public NorthwindDatabase.Enum Enum
        {
            get
            {
                return _Enum;
            }
            set
            {
                _Enum = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, Name="Char", DbType="Char(1)")]
        public Nullable<System.Char> Char
        {
            get
            {
                return _Char;
            }
            set
            {
                _Char = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, Name="XDocument")]
        public System.Xml.Linq.XDocument XDocument
        {
            get
            {
                return _XDocument;
            }
            set
            {
                _XDocument = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, Name="XElement")]
        public System.Xml.Linq.XElement XElement
        {
            get
            {
                return _XElement;
            }
            set
            {
                _XElement = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never,  Name="Guid")]
        public Guid Guid
        {
            get
            {
                return _Guid;
            }
            set
            {
                _Guid = value;
            }
        }

        [Column(CanBeNull=true, UpdateCheck=UpdateCheck.Never, Name="DateTime")]
        public Nullable<System.DateTime> DateTime
        {
            get
            {
                return _DateTime;
            }
            set
            {
                _DateTime = value;
            }
        }
    }
}
