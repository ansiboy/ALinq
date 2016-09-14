using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Transactions;

namespace ALinq.SqlClient
{
    internal static class Error
    {
        // Methods
        public static Exception ArgumentEmpty(object p0)
        {
            return new ArgumentException(Strings.ArgumentEmpty(p0));
        }

        public static Exception ArgumentNull(string paramName)
        {
            return new ArgumentNullException(paramName);
        }

        public static Exception ArgumentOutOfRange(string paramName)
        {
            return new ArgumentOutOfRangeException(paramName);
        }

        public static Exception ArgumentTypeMismatch(object p0)
        {
            return new ArgumentException(Strings.ArgumentTypeMismatch(p0));
        }

        public static Exception ArgumentWrongType(object p0, object p1, object p2)
        {
            return new ArgumentException(Strings.ArgumentWrongType(p0, p1, p2));
        }

        public static Exception ArgumentWrongValue(object p0)
        {
            return new ArgumentException(Strings.ArgumentWrongValue(p0));
        }

        public static Exception BadParameterType(object p0)
        {
            return new NotSupportedException(Strings.BadParameterType(p0));
        }

        public static Exception BadProjectionInSelect()
        {
            return new InvalidOperationException(Strings.BadProjectionInSelect);
        }

        public static Exception BinaryOperatorNotRecognized(object p0)
        {
            return new InvalidOperationException(Strings.BinaryOperatorNotRecognized(p0));
        }

        public static Exception CannotAggregateType(object p0)
        {
            return new NotSupportedException(Strings.CannotAggregateType(p0));
        }

        public static Exception CannotAssignNull(object p0)
        {
            return new InvalidOperationException(Strings.CannotAssignNull(p0));
        }

        public static Exception CannotAssignToMember(object p0)
        {
            return new InvalidOperationException(Strings.CannotAssignToMember(p0));
        }

        public static Exception CannotCompareItemsAssociatedWithDifferentTable()
        {
            return new InvalidOperationException(Strings.CannotCompareItemsAssociatedWithDifferentTable);
        }

        public static Exception CannotConvertToEntityRef(object p0)
        {
            return new InvalidOperationException(Strings.CannotConvertToEntityRef(p0));
        }

        public static Exception CannotDeleteTypesOf(object p0)
        {
            return new InvalidOperationException(Strings.CannotDeleteTypesOf(p0));
        }

        public static Exception CannotEnumerateResultsMoreThanOnce()
        {
            return new InvalidOperationException(Strings.CannotEnumerateResultsMoreThanOnce);
        }

        public static Exception CannotMaterializeEntityType(object p0)
        {
            return new NotSupportedException(Strings.CannotMaterializeEntityType(p0));
        }

        public static Exception CapturedValuesCannotBeSequences()
        {
            return new NotSupportedException(Strings.CapturedValuesCannotBeSequences);
        }

        public static Exception ClassLiteralsNotAllowed(object p0)
        {
            return new InvalidOperationException(Strings.ClassLiteralsNotAllowed(p0));
        }

        public static Exception ClientCaseShouldNotHold(object p0)
        {
            return new InvalidOperationException(Strings.ClientCaseShouldNotHold(p0));
        }

        public static Exception ClrBoolDoesNotAgreeWithSqlType(object p0)
        {
            return new InvalidOperationException(Strings.ClrBoolDoesNotAgreeWithSqlType(p0));
        }

        public static Exception ColumnCannotReferToItself()
        {
            return new InvalidOperationException(Strings.ColumnCannotReferToItself);
        }

        public static Exception ColumnClrTypeDoesNotAgreeWithExpressionsClrType()
        {
            return new InvalidOperationException(Strings.ColumnClrTypeDoesNotAgreeWithExpressionsClrType);
        }

        public static Exception ColumnIsDefinedInMultiplePlaces(object p0)
        {
            return new InvalidOperationException(Strings.ColumnIsDefinedInMultiplePlaces(p0));
        }

        public static Exception ColumnIsNotAccessibleThroughDistinct(object p0)
        {
            return new InvalidOperationException(Strings.ColumnIsNotAccessibleThroughDistinct(p0));
        }

