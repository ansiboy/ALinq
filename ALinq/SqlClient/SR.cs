using System.Globalization;
using System.Resources;
using System.Threading;

namespace ALinq.SqlClient
{
    internal sealed class SR
    {
        // Fields
        internal const string ArgumentEmpty = "ArgumentEmpty";
        internal const string ArgumentTypeMismatch = "ArgumentTypeMismatch";
        internal const string ArgumentWrongType = "ArgumentWrongType";
        internal const string ArgumentWrongValue = "ArgumentWrongValue";
        internal const string BadParameterType = "BadParameterType";
        internal const string BadProjectionInSelect = "BadProjectionInSelect";
        internal const string BinaryOperatorNotRecognized = "BinaryOperatorNotRecognized";
        internal const string CannotAggregateType = "CannotAggregateType";
        internal const string CannotAssignNull = "CannotAssignNull";
        internal const string CannotAssignToMember = "CannotAssignToMember";

        internal const string CannotCompareItemsAssociatedWithDifferentTable =
            "CannotCompareItemsAssociatedWithDifferentTable";

        internal const string CannotConvertToEntityRef = "CannotConvertToEntityRef";
        internal const string CannotDeleteTypesOf = "CannotDeleteTypesOf";
        internal const string CannotEnumerateResultsMoreThanOnce = "CannotEnumerateResultsMoreThanOnce";
        internal const string CannotMaterializeEntityType = "CannotMaterializeEntityType";
        internal const string CannotTranslateExpressionToSql = "CannotTranslateExpressionToSql";
        internal const string CapturedValuesCannotBeSequences = "CapturedValuesCannotBeSequences";
        internal const string ClassLiteralsNotAllowed = "ClassLiteralsNotAllowed";
        internal const string ClientCaseShouldNotHold = "ClientCaseShouldNotHold";
        internal const string ClrBoolDoesNotAgreeWithSqlType = "ClrBoolDoesNotAgreeWithSqlType";
        internal const string ColumnCannotReferToItself = "ColumnCannotReferToItself";

        internal const string ColumnClrTypeDoesNotAgreeWithExpressionsClrType =
            "ColumnClrTypeDoesNotAgreeWithExpressionsClrType";

        internal const string ColumnIsDefinedInMultiplePlaces = "ColumnIsDefinedInMultiplePlaces";
        internal const string ColumnIsNotAccessibleThroughDistinct = "ColumnIsNotAccessibleThroughDistinct";
        internal const string ColumnIsNotAccessibleThroughGroupBy = "ColumnIsNotAccessibleThroughGroupBy";
        internal const string ColumnReferencedIsNotInScope = "ColumnReferencedIsNotInScope";
        internal const string ComparisonNotSupportedForType = "ComparisonNotSupportedForType";

        internal const string CompiledQueryAgainstMultipleShapesNotSupported =
            "CompiledQueryAgainstMultipleShapesNotSupported";

        internal const string CompiledQueryCannotReturnType = "CompiledQueryCannotReturnType";
        internal const string ConstructedArraysNotSupported = "ConstructedArraysNotSupported";
        internal const string ContextNotInitialized = "ContextNotInitialized";
        internal const string ConvertToCharFromBoolNotSupported = "ConvertToCharFromBoolNotSupported";
        internal const string ConvertToDateTimeOnlyForDateTimeOrString = "ConvertToDateTimeOnlyForDateTimeOrString";
        internal const string CouldNotAssignSequence = "CouldNotAssignSequence";
        internal const string CouldNotConvertToPropertyOrField = "CouldNotConvertToPropertyOrField";
        internal const string CouldNotDetermineCatalogName = "CouldNotDetermineCatalogName";
        internal const string CouldNotDetermineDbGeneratedSqlType = "CouldNotDetermineDbGeneratedSqlType";
        internal const string CouldNotDetermineSqlType = "CouldNotDetermineSqlType";
        internal const string CouldNotGetClrType = "CouldNotGetClrType";
        internal const string CouldNotGetSqlType = "CouldNotGetSqlType";
        internal const string CouldNotHandleAliasRef = "CouldNotHandleAliasRef";
        internal const string CouldNotTranslateExpressionForReading = "CouldNotTranslateExpressionForReading";

