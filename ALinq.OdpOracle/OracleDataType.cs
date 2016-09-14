using System;
using System.Collections.Generic;
using ALinq.SqlClient;
using Oracle.DataAccess.Client;

namespace ALinq.Oracle.Odp
{
    class OracleDataType : SqlDataType<OracleDbType>
    {
        public OracleDataType(Dictionary<TypeCode, OracleDbType> typeMapping)
            : base(typeMapping)
        {
        }

        public OracleDataType(Dictionary<TypeCode, OracleDbType> typeMapping, OracleDbType dbType)
            : base(typeMapping, dbType)
        {
            if (dbType == OracleDbType.Decimal)
            {
                Precision = 29;
                Scale = 4;
            }
        }

        protected override bool IsUnicode(OracleDbType dbType)
        {
            switch (dbType)
            {
                case OracleDbType.NChar:
                case OracleDbType.NClob:
                case OracleDbType.NVarchar2:
                    return true;
                default:
                    return false;
            }
        }

        public OracleDataType(Dictionary<TypeCode, OracleDbType> typeMapping, OracleDbType dbType, int precision, int scale)
            : base(typeMapping, dbType, precision, scale)
        {
        }

        public OracleDataType(Dictionary<TypeCode, OracleDbType> typeMapping, OracleDbType dbType, int size)
            : base(typeMapping, dbType, size)
        {
        }

        public override Type GetClosestRuntimeType()
        {
            if (RuntimeOnlyType != null)
            {
                return RuntimeOnlyType;
            }

            if (this == OracleDataTypeProvider.Instance.GuidType)
                return typeof(Guid);

            switch (SqlDbType)
            {

                case OracleDbType.Byte:
                    return typeof(byte);

                case OracleDbType.Decimal:
                    return typeof(decimal);
                case OracleDbType.Int16:
                    return typeof(Int16);
                case OracleDbType.Int32:
                    return typeof(Int32);
                case OracleDbType.Int64:
                    return typeof(decimal);

                case OracleDbType.Double:
                    return typeof(double);
                case OracleDbType.Single:
                    return typeof(float);

                //二进制
                case OracleDbType.LongRaw:
                case OracleDbType.BFile:
                case OracleDbType.Blob:
                case OracleDbType.Raw:
                    return typeof(byte[]);
                //return typeof (ALinq.Binary);

                //时间
                case OracleDbType.Date:
                    return typeof(DateTime);
                case OracleDbType.TimeStamp:
                    return typeof(DateTime);
                case OracleDbType.TimeStampLTZ:
                    return typeof(DateTime);
                case OracleDbType.TimeStampTZ:
                    return typeof(DateTime);
                case OracleDbType.IntervalDS:
                    return typeof(TimeSpan);
                case OracleDbType.IntervalYM:
                    return typeof(Int32);

                //字符串
                case OracleDbType.Long:
                case OracleDbType.NClob:
                case OracleDbType.Clob:
                case OracleDbType.XmlType:
                    return typeof(string);

                case OracleDbType.NVarchar2:
                case OracleDbType.Varchar2:
                case OracleDbType.Char:
                case OracleDbType.NChar:
                    if (Size == 1)
                        return typeof(char);
                    return typeof(string);
            }
            return typeof(object);
        }

        protected override int GetTypeCoercionPrecedence(OracleDbType type)
        {
            switch (type)
            {
                case OracleDbType.BFile:
                    return 0;
                case OracleDbType.Blob:
                    return 1;
                case OracleDbType.Clob:
                    return 2;
                case OracleDbType.Raw:
                    return 3;
                case OracleDbType.LongRaw:
                    return 4;

                case OracleDbType.Char:
                    return 11;
                case OracleDbType.Varchar2:
                    return 12;
                case OracleDbType.NChar:
                    return 13;
                case OracleDbType.NVarchar2:
                    return 14;

                case OracleDbType.Byte:
                    return 20;
                case OracleDbType.Int16:
                    return 21;
                case OracleDbType.Int32:
                    return 22;
                case OracleDbType.Int64:
                    return 23;
                case OracleDbType.Single:
                    return 24;
                case OracleDbType.Double:
                    return 25;
                case OracleDbType.Decimal:
                    return 26;
                case OracleDbType.Date:
                    return 27;

                case OracleDbType.RefCursor:
                    return 30;
            }
            throw SqlClient.Error.UnexpectedTypeCode(type);
        }


        internal override DBTypeCategory Category
        {
            get
            {
                switch (SqlDbType)
                {
                    case OracleDbType.BFile:
                    case OracleDbType.Blob:
                    case OracleDbType.LongRaw:
                    case OracleDbType.NClob:
                    case OracleDbType.Raw:
                        return DBTypeCategory.Binary;

                    case OracleDbType.Byte:
                    case OracleDbType.Decimal:
                    case OracleDbType.Double:
                    case OracleDbType.Int16:
                    case OracleDbType.Int32:
                    case OracleDbType.Int64:
                    case OracleDbType.Single:
                        return DBTypeCategory.Numeric;

                    case OracleDbType.Long:
                    case OracleDbType.Char:
                    case OracleDbType.NChar:
                    case OracleDbType.NVarchar2:
                    case OracleDbType.Varchar2:
                    case OracleDbType.XmlType:
                    case OracleDbType.Clob:
                        return DBTypeCategory.Text;

                    case OracleDbType.RefCursor:
                        return DBTypeCategory.Cursor;

                    case OracleDbType.Date:
                    case OracleDbType.TimeStamp:
                    case OracleDbType.TimeStampTZ:
                    case OracleDbType.TimeStampLTZ:
                        return DBTypeCategory.DateTime;

                    default:
                        return DBTypeCategory.Binary;
                }
            }
        }

        public override bool HasPrecisionAndScale
        {
            get
            {
                if (SqlDbType == OracleDbType.Decimal)
                    return true;
                return false;
            }
        }

        public override bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public override bool IsLargeType
        {
            get
            {
                //switch (SqlDbType)
                //{
                //    case OracleDbType.Varchar2:
                //    case OracleDbType.NVarchar2:
                //    case OracleDbType.Char:
                //    case OracleDbType.Date:
                //    case OracleDbType.Byte:
                //    case OracleDbType.Decimal:
                //    case OracleDbType.Double:
                //    case OracleDbType.Int16:
                //    case OracleDbType.Int32:
                //    case OracleDbType.Int64:
                //    case OracleDbType.Single:
                //    case OracleDbType.Raw:
                //        return false;
                //}
                //return true;
                switch (SqlDbType)
                {
                    case OracleDbType.Blob:
                    case OracleDbType.Clob:
                    case OracleDbType.NClob:
                        return true;
                    case OracleDbType.Char:
                    case OracleDbType.Varchar2:
                        if (Size != 0 & Size > 2000)
                            return true;
                        return false;
                }
                return false;
            }
        }

        public override bool IsNumeric
        {
            get
            {
                switch (SqlDbType)
                {
                    case OracleDbType.Byte:
                    case OracleDbType.Decimal:
                    case OracleDbType.Double:
                    case OracleDbType.Single:
                    case OracleDbType.Int16:
                    case OracleDbType.Int32:
                    case OracleDbType.Int64:
                        return true;
                }
                return false;
            }
        }



        //public override OracleDbType SqlDbType
        //{
        //    get { return (OracleDbType)sqlDbType; }
        //    set { sqlDbType = (int)value; }
        //}


    }
}
