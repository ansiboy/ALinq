using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Data;
using ALinq.EffiProz;
using ALinq.SqlClient;

namespace ALinq.EffiProz
{
    class EfzDataTypeProvider : DbTypeProviderBase<EfzDataType, DbType>
    {
        private Dictionary<TypeCode, DbType> typeMapping;
        protected override Dictionary<TypeCode, DbType> TypeMapping
        {
            get
            {
                if (typeMapping == null)
                {
                    typeMapping = new Dictionary<TypeCode, DbType>
                                      {
                                          {TypeCode.Boolean, DbType.Boolean},
                                          {TypeCode.Byte, DbType.Byte},
                                          {TypeCode.Char, DbType.AnsiStringFixedLength},
                                          {TypeCode.DateTime, DbType.DateTime},
                                          {TypeCode.Decimal, DbType.Decimal},
                                          {TypeCode.Double, DbType.Double},
                                          {TypeCode.Int16, DbType.Int16},
                                          {TypeCode.Int32, DbType.Int32},
                                          {TypeCode.Int64, DbType.Int64},
                                          {TypeCode.Object, DbType.Binary},
                                          {TypeCode.SByte, DbType.Int16},
                                          {TypeCode.Single, DbType.Single},
                                          {TypeCode.String, DbType.String},
                                          {TypeCode.UInt16, DbType.UInt16},
                                          {TypeCode.UInt32, DbType.UInt32},
                                          {TypeCode.UInt64, DbType.Decimal},
                                      };
                }
                return typeMapping;
            }
        }

        //private static EfzDataType guidType;

        //internal override EfzDataType GuidType
        //{
        //    get
        //    {
        //        if (guidType == null)
        //        {
        //            guidType = CreateSqlType(DbType.String);
        //        }
        //        return guidType;
        //    }
        //}

        //private static EfzDataTypeProvider instance;
        //public static EfzDataTypeProvider Instance
        //{
        //    get
        //    {
        //        if (instance == null)
        //            instance = new EfzDataTypeProvider();
        //        return instance;
        //    }
        //}

        //internal override EfzDataType ReturnTypeOfFunction(SqlFunctionCall functionCall)
        //{
        //    switch (functionCall.Name)
        //    {
        //        case "strftime":
        //            return SqlTypes[TypeCode.String];
        //    }
        //    return base.ReturnTypeOfFunction(functionCall);
        //}

        protected override EfzDataType Parse(string typeName, int size, int scale, string[] options)
        {
            Debug.Assert(typeName == typeName.ToUpper());
            switch (typeName.ToUpper())
            {
                case "CHAR":
                case "VARCHAR":
                case "TEXT":
                    return From(typeof(string), size);
                case "BINARY":
                    return From(typeof(byte[]), size);
                case "NUMBER":
                    if (scale == SqlDataType<DbType>.NULL_VALUE)
                        return From(typeof(decimal), size);
                    return From(typeof(decimal), size, scale);
                case "DECIMAL":
                    if (size == SqlDataType<DbType>.NULL_VALUE)
                        size = DEFAULT_DECIMAL_PRECISION;

                    if (scale == SqlDataType<DbType>.NULL_VALUE)
                        scale = DEFAULT_DECIMAL_SCALE;

                    return From(typeof(decimal), size, scale);

                case "INT":
                case "INTEGER":
                    return From(typeof(Int32));

            }
            if (!Enum.GetNames(typeof(DbType)).Select(n => n.ToUpperInvariant())
                     .Contains(typeName.ToUpperInvariant()))
            {
                throw SqlClient.Error.InvalidProviderType(typeName);
            }
            var type = Enum.Parse(typeof(DbType), typeName, true);
            EfzDataType item;

            if (scale > 0)
                item = CreateSqlType((DbType)type, size, scale);
            else
                item = CreateSqlType((DbType)type, size);
            return item;
        }

        internal override void InitializeParameter(EfzDataType type, System.Data.Common.DbParameter parameter, object value)
        {
            base.InitializeParameter(type, parameter, value);
        }

    }
}