        internal const string CreateDatabaseFailedBecauseOfClassWithNoMembers =
            "CreateDatabaseFailedBecauseOfClassWithNoMembers";

        internal const string CreateDatabaseFailedBecauseOfContextWithNoTables =
            "CreateDatabaseFailedBecauseOfContextWithNoTables";

        internal const string CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists =
            "CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists";

        internal const string DatabaseDeleteThroughContext = "DatabaseDeleteThroughContext";
        internal const string DeferredMemberWrongType = "DeferredMemberWrongType";
        internal const string DidNotExpectAs = "DidNotExpectAs";
        internal const string DidNotExpectTypeBinding = "DidNotExpectTypeBinding";
        internal const string DidNotExpectTypeChange = "DidNotExpectTypeChange";
        internal const string DistributedTransactionsAreNotAllowed = "DistributedTransactionsAreNotAllowed";
        internal const string EmptyCaseNotSupported = "EmptyCaseNotSupported";
        internal const string ExceptNotSupportedForHierarchicalTypes = "ExceptNotSupportedForHierarchicalTypes";
        internal const string ExpectedBitFoundPredicate = "ExpectedBitFoundPredicate";
        internal const string ExpectedClrTypesToAgree = "ExpectedClrTypesToAgree";
        internal const string ExpectedNoObjectType = "ExpectedNoObjectType";
        internal const string ExpectedPredicateFoundBit = "ExpectedPredicateFoundBit";
        internal const string ExpectedQueryableArgument = "ExpectedQueryableArgument";
        internal const string ExpressionNotDeferredQuerySource = "ExpressionNotDeferredQuerySource";

        internal const string GeneralCollectionMaterializationNotSupported =
            "GeneralCollectionMaterializationNotSupported";

        internal const string GroupingNotSupportedAsOrderCriterion = "GroupingNotSupportedAsOrderCriterion";
        internal const string IifReturnTypesMustBeEqual = "IifReturnTypesMustBeEqual";
        internal const string Impossible = "Impossible";
        internal const string IndexOfWithStringComparisonArgNotSupported = "IndexOfWithStringComparisonArgNotSupported";
        internal const string InfiniteDescent = "InfiniteDescent";
        internal const string InsertItemMustBeConstant = "InsertItemMustBeConstant";
        internal const string IntersectNotSupportedForHierarchicalTypes = "IntersectNotSupportedForHierarchicalTypes";
        internal const string InvalidConnectionArgument = "InvalidConnectionArgument";
        internal const string InvalidDbGeneratedType = "InvalidDbGeneratedType";
        internal const string InvalidFormatNode = "InvalidFormatNode";
        internal const string InvalidGroupByExpression = "InvalidGroupByExpression";
        internal const string InvalidGroupByExpressionType = "InvalidGroupByExpressionType";
        internal const string InvalidMethodExecution = "InvalidMethodExecution";
        internal const string InvalidOrderByExpression = "InvalidOrderByExpression";
        internal const string InvalidProviderType = "InvalidProviderType";

        internal const string InvalidReferenceToRemovedAliasDuringDeflation =
            "InvalidReferenceToRemovedAliasDuringDeflation";

        internal const string InvalidReturnFromSproc = "InvalidReturnFromSproc";
        internal const string InvalidSequenceOperatorCall = "InvalidSequenceOperatorCall";

        internal const string LastIndexOfWithStringComparisonArgNotSupported =
            "LastIndexOfWithStringComparisonArgNotSupported";

