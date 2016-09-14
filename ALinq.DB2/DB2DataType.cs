using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ALinq.SqlClient;
using System.Data.OleDb;
using IBM.Data.DB2;

namespace ALinq.DB2
{
    class DB2DataType : SqlClient.SqlDataType<DB2Type>, IProviderType
    {
        public DB2DataType(Dictionary<TypeCode, DB2Type> typeMapping)
            : base(typeMapping)
        {

        }

        public DB2DataType(Dictionary<TypeCode, DB2Type> typeMapping, DB2Type dbType)
            : base(typeMapping, dbType)
        {
            if (dbType == DB2Type.Decimal || dbType == DB2Type.DecimalFloat)
            {
                Precision = 29;
                Scale = 4;
            }
        }

        public DB2DataType(Dictionary<TypeCode, DB2Type> typeMapping, DB2Type dbType, int precision, int scale)
            : base(typeMapping, dbType, precision == NULL_VALUE ? 20 : precision, scale == NULL_VALUE ? 20 : scale)
        {
        }

        public DB2DataType(Dictionary<TypeCode, DB2Type> typeMapping, DB2Type dbType, int size)
            : base(typeMapping, dbType, size == NULL_VALUE ? 20 : size)
        {
        }

        public override Type GetClosestRuntimeType()
        {
            if (RuntimeOnlyType != null)
            {
                return RuntimeOnlyType;
            }
            switch (SqlDbType)
            {
                case DB2Type.BigInt:
                    return typeof(long);


                case DB2Type.Date:
                case DB2Type.Time:
                case DB2Type.Timestamp:
                    return typeof(DateTime);

                case DB2Type.Decimal:
                case DB2Type.Numeric:
                case DB2Type.DecimalFloat:
                    return typeof(decimal);

                case DB2Type.Float:
                case DB2Type.Real:
                    return typeof(Single);

                case DB2Type.Binary:
                case DB2Type.Blob:
                case DB2Type.Graphic:
                case DB2Type.LongVarBinary:
                case DB2Type.VarBinary:
                    return typeof(byte[]);

                case DB2Type.Integer:
                    return typeof(int);

                case DB2Type.SmallInt:
                    return typeof(Int16);

                case DB2Type.Clob:
                case DB2Type.Char:
                case DB2Type.LongVarChar:
                case DB2Type.VarChar:
                    return typeof(string);

                case DB2Type.Datalink:
                    break;
                case DB2Type.DbClob:
                    break;
                case DB2Type.Double:
                    return typeof(double);
            }
            return typeof(object);
        }

        protected override int GetTypeCoercionPrecedence(DB2Type type)
        {
            switch (type)
            {
                case DB2Type.BigInt:
                    return 0;
                case DB2Type.Binary:
                    return 1;
                case DB2Type.Blob:
                    return 2;
                case DB2Type.Char:
                    return 3;
                case DB2Type.Clob:
                    return 4;
                case DB2Type.Datalink:
                    return 5;
                case DB2Type.Date:
                    return 6;
                case DB2Type.DbClob:
                    return 7;
                case DB2Type.Decimal:
                    return 8;
                case DB2Type.DecimalFloat:
                    return 9;
                case DB2Type.Double:
                    return 10;
                case DB2Type.Float:
                    return 11;
                case DB2Type.Graphic:
                    return 12;
                case DB2Type.Integer:
                    return 13;
                case DB2Type.Invalid:
                    return 14;
                case DB2Type.LongVarBinary:
                    return 15;
                case DB2Type.LongVarChar:
                    return 16;
                case DB2Type.LongVarGraphic:
                    return 17;
                case DB2Type.Numeric:
                    return 18;
                case DB2Type.Real:
                    return 19;
                case DB2Type.Real370:
                    return 20;
                case DB2Type.RowId:
                    return 21;
                case DB2Type.SmallInt:
                    return 22;
                case DB2Type.Time:
                    return 23;
                case DB2Type.Timestamp:
                    return 24;
                case DB2Type.VarBinary:
                    return 25;
                case DB2Type.VarChar:
                    return 26;
                case DB2Type.VarGraphic:
                    return 27;
                case DB2Type.Xml:
                    return 28;
            }
            throw SqlClient.Error.UnexpectedTypeCode(type);
        }

        internal override DBTypeCategory Category
        {
            get
            {
                switch (SqlDbType)
                {
                    case DB2Type.BigInt:
                    case DB2Type.Decimal:
                    case DB2Type.Double:
                    case DB2Type.Numeric:
                    case DB2Type.SmallInt:
                    case DB2Type.Integer:
                    case DB2Type.Real:
                    case DB2Type.Real370:
                        return DBTypeCategory.Numeric;

                    case DB2Type.Binary:
                    case DB2Type.LongVarBinary:
                    case DB2Type.VarBinary:
                        return DBTypeCategory.Binary;

                    case DB2Type.LongVarChar:
                    case DB2Type.VarChar:
                    case DB2Type.Char:
                        return DBTypeCategory.Text;

                    case DB2Type.Date:
                    case DB2Type.Time:
                    case DB2Type.Timestamp:
                        return DBTypeCategory.DateTime;
                }
                throw SqlClient.Error.UnexpectedTypeCode(this);
            }
        }

        public override bool HasPrecisionAndScale
        {
            get
            {
                switch (SqlDbType)
                {
                    case DB2Type.Decimal:
                    case DB2Type.DecimalFloat:
                    case DB2Type.Numeric:
                        return true;
                }
                return false;
            }
        }

        public override bool IsFixedSize
        {
            get
            {
                switch (SqlDbType)
                {
                    case DB2Type.Binary:
                    case DB2Type.LongVarBinary:
                    case DB2Type.LongVarChar:
                    case DB2Type.Numeric:
                    case DB2Type.VarBinary:
                    case DB2Type.VarChar:
                        return false;
                }
                return true;
            }
        }

        public override bool IsLargeType
        {
            get
            {
                switch (SqlDbType)
                {
                    case DB2Type.Binary:
                    case DB2Type.LongVarChar:
                    case DB2Type.LongVarBinary:
                        return true;
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
                    case DB2Type.BigInt:
                    case DB2Type.Decimal:
                    case DB2Type.Double:
                    case DB2Type.Numeric:
                    case DB2Type.SmallInt:
                    case DB2Type.Integer:
                        return true;
                }
                return false;
            }
        }

        public override string ToQueryString(QueryFormatOptions formatFlags)
        {
            switch (SqlDbType)
            {
                case DB2Type.VarChar:
                case DB2Type.LongVarChar:
                    if (Size == STRING_SIZE)
                        return "VarChar(" + STRING_SIZE + ")";
                    return string.Format("VarChar({0})", Size);
                case DB2Type.Decimal:
                    return string.Format("Decimal({0},{1})", Precision, Scale);
                case DB2Type.Numeric:
                    if (Scale == 0)
                        return string.Format("Numeric({0})", Precision);

                    return string.Format("Numeric({0},{1})", Precision, Scale);
                case DB2Type.BigInt:
                    return "BigInt";
                case DB2Type.Binary:
                    return "Blob";
                case DB2Type.Char:
                    return string.Format("Char({0})", Size);
            }
            return base.ToQueryString(formatFlags);
        }

    }
}