        public static Exception ColumnIsNotAccessibleThroughGroupBy(object p0)
        {
            return new InvalidOperationException(Strings.ColumnIsNotAccessibleThroughGroupBy(p0));
        }

        public static Exception ColumnReferencedIsNotInScope(object p0)
        {
            return new InvalidOperationException(Strings.ColumnReferencedIsNotInScope(p0));
        }

        public static Exception ComparisonNotSupportedForType(object p0)
        {
            return new NotSupportedException(Strings.ComparisonNotSupportedForType(p0));
        }

        public static Exception CompiledQueryAgainstMultipleShapesNotSupported()
        {
            return new NotSupportedException(Strings.CompiledQueryAgainstMultipleShapesNotSupported);
        }

        public static Exception CompiledQueryCannotReturnType(object p0)
        {
            return new InvalidOperationException(Strings.CompiledQueryCannotReturnType(p0));
        }

        public static Exception ConstructedArraysNotSupported()
        {
            return new NotSupportedException(Strings.ConstructedArraysNotSupported);
        }

        public static Exception ContextNotInitialized()
        {
            return new InvalidOperationException(Strings.ContextNotInitialized);
        }

        public static Exception ConvertToCharFromBoolNotSupported()
        {
            return new NotSupportedException(Strings.ConvertToCharFromBoolNotSupported);
        }

        public static Exception ConvertToDateTimeOnlyForDateTimeOrString()
        {
            return new NotSupportedException(Strings.ConvertToDateTimeOnlyForDateTimeOrString);
        }

        public static Exception CouldNotAssignSequence(object p0, object p1)
        {
            return new InvalidOperationException(Strings.CouldNotAssignSequence(p0, p1));
        }

        public static Exception CouldNotConvertToPropertyOrField(object p0)
        {
            return new InvalidOperationException(Strings.CouldNotConvertToPropertyOrField(p0));
        }

        public static Exception CouldNotDetermineCatalogName()
        {
            return new InvalidOperationException(Strings.CouldNotDetermineCatalogName);
        }

        public static Exception CouldNotDetermineDbGeneratedSqlType(object p0)
        {
            return new InvalidOperationException(Strings.CouldNotDetermineDbGeneratedSqlType(p0));
        }

        public static Exception CouldNotDetermineSqlType(object p0)
        {
            return new InvalidOperationException(Strings.CouldNotDetermineSqlType(p0));
        }

        public static Exception CouldNotGetClrType()
        {
            return new InvalidOperationException(Strings.CouldNotGetClrType);
        }

        public static Exception CouldNotGetSqlType()
        {
            return new InvalidOperationException(Strings.CouldNotGetSqlType);
        }

        public static Exception CouldNotHandleAliasRef(object p0)
        {
            return new InvalidOperationException(Strings.CouldNotHandleAliasRef(p0));
        }

        public static Exception CouldNotTranslateExpressionForReading(object p0)
        {
            return new InvalidOperationException(Strings.CouldNotTranslateExpressionForReading(p0));
        }

        public static Exception CreateDatabaseFailedBecauseOfClassWithNoMembers(object p0)
        {
            return new InvalidOperationException(Strings.CreateDatabaseFailedBecauseOfClassWithNoMembers(p0));
        }

        public static Exception CreateDatabaseFailedBecauseOfContextWithNoTables(object p0)
        {
            return new InvalidOperationException(Strings.CreateDatabaseFailedBecauseOfContextWithNoTables(p0));
        }

        public static Exception CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(object p0)
        {
            return new InvalidOperationException(Strings.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(p0));
        }

        public static Exception DatabaseDeleteThroughContext()
        {
            return new InvalidOperationException(Strings.DatabaseDeleteThroughContext);
        }

        public static Exception DeferredMemberWrongType()
        {
            return new InvalidOperationException(Strings.DeferredMemberWrongType);
        }

        public static Exception DidNotExpectAs(object p0)
        {
            return new InvalidOperationException(Strings.DidNotExpectAs(p0));
        }

        public static Exception DidNotExpectTypeBinding()
        {
            return new InvalidOperationException(Strings.DidNotExpectTypeBinding);
        }