        internal const string LenOfTextOrNTextNotSupported = "LenOfTextOrNTextNotSupported";
        internal const string LogAttemptingToDeleteDatabase = "LogAttemptingToDeleteDatabase";
        internal const string LogGeneralInfoMessage = "LogGeneralInfoMessage";
        internal const string LogStoredProcedureExecution = "LogStoredProcedureExecution";
        internal const string MappedTypeMustHaveDefaultConstructor = "MappedTypeMustHaveDefaultConstructor";
        internal const string MathRoundNotSupported = "MathRoundNotSupported";
        internal const string MaxSizeNotSupported = "MaxSizeNotSupported";
        internal const string MemberAccessIllegal = "MemberAccessIllegal";
        internal const string MemberCannotBeTranslated = "MemberCannotBeTranslated";
        internal const string MemberCouldNotBeTranslated = "MemberCouldNotBeTranslated";
        internal const string MemberNotPartOfProjection = "MemberNotPartOfProjection";
        internal const string MethodFormHasNoSupportConversionToSql = "MethodFormHasNoSupportConversionToSql";
        internal const string MethodHasNoSupportConversionToSql = "MethodHasNoSupportConversionToSql";
        internal const string MethodNotMappedToStoredProcedure = "MethodNotMappedToStoredProcedure";
        internal const string NoMethodInTypeMatchingArguments = "NoMethodInTypeMatchingArguments";
        internal const string NonConstantExpressionsNotSupportedFor = "NonConstantExpressionsNotSupportedFor";

        internal const string NonConstantExpressionsNotSupportedForRounding =
            "NonConstantExpressionsNotSupportedForRounding";

        internal const string NonCountAggregateFunctionsAreNotValidOnProjections =
            "NonCountAggregateFunctionsAreNotValidOnProjections";

