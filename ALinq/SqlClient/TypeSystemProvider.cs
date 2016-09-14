using System;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace ALinq.SqlClient
{
    internal abstract class TypeSystemProvider : ITypeSystemProvider
    {
        // Methods
        protected TypeSystemProvider()
        {
        }

        public abstract IProviderType ChangeTypeFamilyTo(IProviderType type, IProviderType typeWithFamily);
        public abstract IProviderType From(object o);
        public abstract IProviderType From(Type runtimeType);
        public abstract IProviderType From(Type type, int? size);
        public abstract IProviderType GetApplicationType(int index);
        public abstract IProviderType GetBestLargeType(IProviderType type);
        public abstract IProviderType GetBestType(IProviderType typeA, IProviderType typeB);
        public abstract void InitializeParameter(IProviderType type, DbParameter parameter, object value);
        public abstract IProviderType MostPreciseTypeInFamily(IProviderType type);
        public abstract IProviderType Parse(string text);
        public abstract IProviderType PredictTypeForBinary(SqlNodeType binaryOp, IProviderType leftType, IProviderType rightType);
        public abstract IProviderType PredictTypeForUnary(SqlNodeType unaryOp, IProviderType operandType);
        public abstract IProviderType ReturnTypeOfFunction(SqlFunctionCall functionCall);
    }

    internal abstract class TypeSystemProvider<T> : ITypeSystemProvider where T : IProviderType
    {
        internal abstract T ChangeTypeFamilyTo(T type, T typeWithFamily);
        internal abstract T From(object o);
        internal abstract T From(Type runtimeType);
        internal abstract T From(Type type, int? size);
        internal abstract T GetApplicationType(int index);
        internal abstract T GetBestLargeType(T type);
        internal abstract T GetBestType(T typeA, T typeB);
        internal abstract void InitializeParameter(T type, DbParameter parameter, object value);
        internal abstract T MostPreciseTypeInFamily(T type);
        internal abstract T Parse(string text);
        internal abstract T PredictTypeForBinary(SqlNodeType binaryOp, T leftType, T rightType);
        internal abstract T PredictTypeForUnary(SqlNodeType unaryOp, T operandType);
        internal abstract T ReturnTypeOfFunction(SqlFunctionCall functionCall);

        #region ITypeSystemProvider Members

        IProviderType ITypeSystemProvider.ChangeTypeFamilyTo(IProviderType type, IProviderType typeWithFamily)
        {
            return ChangeTypeFamilyTo((T)type, (T)typeWithFamily);
        }

        IProviderType ITypeSystemProvider.From(object o)
        {
            return From(o);
        }

        IProviderType ITypeSystemProvider.From(Type runtimeType)
        {
            return From(runtimeType);
        }

        IProviderType ITypeSystemProvider.From(Type type, int? size)
        {
            return From(type, size);
        }

        IProviderType ITypeSystemProvider.GetApplicationType(int index)
        {
            return GetApplicationType(index);
        }

        IProviderType ITypeSystemProvider.GetBestLargeType(IProviderType type)
        {
            return GetBestLargeType((T)type);
        }

        IProviderType ITypeSystemProvider.GetBestType(IProviderType typeA, IProviderType typeB)
        {
            return GetBestType((T)typeA, (T)typeB);
        }

        void ITypeSystemProvider.InitializeParameter(IProviderType type, DbParameter parameter, object value)
        {
            InitializeParameter((T)type, parameter, value);
        }

        IProviderType ITypeSystemProvider.MostPreciseTypeInFamily(IProviderType type)
        {
            return MostPreciseTypeInFamily((T)type);
        }

        IProviderType ITypeSystemProvider.Parse(string text)
        {
            return Parse(text);
        }

        IProviderType ITypeSystemProvider.PredictTypeForBinary(SqlNodeType binaryOp, IProviderType leftType, IProviderType rightType)
        {
            return PredictTypeForBinary(binaryOp, (T)leftType, (T)rightType);
        }

        IProviderType ITypeSystemProvider.PredictTypeForUnary(SqlNodeType unaryOp, IProviderType operandType)
        {
            return PredictTypeForUnary(unaryOp, (T) operandType);
        }

        IProviderType ITypeSystemProvider.ReturnTypeOfFunction(SqlFunctionCall functionCall)
        {
            return ReturnTypeOfFunction(functionCall);
        }

        #endregion
    }


}