        public static Exception DidNotExpectTypeChange(object p0, object p1)
        {
            return new InvalidOperationException(Strings.DidNotExpectTypeChange(p0, p1));
        }

        public static Exception DistributedTransactionsAreNotAllowed()
        {
            return new TransactionPromotionException(Strings.DistributedTransactionsAreNotAllowed);
        }

        public static Exception EmptyCaseNotSupported()
        {
            return new InvalidOperationException(Strings.EmptyCaseNotSupported);
        }

        public static Exception ExceptNotSupportedForHierarchicalTypes()
        {
            return new NotSupportedException(Strings.ExceptNotSupportedForHierarchicalTypes);
        }

        public static Exception ExpectedBitFoundPredicate()
        {
            return new ArgumentException(Strings.ExpectedBitFoundPredicate);
        }

        public static Exception ExpectedClrTypesToAgree(object p0, object p1)
        {
            return new InvalidOperationException(Strings.ExpectedClrTypesToAgree(p0, p1));
        }

        public static Exception ExpectedNoObjectType()
        {
            return new InvalidOperationException(Strings.ExpectedNoObjectType);
        }

        public static Exception ExpectedPredicateFoundBit()
        {
            return new ArgumentException(Strings.ExpectedPredicateFoundBit);
        }

        public static Exception ExpectedQueryableArgument(object p0, object p1, object p2)
        {
            return new ArgumentException(Strings.ExpectedQueryableArgument(p0, p1, p2));
        }

        public static Exception ExpressionNotDeferredQuerySource()
        {
            return new InvalidOperationException(Strings.ExpressionNotDeferredQuerySource);
        }

        public static Exception ExpressionNotSupportedForSqlServerVersion(Collection<string> reasons)
        {
            var builder = new StringBuilder(Strings.CannotTranslateExpressionToSql);
            foreach (string str in reasons)
            {
                builder.AppendLine(str);
            }
            return new NotSupportedException(builder.ToString());
        }

        public static Exception GeneralCollectionMaterializationNotSupported()
        {
            return new NotSupportedException(Strings.GeneralCollectionMaterializationNotSupported);
        }

        public static Exception GroupingNotSupportedAsOrderCriterion()
        {
            return new NotSupportedException(Strings.GroupingNotSupportedAsOrderCriterion);
        }

        public static Exception IifReturnTypesMustBeEqual(object p0, object p1)
        {
            return new NotSupportedException(Strings.IifReturnTypesMustBeEqual(p0, p1));
        }

        public static Exception Impossible()
        {
            return new InvalidOperationException(Strings.Impossible);
        }

        public static Exception IndexOfWithStringComparisonArgNotSupported()
        {
            return new NotSupportedException(Strings.IndexOfWithStringComparisonArgNotSupported);
        }

        public static Exception InfiniteDescent()
        {
            return new InvalidOperationException(Strings.InfiniteDescent);
        }

        public static Exception InsertItemMustBeConstant()
        {
            return new NotSupportedException(Strings.InsertItemMustBeConstant);
        }

        public static Exception IntersectNotSupportedForHierarchicalTypes()
        {
            return new NotSupportedException(Strings.IntersectNotSupportedForHierarchicalTypes);
        }

        public static Exception InvalidConnectionArgument(object p0)
        {
            return new ArgumentException(Strings.InvalidConnectionArgument(p0));
        }

        public static Exception InvalidDbGeneratedType(object p0)
        {
            return new NotSupportedException(Strings.InvalidDbGeneratedType(p0));
        }

        public static Exception InvalidFormatNode(object p0)
        {
            return new InvalidOperationException(Strings.InvalidFormatNode(p0));
        }

        public static Exception InvalidGroupByExpression()
        {
            return new NotSupportedException(Strings.InvalidGroupByExpression);
        }

        public static Exception InvalidGroupByExpressionType(object p0)
        {
            return new NotSupportedException(Strings.InvalidGroupByExpressionType(p0));
        }

        public static Exception InvalidMethodExecution(object p0)
        {
            return new InvalidOperationException(Strings.InvalidMethodExecution(p0));
        }

