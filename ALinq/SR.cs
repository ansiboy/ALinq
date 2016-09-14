using System.Globalization;
using System.Resources;
using System.Threading;

namespace ALinq
{
    internal sealed class SR
    {
        // Fields
        internal const string ArgumentTypeHasNoIdentityKey = "ArgumentTypeHasNoIdentityKey";
        internal const string CannotAddChangeConflicts = "CannotAddChangeConflicts";
        internal const string CannotAttachAddNonNewEntities = "CannotAttachAddNonNewEntities";
        internal const string CannotAttachAlreadyExistingEntity = "CannotAttachAlreadyExistingEntity";
        internal const string CannotAttachAsModifiedWithoutOriginalState = "CannotAttachAsModifiedWithoutOriginalState";
        internal const string CannotChangeInheritanceType = "CannotChangeInheritanceType";
        internal const string CannotPerformCUDOnReadOnlyTable = "CannotPerformCUDOnReadOnlyTable";
        internal const string CannotPerformOperationDuringSubmitChanges = "CannotPerformOperationDuringSubmitChanges";
        internal const string CannotPerformOperationForUntrackedObject = "CannotPerformOperationForUntrackedObject";
        internal const string CannotPerformOperationOutsideSubmitChanges = "CannotPerformOperationOutsideSubmitChanges";
        internal const string CannotRemoveChangeConflicts = "CannotRemoveChangeConflicts";
        internal const string CannotRemoveUnattachedEntity = "CannotRemoveUnattachedEntity";
        internal const string CantAddAlreadyExistingItem = "CantAddAlreadyExistingItem";
        internal const string CantAddAlreadyExistingKey = "CantAddAlreadyExistingKey";
        internal const string ColumnMappedMoreThanOnce = "ColumnMappedMoreThanOnce";
        internal const string CouldNotAttach = "CouldNotAttach";
        internal const string CouldNotConvert = "CouldNotConvert";
        internal const string CouldNotGetTableForSubtype = "CouldNotGetTableForSubtype";

        internal const string CouldNotRemoveRelationshipBecauseOneSideCannotBeNull =
            "CouldNotRemoveRelationshipBecauseOneSideCannotBeNull";

        internal const string CycleDetected = "CycleDetected";
        internal const string DatabaseGeneratedAlreadyExistingKey = "DatabaseGeneratedAlreadyExistingKey";
        internal const string DataContextCannotBeUsedAfterDispose = "DataContextCannotBeUsedAfterDispose";
        internal const string DbGeneratedChangeNotAllowed = "DbGeneratedChangeNotAllowed";
        internal const string DeferredLoadingRequiresObjectTracking = "DeferredLoadingRequiresObjectTracking";
        internal const string DeleteCallbackComment = "DeleteCallbackComment";
        internal const string EntityIsTheWrongType = "EntityIsTheWrongType";
        internal const string EntitySetAlreadyLoaded = "EntitySetAlreadyLoaded";
        internal const string EntitySetDataBindingWithAbstractBaseClass = "EntitySetDataBindingWithAbstractBaseClass";

        internal const string EntitySetDataBindingWithNonPublicDefaultConstructor =
            "EntitySetDataBindingWithNonPublicDefaultConstructor";

        internal const string EntitySetModifiedDuringEnumeration = "EntitySetModifiedDuringEnumeration";
        internal const string ExpectedQueryableArgument = "ExpectedQueryableArgument";
        internal const string ExpectedUpdateDeleteOrChange = "ExpectedUpdateDeleteOrChange";
        internal const string IdentityChangeNotAllowed = "IdentityChangeNotAllowed";
        internal const string IncludeCycleNotAllowed = "IncludeCycleNotAllowed";
        internal const string IncludeNotAllowedAfterFreeze = "IncludeNotAllowedAfterFreeze";
        internal const string InconsistentAssociationAndKeyChange = "InconsistentAssociationAndKeyChange";
        internal const string InsertAutoSyncFailure = "InsertAutoSyncFailure";
        internal const string InsertCallbackComment = "InsertCallbackComment";
        internal const string InvalidLoadOptionsLoadMemberSpecification = "InvalidLoadOptionsLoadMemberSpecification";
        internal const string KeyIsWrongSize = "KeyIsWrongSize";
        internal const string KeyValueIsWrongType = "KeyValueIsWrongType";
        internal const string LoadOptionsChangeNotAllowedAfterQuery = "LoadOptionsChangeNotAllowedAfterQuery";
        internal const string ModifyDuringAddOrRemove = "ModifyDuringAddOrRemove";
        internal const string NonEntityAssociationMapping = "NonEntityAssociationMapping";
        internal const string ObjectTrackingRequired = "ObjectTrackingRequired";
        internal const string OptionsCannotBeModifiedAfterQuery = "OptionsCannotBeModifiedAfterQuery";
        internal const string OriginalEntityIsWrongType = "OriginalEntityIsWrongType";
        internal const string OwningTeam = "OwningTeam";
        internal const string ProviderDoesNotImplementRequiredInterface = "ProviderDoesNotImplementRequiredInterface";
        internal const string ProviderTypeNull = "ProviderTypeNull";
        internal const string RefreshOfDeletedObject = "RefreshOfDeletedObject";
        internal const string RowNotFoundOrChanged = "RowNotFoundOrChanged";
        internal const string SubqueryDoesNotSupportOperator = "SubqueryDoesNotSupportOperator";
        internal const string SubqueryMustBeSequence = "SubqueryMustBeSequence";
        internal const string SubqueryNotAllowedAfterFreeze = "SubqueryNotAllowedAfterFreeze";
        internal const string SubqueryNotSupportedOn = "SubqueryNotSupportedOn";
        internal const string SubqueryNotSupportedOnType = "SubqueryNotSupportedOnType";
        internal const string TypeCouldNotBeAdded = "TypeCouldNotBeAdded";
        internal const string TypeCouldNotBeRemoved = "TypeCouldNotBeRemoved";
        internal const string TypeCouldNotBeTracked = "TypeCouldNotBeTracked";
        internal const string TypeIsNotEntity = "TypeIsNotEntity";
        internal const string TypeIsNotMarkedAsTable = "TypeIsNotMarkedAsTable";
        internal const string UnableToDetermineDataContext = "UnableToDetermineDataContext";
        internal const string UnhandledBindingType = "UnhandledBindingType";
        internal const string UnhandledExpressionType = "UnhandledExpressionType";
        internal const string UnrecognizedRefreshObject = "UnrecognizedRefreshObject";
        internal const string UpdateCallbackComment = "UpdateCallbackComment";
        internal const string UpdatesFailedMessage = "UpdatesFailedMessage";
        private static SR loader;
        private readonly ResourceManager resources;

        // Methods
        internal SR()
        {
            resources = new ResourceManager("ALinq.Resources.ALinq", GetType().Assembly);
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
            //SR loader = GetLoader();
            //if (loader == null)
            //{
            //    return null;
            //}
            //return loader.resources.GetString(name, Culture);
            return Resources.GetString(name);
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