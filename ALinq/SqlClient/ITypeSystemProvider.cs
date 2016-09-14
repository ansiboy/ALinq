using System;
using System.Data.Common;

namespace ALinq.SqlClient
{
    //public interface IDbTypeSystem
    //{
    //    IProviderType Parse(string text);
    //}

    internal interface ITypeSystemProvider 
    {
        IProviderType ChangeTypeFamilyTo(IProviderType type, IProviderType typeWithFamily);
        IProviderType From(object o);
        IProviderType From(Type runtimeType);
        IProviderType From(Type type, int? size);
        IProviderType GetApplicationType(int index);
        IProviderType GetBestLargeType(IProviderType type);
        IProviderType GetBestType(IProviderType typeA, IProviderType typeB);
        void InitializeParameter(IProviderType type, DbParameter parameter, object value);
        IProviderType MostPreciseTypeInFamily(IProviderType type);
        IProviderType Parse(string text);
        IProviderType PredictTypeForBinary(SqlNodeType binaryOp, IProviderType leftType, IProviderType rightType);
        IProviderType PredictTypeForUnary(SqlNodeType unaryOp, IProviderType operandType);
        IProviderType ReturnTypeOfFunction(SqlFunctionCall functionCall);
    }
}