        public static Exception InvalidOrderByExpression(object p0)
        {
            return new NotSupportedException(Strings.InvalidOrderByExpression(p0));
        }

        public static Exception InvalidProviderType(object p0)
        {
            return new NotSupportedException(Strings.InvalidProviderType(p0));
        }

        public static Exception InvalidReferenceToRemovedAliasDuringDeflation()
        {
            return new InvalidOperationException(Strings.InvalidReferenceToRemovedAliasDuringDeflation);
        }

        public static Exception InvalidReturnFromSproc(object p0)
        {
            return new InvalidOperationException(Strings.InvalidReturnFromSproc(p0));
        }

        public static Exception InvalidSequenceOperatorCall(object p0)
        {
            return new InvalidOperationException(Strings.InvalidSequenceOperatorCall(p0));
        }

        public static Exception LastIndexOfWithStringComparisonArgNotSupported()
        {
            return new NotSupportedException(Strings.LastIndexOfWithStringComparisonArgNotSupported);
        }

        public static Exception MappedTypeMustHaveDefaultConstructor(object p0)
        {
            return new InvalidOperationException(Strings.MappedTypeMustHaveDefaultConstructor(p0));
        }

        public static Exception MathRoundNotSupported()
        {
            return new NotSupportedException(Strings.MathRoundNotSupported);
        }

        public static Exception MemberAccessIllegal(object p0, object p1, object p2)
        {
            return new InvalidOperationException(Strings.MemberAccessIllegal(p0, p1, p2));
        }

        public static Exception MemberCannotBeTranslated(object p0, object p1)
        {
            return new NotSupportedException(Strings.MemberCannotBeTranslated(p0, p1));
        }

        public static Exception MemberCouldNotBeTranslated(object p0, object p1)
        {
            return new InvalidOperationException(Strings.MemberCouldNotBeTranslated(p0, p1));
        }

        public static Exception MemberNotPartOfProjection(object p0, object p1)
        {
            return new InvalidOperationException(Strings.MemberNotPartOfProjection(p0, p1));
        }

        public static Exception MethodFormHasNoSupportConversionToSql(object p0, object p1)
        {
            return new NotSupportedException(Strings.MethodFormHasNoSupportConversionToSql(p0, p1));
        }

        public static Exception MethodHasNoSupportConversionToSql(object p0)
        {
            return new NotSupportedException(Strings.MethodHasNoSupportConversionToSql(p0));
        }

        public static Exception MethodNotMappedToStoredProcedure(object p0)
        {
            return new InvalidOperationException(Strings.MethodNotMappedToStoredProcedure(p0));
        }

        public static Exception NoMethodInTypeMatchingArguments(object p0)
        {
            return new InvalidOperationException(Strings.NoMethodInTypeMatchingArguments(p0));
        }

        public static Exception NonConstantExpressionsNotSupportedFor(object p0)
        {
            return new NotSupportedException(Strings.NonConstantExpressionsNotSupportedFor(p0));
        }

        public static Exception NonConstantExpressionsNotSupportedForRounding()
        {
            return new NotSupportedException(Strings.NonConstantExpressionsNotSupportedForRounding);
        }

        public static Exception NonCountAggregateFunctionsAreNotValidOnProjections(object p0)
        {
            return new NotSupportedException(Strings.NonCountAggregateFunctionsAreNotValidOnProjections(p0));
        }

        public static Exception NotImplemented()
        {
            return new NotImplementedException();
        }

        public static Exception NotSupported()
        {
            return new NotSupportedException();
        }

        public static Exception ParameterNotInScope(object p0)
        {
            return new InvalidOperationException(Strings.ParameterNotInScope(p0));
        }

        public static Exception ParametersCannotBeSequences()
        {
            return new NotSupportedException(Strings.ParametersCannotBeSequences);
        }

        public static Exception ProviderCannotBeUsedAfterDispose()
        {
            return new ObjectDisposedException(Strings.ProviderCannotBeUsedAfterDispose);
        }

        public static Exception ProviderNotInstalled(object p0, object p1)
        {
            return new InvalidOperationException(Strings.ProviderNotInstalled(p0, p1));
        }