        internal const string OwningTeam = "OwningTeam";
        internal const string ParameterNotInScope = "ParameterNotInScope";
        internal const string ParametersCannotBeSequences = "ParametersCannotBeSequences";
        internal const string ProviderCannotBeUsedAfterDispose = "ProviderCannotBeUsedAfterDispose";
        internal const string ProviderNotInstalled = "ProviderNotInstalled";
        internal const string QueryOnLocalCollectionNotSupported = "QueryOnLocalCollectionNotSupported";
        internal const string QueryOperatorNotSupported = "QueryOperatorNotSupported";
        internal const string QueryOperatorOverloadNotSupported = "QueryOperatorOverloadNotSupported";
        internal const string ReaderUsedAfterDispose = "ReaderUsedAfterDispose";
        internal const string RequiredColumnDoesNotExist = "RequiredColumnDoesNotExist";
        internal const string ResultTypeNotMappedToFunction = "ResultTypeNotMappedToFunction";
        internal const string SelectManyDoesNotSupportStrings = "SelectManyDoesNotSupportStrings";
        internal const string SequenceOperatorsNotSupportedForType = "SequenceOperatorsNotSupportedForType";
        internal const string SimpleCaseShouldNotHold = "SimpleCaseShouldNotHold";
        internal const string SkipIsValidOnlyOverOrderedQueries = "SkipIsValidOnlyOverOrderedQueries";
        internal const string SkipNotSupportedForSequenceTypes = "SkipNotSupportedForSequenceTypes";
        internal const string SkipRequiresSingleTableQueryWithPKs = "SkipRequiresSingleTableQueryWithPKs";
        internal const string SourceExpressionAnnotation = "SourceExpressionAnnotation";
        internal const string SprocsCannotBeComposed = "SprocsCannotBeComposed";
        internal const string SqlMethodOnlyForSql = "SqlMethodOnlyForSql";
        internal const string TextNTextAndImageCannotOccurInDistinct = "TextNTextAndImageCannotOccurInDistinct";
        internal const string TextNTextAndImageCannotOccurInUnion = "TextNTextAndImageCannotOccurInUnion";
        internal const string ToStringOnlySupportedForPrimitiveTypes = "ToStringOnlySupportedForPrimitiveTypes";
        internal const string TransactionDoesNotMatchConnection = "TransactionDoesNotMatchConnection";
        internal const string TypeBinaryOperatorNotRecognized = "TypeBinaryOperatorNotRecognized";
        internal const string TypeCannotBeOrdered = "TypeCannotBeOrdered";
        internal const string TypeColumnWithUnhandledSource = "TypeColumnWithUnhandledSource";
        internal const string UnableToBindUnmappedMember = "UnableToBindUnmappedMember";
        internal const string UnexpectedFloatingColumn = "UnexpectedFloatingColumn";
        internal const string UnexpectedNode = "UnexpectedNode";
        internal const string UnexpectedSharedExpression = "UnexpectedSharedExpression";
        internal const string UnexpectedSharedExpressionReference = "UnexpectedSharedExpressionReference";
        internal const string UnexpectedTypeCode = "UnexpectedTypeCode";
        internal const string UnhandledBindingType = "UnhandledBindingType";
        internal const string UnhandledExpressionType = "UnhandledExpressionType";
        internal const string UnhandledMemberAccess = "UnhandledMemberAccess";
        internal const string UnhandledStringTypeComparison = "UnhandledStringTypeComparison";
        internal const string UnionDifferentMemberOrder = "UnionDifferentMemberOrder";
        internal const string UnionDifferentMembers = "UnionDifferentMembers";
        internal const string UnionIncompatibleConstruction = "UnionIncompatibleConstruction";
        internal const string UnionOfIncompatibleDynamicTypes = "UnionOfIncompatibleDynamicTypes";
        internal const string UnionWithHierarchy = "UnionWithHierarchy";
        internal const string UnmappedDataMember = "UnmappedDataMember";
        internal const string UnrecognizedExpressionNode = "UnrecognizedExpressionNode";
        internal const string UnrecognizedProviderMode = "UnrecognizedProviderMode";
        internal const string UnsafeStringConversion = "UnsafeStringConversion";
        internal const string UnsupportedDateTimeConstructorForm = "UnsupportedDateTimeConstructorForm";
        internal const string UnsupportedNodeType = "UnsupportedNodeType";
        internal const string UnsupportedStringConstructorForm = "UnsupportedStringConstructorForm";
        internal const string UnsupportedTimeSpanConstructorForm = "UnsupportedTimeSpanConstructorForm";
        internal const string UnsupportedTypeConstructorForm = "UnsupportedTypeConstructorForm";
        internal const string UpdateItemMustBeConstant = "UpdateItemMustBeConstant";
        internal const string ValueHasNoLiteralInSql = "ValueHasNoLiteralInSql";

        internal const string VbLikeDoesNotSupportMultipleCharacterRanges =
            "VbLikeDoesNotSupportMultipleCharacterRanges";

        internal const string VbLikeUnclosedBracket = "VbLikeUnclosedBracket";
        internal const string WrongDataContext = "WrongDataContext";
        internal const string WrongNumberOfValuesInCollectionArgument = "WrongNumberOfValuesInCollectionArgument";
        private static SR loader;
        private readonly ResourceManager resources;

        // Methods
        internal SR()
        {
            resources = new ResourceManager("ALinq.Resources.ALinq.SqlClient", GetType().Assembly);
        }

        private static CultureInfo Culture
        {
            get { return null; }
        }

        public static ResourceManager Resources
        {
            get { return GetLoader().resources; }
        }

        private static SR GetLoader()
        {
            if (loader == null)
            {
                SR sr = new SR();
                Interlocked.CompareExchange(ref loader, sr, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            string format = loader.resources.GetString(name, Culture);
            if ((args == null) || (args.Length <= 0))
            {
                return format;
            }
            for (int i = 0; i < args.Length; i++)
            {
                string str2 = args[i] as string;
                if ((str2 != null) && (str2.Length > 0x400))
                {
                    args[i] = str2.Substring(0, 0x3fd) + "...";
                }
            }
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        // Properties
    }
}