using System;
using System.Collections.Generic;
using ALinq.SqlClient;
using FirebirdSql.Data.FirebirdClient;

namespace ALinq.Firebird
{
    internal class FirebirdDataTypeProvider : DbTypeProviderBase<FirebirdDataType, FbDbType>
    {
        internal const int INT64_TO_DECIMAL_LENGTH = 38;
        internal const int UInt32_TO_DECIMAL_LENGTH = 38;
        internal const int UInt64_TO_DECIMAL_LENGTH = 39;

        private Dictionary<TypeCode, FbDbType> typeMapping;
        protected override Dictionary<TypeCode, FbDbType> TypeMapping
        {
            get
            {
                if (typeMapping == null)
                {
                    typeMapping = new Dictionary<TypeCode, FbDbType>()
                                      {
                                          {TypeCode.Boolean, FbDbType.SmallInt},
                                          {TypeCode.Byte, FbDbType.SmallInt},
                                          {TypeCode.Char, FbDbType.Char},
                                          {TypeCode.DateTime, FbDbType.TimeStamp},
                                          {TypeCode.Decimal, FbDbType.Decimal},
                                          {TypeCode.Double,FbDbType.Double},
                                          {TypeCode.Int16,FbDbType.SmallInt},
                                          {TypeCode.Int32, FbDbType.Integer},
                                          {TypeCode.Int64, FbDbType.Decimal},
                                          {TypeCode.Object, FbDbType.Binary},
                                          {TypeCode.SByte, FbDbType.SmallInt},
                                          {TypeCode.Single, FbDbType.Float},
                                          {TypeCode.String, FbDbType.VarChar},
                                          {TypeCode.UInt16, FbDbType.Integer},
                                          {TypeCode.UInt32, FbDbType.Integer},
                                          {TypeCode.UInt64, FbDbType.Decimal},
                                      };
                }
                return typeMapping;
            }
        }


        private static readonly object sqlTypesLock = new object();
        private Dictionary<TypeCode, FirebirdDataType> sqlTypes;

        internal override Dictionary<TypeCode, FirebirdDataType> SqlTypes
        {
            get
            {
                if (sqlTypes == null)
                {
                    lock (sqlTypesLock)
                    {
                        sqlTypes = new Dictionary<TypeCode, FirebirdDataType>
                                       {
                                           {TypeCode.Boolean, new FirebirdDataType(TypeMapping, FbDbType.SmallInt)},
                                           {TypeCode.Byte, new FirebirdDataType(TypeMapping, FbDbType.SmallInt)},
                                           {TypeCode.Char, new FirebirdDataType(TypeMapping, FbDbType.Char, 1)},
                                           {TypeCode.DateTime, new FirebirdDataType(TypeMapping, FbDbType.TimeStamp)},
                                           {TypeCode.Decimal, new FirebirdDataType(TypeMapping,FbDbType.Decimal,18,DEFAULT_DECIMAL_SCALE)},
                                           {TypeCode.Double,new FirebirdDataType(TypeMapping,FbDbType.Double)},
                                           {TypeCode.Int16, new FirebirdDataType(TypeMapping, FbDbType.SmallInt)},
                                           {TypeCode.Int32, new FirebirdDataType(TypeMapping, FbDbType.Integer)},
                                           {TypeCode.Int64, new FirebirdDataType(TypeMapping, FbDbType.BigInt)},
                                           {TypeCode.Object, new FirebirdDataType(TypeMapping, FbDbType.Binary)},
                                           {TypeCode.SByte, new FirebirdDataType(TypeMapping, FbDbType.SmallInt)},
                                           {TypeCode.Single, new FirebirdDataType(TypeMapping, FbDbType.Float)},
                                           {TypeCode.String, new FirebirdDataType(TypeMapping, FbDbType.VarChar, STRING_SIZE)},
                                           {TypeCode.UInt16, new FirebirdDataType(TypeMapping, FbDbType.Integer)},
                                           {TypeCode.UInt32, new FirebirdDataType(TypeMapping, FbDbType.BigInt)},
                                           {TypeCode.UInt64, new FirebirdDataType(TypeMapping, FbDbType.BigInt)}
                                       };
                    }
                }
                return sqlTypes;
            }
        }

        protected override FirebirdDataType Parse(string typeName, int size, int scale, string[] options)
        {
            switch (typeName)
            {
                case "BLOB":
                    if (options.Length >= 2)
                    {
                        if (options[0] == "SUB_TYPE" && options[1] == "TEXT")
                        {
                            return From(typeof(string), -1);
                        }
                    }
                    return From(typeof(byte[]));
            }
            return base.Parse(typeName, size, scale, options);
        }
    }
}