        public static Exception QueryOnLocalCollectionNotSupported()
        {
            return new NotSupportedException(Strings.QueryOnLocalCollectionNotSupported);
        }

        public static Exception QueryOperatorNotSupported(object p0)
        {
            return new NotSupportedException(Strings.QueryOperatorNotSupported(p0));
        }

        public static Exception QueryOperatorOverloadNotSupported(object p0)
        {
            return new NotSupportedException(Strings.QueryOperatorOverloadNotSupported(p0));
        }

        public static Exception ReaderUsedAfterDispose()
        {
            return new InvalidOperationException(Strings.ReaderUsedAfterDispose);
        }

        public static Exception RequiredColumnDoesNotExist(object p0)
        {
            return new InvalidOperationException(Strings.RequiredColumnDoesNotExist(p0));
        }

        public static Exception ResultTypeNotMappedToFunction(object p0, object p1)
        {
            return new InvalidOperationException(Strings.ResultTypeNotMappedToFunction(p0, p1));
        }

        public static Exception SelectManyDoesNotSupportStrings()
        {
            return new ArgumentException(Strings.SelectManyDoesNotSupportStrings);
        }

        public static Exception SequenceOperatorsNotSupportedForType(object p0)
        {
            return new NotSupportedException(Strings.SequenceOperatorsNotSupportedForType(p0));
        }

        public static Exception SimpleCaseShouldNotHold(object p0)
        {
            return new InvalidOperationException(Strings.SimpleCaseShouldNotHold(p0));
        }

        public static Exception SkipIsValidOnlyOverOrderedQueries()
        {
            return new InvalidOperationException(Strings.SkipIsValidOnlyOverOrderedQueries);
        }

        public static Exception SkipNotSupportedForSequenceTypes()
        {
            return new NotSupportedException(Strings.SkipNotSupportedForSequenceTypes);
        }

        public static Exception SkipRequiresSingleTableQueryWithPKs()
        {
            return new NotSupportedException(Strings.SkipRequiresSingleTableQueryWithPKs);
        }

        public static Exception SprocsCannotBeComposed()
        {
            return new InvalidOperationException(Strings.SprocsCannotBeComposed);
        }

        public static Exception SqlMethodOnlyForSql(object p0)
        {
            return new NotSupportedException(Strings.SqlMethodOnlyForSql(p0));
        }

        public static Exception ToStringOnlySupportedForPrimitiveTypes()
        {
            return new NotSupportedException(Strings.ToStringOnlySupportedForPrimitiveTypes);
        }

        public static Exception TransactionDoesNotMatchConnection()
        {
            return new InvalidOperationException(Strings.TransactionDoesNotMatchConnection);
        }

        public static Exception TypeBinaryOperatorNotRecognized()
        {
            return new InvalidOperationException(Strings.TypeBinaryOperatorNotRecognized);
        }

        public static Exception TypeCannotBeOrdered(object p0)
        {
            return new InvalidOperationException(Strings.TypeCannotBeOrdered(p0));
        }

        public static Exception TypeColumnWithUnhandledSource()
        {
            return new InvalidOperationException(Strings.TypeColumnWithUnhandledSource);
        }

        public static Exception UnableToBindUnmappedMember(object p0, object p1, object p2)
        {
            return new InvalidOperationException(Strings.UnableToBindUnmappedMember(p0, p1, p2));
        }

        public static Exception UnexpectedFloatingColumn()
        {
            return new InvalidOperationException(Strings.UnexpectedFloatingColumn);
        }

        public static Exception UnexpectedNode(object p0)
        {
            return new InvalidOperationException(Strings.UnexpectedNode(p0));
        }

        public static Exception UnexpectedSharedExpression()
        {
            return new InvalidOperationException(Strings.UnexpectedSharedExpression);
        }

        public static Exception UnexpectedSharedExpressionReference()
        {
            return new InvalidOperationException(Strings.UnexpectedSharedExpressionReference);
        }

        public static Exception UnexpectedTypeCode(object p0)
        {
            return new InvalidOperationException(Strings.UnexpectedTypeCode(p0));
        }

