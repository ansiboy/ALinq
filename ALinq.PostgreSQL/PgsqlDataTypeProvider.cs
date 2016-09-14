using System;
using System.Collections.Generic;
using ALinq.SqlClient;
using NpgsqlTypes;

namespace ALinq.PostgreSQL
{
    class PgsqlDataTypeProvider : DbTypeProviderBase<PgsqlDataType, NpgsqlDbType>
    {
        private static Dictionary<TypeCode, NpgsqlDbType> typeMapping;
        private static readonly object objLock = new object();

        protected override Dictionary<TypeCode, NpgsqlDbType> TypeMapping
        {
            get
            {
                if (typeMapping == null)
                {
                    lock (objLock)
                    {
                        typeMapping = new Dictionary<TypeCode, NpgsqlDbType>
                                          {
                                              { TypeCode.Boolean, NpgsqlDbType.Boolean },
                                              { TypeCode.Byte, NpgsqlDbType.Smallint },
                                              { TypeCode.Char, NpgsqlDbType.Char },
                                              { TypeCode.DateTime, NpgsqlDbType.Date },
                                              { TypeCode.Decimal, NpgsqlDbType.Numeric },
                                              { TypeCode.Double, NpgsqlDbType.Double },
                                              { TypeCode.Int16, NpgsqlDbType.Smallint },
                                              { TypeCode.Int32, NpgsqlDbType.Integer },
                                              { TypeCode.Int64, NpgsqlDbType.Bigint },
                                              { TypeCode.Object, NpgsqlDbType.Bytea },
                                              { TypeCode.SByte, NpgsqlDbType.Smallint },
                                              { TypeCode.Single, NpgsqlDbType.Real },
                                              { TypeCode.String, NpgsqlDbType.Varchar },
                                              { TypeCode.UInt16, NpgsqlDbType.Integer },
                                              { TypeCode.UInt32, NpgsqlDbType.Bigint },
                                              { TypeCode.UInt64, NpgsqlDbType.Numeric }
                                          };

                    }
                }
                return typeMapping;
            }
        }

        private static PgsqlDataType guidType;
        internal override PgsqlDataType GuidType
        {
            get
            {
                if (guidType == null)
                {
                    guidType = new PgsqlDataType(TypeMapping, NpgsqlDbType.Uuid);
                }
                return guidType;
            }
        }

        private static PgsqlDataTypeProvider instance;
        internal static PgsqlDataTypeProvider Instance
        {
            get
            {
                if (instance == null)
                    instance = new PgsqlDataTypeProvider();
                return instance;
            }
        }

        //internal override PgsqlDataType From(Type type, int? size, int? scale)
        //{
        //    if (type == typeof(TimeSpan))
        //    {
        //        return SqlTypes[TypeCode.DateTime]; //SqlTypeSystem.Create(System.Data.SqlDbType.BigInt);
        //    }
        //    return base.From(type, size, scale);
        //}
    }
}
