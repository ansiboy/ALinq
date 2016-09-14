using System;
using System.Collections.Generic;
using ALinq.SqlClient;
using System.Data.OleDb;
using IBM.Data.DB2;

namespace ALinq.DB2
{
    class DB2DataTypeProvider : DbTypeProviderBase<DB2DataType, DB2Type>
    {
        internal const int INT64_TO_DECIMAL_LENGTH = 38;
        internal const int UInt32_TO_DECIMAL_LENGTH = 38;
        internal const int UInt64_TO_DECIMAL_LENGTH = 39;
        private Dictionary<TypeCode, DB2Type> typeMapping;

        protected override Dictionary<TypeCode, DB2Type> TypeMapping
        {
            get
            {
                if (typeMapping == null)
                {
                    typeMapping = new Dictionary<TypeCode, DB2Type>()
                                      {
                                          {TypeCode.Boolean, DB2Type.SmallInt},
                                          {TypeCode.Byte, DB2Type.SmallInt},
                                          {TypeCode.Char, DB2Type.Char},
                                          {TypeCode.DateTime, DB2Type.Timestamp},
                                          {TypeCode.Decimal, DB2Type.Decimal},
                                          {TypeCode.Double,DB2Type.Double},
                                          {TypeCode.Int16,DB2Type.SmallInt},
                                          {TypeCode.Int32, DB2Type.Integer},
                                          {TypeCode.Int64, DB2Type.BigInt},
                                          {TypeCode.Object, DB2Type.Binary},
                                          {TypeCode.SByte, DB2Type.SmallInt},
                                          {TypeCode.Single, DB2Type.Real},
                                          {TypeCode.String, DB2Type.VarChar},
                                          {TypeCode.UInt16, DB2Type.Integer},
                                          {TypeCode.UInt32, DB2Type.BigInt},
                                          {TypeCode.UInt64, DB2Type.Decimal}
                                      };
                }
                return typeMapping;
            }
        }

        //private static readonly object sqlTypesLock = new object();
        //private Dictionary<TypeCode, DB2DataType> sqlTypes;
        //internal override Dictionary<TypeCode, DB2DataType> SqlTypes
        //{
        //    get
        //    {
        //        if (sqlTypes == null)
        //        {
        //            lock (sqlTypesLock)
        //            {
        //                sqlTypes = new Dictionary<TypeCode, DB2DataType>
        //                               {
        //                                   {TypeCode.Boolean, new DB2DataType(TypeMapping, DB2Type.Numeric,1,0)},
        //                                   {TypeCode.Byte, new DB2DataType(TypeMapping, DB2Type.Numeric,3,0)},
        //                                   {TypeCode.Char, new DB2DataType(TypeMapping, DB2Type.Char, 1)},
        //                                   {TypeCode.DateTime, new DB2DataType(TypeMapping, DB2Type.Timestamp)},
        //                                   {TypeCode.Decimal, new DB2DataType(TypeMapping,DB2Type.Decimal,DEFAULT_DECIMAL_PRECISION,DEFAULT_DECIMAL_SCALE)},
        //                                   {TypeCode.Double,new DB2DataType(TypeMapping,DB2Type.Double)},
        //                                   {TypeCode.Int16, new DB2DataType(TypeMapping, DB2Type.SmallInt)},
        //                                   {TypeCode.Int32, new DB2DataType(TypeMapping, DB2Type.Integer)},
        //                                   {TypeCode.Int64, new DB2DataType(TypeMapping, DB2Type.BigInt)},
        //                                   {TypeCode.Object, new DB2DataType(TypeMapping, DB2Type.Binary)},
        //                                   {TypeCode.SByte, new DB2DataType(TypeMapping, DB2Type.SmallInt)},
        //                                   {TypeCode.Single, new DB2DataType(TypeMapping, DB2Type.Real)},
        //                                   {TypeCode.String, new DB2DataType(TypeMapping, DB2Type.VarChar, STRING_SIZE)},
        //                                   {TypeCode.UInt16, new DB2DataType(TypeMapping, DB2Type.Integer)},
        //                                   {TypeCode.UInt32, new DB2DataType(TypeMapping, DB2Type.BigInt)},
        //                                   {TypeCode.UInt64, new DB2DataType(TypeMapping, DB2Type.Numeric,31,0)}
        //                               };
        //            }
        //        }
        //        return sqlTypes;
        //    }
        //}

        internal override DB2DataType Parse(string stype)
        {
            if (stype == null)
                throw Error.ArgumentNull(stype);

            stype = stype.ToUpper();
            switch (stype)
            {
                case "BINARY":
                    return CreateSqlType(DB2Type.Binary);
                //case "BIT":
                //    return CreateSqlType(OleDbType.Boolean);
                //case "TINYINT":
                //    return CreateSqlType(OleDbType.TinyInt);
                //case "MONEY":
                //    return CreateSqlType(OleDbType.Currency);
                //case "DATETIME":
                //    return CreateSqlType(OleDbType.DBTimeStamp);
                //case "UNIQUEIDENTIFIER":
                //    return CreateSqlType(OleDbType.Guid);
                case "SINGLE":
                    return CreateSqlType(DB2Type.Real);
                case "FLOAT":
                    return CreateSqlType(DB2Type.Double);
                //case "SMALLINT":
                //    return CreateSqlType(OleDbType.SmallInt);
                //case "COUNTER":
                //case "INTEGER":
                //    return CreateSqlType(OleDbType.Integer);
                case "NUMERIC":
                case "DECIMAL":
                    return CreateSqlType(DB2Type.Decimal);
                //case "TEXT":
                //    return CreateSqlType(OleDbType.VarChar, STRING_SIZE);
                //case "IMAGE":
                //    return CreateSqlType(OleDbType.VarBinary);
                //case "CHARACTER":
                //    return CreateSqlType(OleDbType.Char, 1);
            }
            return base.Parse(stype);
        }
    }
}