        public static Exception UnhandledBindingType(object p0)
        {
            return new InvalidOperationException(Strings.UnhandledBindingType(p0));
        }

        public static Exception UnhandledExpressionType(object p0)
        {
            return new ArgumentException(Strings.UnhandledExpressionType(p0));
        }

        public static Exception UnhandledMemberAccess(object p0, object p1)
        {
            return new InvalidOperationException(Strings.UnhandledMemberAccess(p0, p1));
        }

        public static Exception UnhandledStringTypeComparison()
        {
            return new NotSupportedException(Strings.UnhandledStringTypeComparison);
        }

        public static Exception UnionDifferentMemberOrder()
        {
            return new NotSupportedException(Strings.UnionDifferentMemberOrder);
        }

        public static Exception UnionDifferentMembers()
        {
            return new NotSupportedException(Strings.UnionDifferentMembers);
        }

        public static Exception UnionIncompatibleConstruction()
        {
            return new NotSupportedException(Strings.UnionIncompatibleConstruction);
        }

        public static Exception UnionOfIncompatibleDynamicTypes()
        {
            return new NotSupportedException(Strings.UnionOfIncompatibleDynamicTypes);
        }

        public static Exception UnionWithHierarchy()
        {
            return new NotSupportedException(Strings.UnionWithHierarchy);
        }

        public static Exception UnmappedDataMember(object p0, object p1, object p2)
        {
            return new InvalidOperationException(Strings.UnmappedDataMember(p0, p1, p2));
        }

        public static Exception UnrecognizedExpressionNode(object p0)
        {
            return new InvalidOperationException(Strings.UnrecognizedExpressionNode(p0));
        }

        public static Exception UnrecognizedProviderMode(object p0)
        {
            return new InvalidOperationException(Strings.UnrecognizedProviderMode(p0));
        }

        public static Exception UnsafeStringConversion(object p0, object p1)
        {
            return new FormatException(Strings.UnsafeStringConversion(p0, p1));
        }

        public static Exception UnsupportedDateTimeConstructorForm()
        {
            return new NotSupportedException(Strings.UnsupportedDateTimeConstructorForm);
        }

        public static Exception UnsupportedNodeType(object p0)
        {
            return new NotSupportedException(Strings.UnsupportedNodeType(p0));
        }

        public static Exception UnsupportedStringConstructorForm()
        {
            return new NotSupportedException(Strings.UnsupportedStringConstructorForm);
        }

        public static Exception UnsupportedTimeSpanConstructorForm()
        {
            return new NotSupportedException(Strings.UnsupportedTimeSpanConstructorForm);
        }

        public static Exception UnsupportedTypeConstructorForm(object p0)
        {
            return new NotSupportedException(Strings.UnsupportedTypeConstructorForm(p0));
        }

        public static Exception UpdateItemMustBeConstant()
        {
            return new NotSupportedException(Strings.UpdateItemMustBeConstant);
        }

        public static Exception ValueHasNoLiteralInSql(object p0)
        {
            return new InvalidOperationException(Strings.ValueHasNoLiteralInSql(p0));
        }

        public static Exception VbLikeDoesNotSupportMultipleCharacterRanges()
        {
            return new ArgumentException(Strings.VbLikeDoesNotSupportMultipleCharacterRanges);
        }

        public static Exception VbLikeUnclosedBracket()
        {
            return new ArgumentException(Strings.VbLikeUnclosedBracket);
        }

        public static Exception WrongDataContext()
        {
            return new InvalidOperationException(Strings.WrongDataContext);
        }

        public static Exception WrongNumberOfValuesInCollectionArgument(object p0, object p1, object p2)
        {
            return new ArgumentException(Strings.WrongNumberOfValuesInCollectionArgument(p0, p1, p2));
        }


        //public static Exception TablesLimited()
        //{
        //    return new Exception(Constants.FreeEditionLimited);
        //}

        //public static Exception TablesLimited(LicenseType licenseType, int limitedNumber)
        //{
        //    var msg = string.Format("{0} edition is limited to {1} tables in a database.", licenseType, limitedNumber);
        //    return new Exception(msg);
        //}
    }
}