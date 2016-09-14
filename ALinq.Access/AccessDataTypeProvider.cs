using System;
using System.Collections.Generic;
using ALinq.SqlClient;
using System.Data.OleDb;

namespace ALinq.Access
{
    class AccessDataTypeProvider : DbTypeProviderBase<AccessDataType, OleDbType>
    {
        private Dictionary<TypeCode, OleDbType> typeMapping;

        protected override Dictionary<TypeCode, OleDbType> TypeMapping
        {
            get
            {
                if (typeMapping == null)
                {
                    typeMapping = new Dictionary<TypeCode, OleDbType>()
                                      {
                                          {TypeCode.Boolean, OleDbType.Boolean},
                                          {TypeCode.Byte, OleDbType.UnsignedTinyInt},
                                          {TypeCode.Char, OleDbType.Char},
                                          {TypeCode.DateTime, OleDbType.Date},
                                          {TypeCode.Decimal, OleDbType.Decimal},
                                          {TypeCode.Double,OleDbType.Double},
                                          {TypeCode.Int16,OleDbType.SmallInt},
                                          {TypeCode.Int32, OleDbType.Integer},
                                          {TypeCode.Int64, OleDbType.BigInt},
                                          {TypeCode.Object, OleDbType.Binary},
                                          {TypeCode.SByte, OleDbType.SmallInt},
                                          {TypeCode.Single, OleDbType.Single},
                                          {TypeCode.String, OleDbType.VarChar},
                                          {TypeCode.UInt16, OleDbType.Integer},
                                          {TypeCode.UInt32, OleDbType.Integer},
                                          {TypeCode.UInt64, OleDbType.Decimal},
                                      };
                }
                return typeMapping;
            }
        }

        private AccessDataType guidType;
        internal override AccessDataType GuidType
        {
            get
            {
                if (guidType == null)
                {
                    guidType = CreateSqlType(OleDbType.Guid);
                }
                return guidType;
            }
        }

        internal override AccessDataType Parse(string stype)
        {
            if (stype == null)
                throw Error.ArgumentNull(stype);

            stype = stype.ToUpper();
            switch (stype)
            {
                case "BINARY":
                    return CreateSqlType(OleDbType.Binary);
                case "BIGINT":
                    return CreateSqlType(OleDbType.BigInt);
                case "BIT":
                    return CreateSqlType(OleDbType.Boolean);
                case "TINYINT":
                    return CreateSqlType(OleDbType.TinyInt);
                case "MONEY":
                    return CreateSqlType(OleDbType.Currency);
                case "DATE":
                case "DATETIME":
                    return CreateSqlType(OleDbType.Date);
                case "UNIQUEIDENTIFIER":
                    return CreateSqlType(OleDbType.Guid);
                case "REAL":
                    return CreateSqlType(OleDbType.Single);
                case "FLOAT":
                    return CreateSqlType(OleDbType.Double);
                case "SMALLINT":
                    return CreateSqlType(OleDbType.SmallInt);
                case "COUNTER":
                case "INTEGER":
                    return CreateSqlType(OleDbType.Integer);
                case "DECIMAL":
                    return CreateSqlType(OleDbType.Decimal);
                case "TEXT":
                    return CreateSqlType(OleDbType.VarChar, STRING_SIZE);
                case "IMAGE":
                    return CreateSqlType(OleDbType.VarBinary);
                case "CHARACTER":
                    return CreateSqlType(OleDbType.Char, 1);

            }
            return base.Parse(stype);
        }
    }
}
