using System;
using System.Reflection;

namespace ALinq
{
    internal static class Strings
    {
        // Methods

        // Properties
        internal static string CannotAddChangeConflicts
        {
            get { return SR.GetString("CannotAddChangeConflicts"); }
        }

        internal static string CannotAttachAddNonNewEntities
        {
            get { return SR.GetString("CannotAttachAddNonNewEntities"); }
        }

        internal static string CannotAttachAlreadyExistingEntity
        {
            get { return SR.GetString("CannotAttachAlreadyExistingEntity"); }
        }

        internal static string CannotAttachAsModifiedWithoutOriginalState
        {
            get { return SR.GetString("CannotAttachAsModifiedWithoutOriginalState"); }
        }

        internal static string CannotPerformOperationDuringSubmitChanges
        {
            get { return SR.GetString("CannotPerformOperationDuringSubmitChanges"); }
        }

        internal static string CannotPerformOperationForUntrackedObject
        {
            get { return SR.GetString("CannotPerformOperationForUntrackedObject"); }
        }

        internal static string CannotPerformOperationOutsideSubmitChanges
        {
            get { return SR.GetString("CannotPerformOperationOutsideSubmitChanges"); }
        }

        internal static string CannotRemoveChangeConflicts
        {
            get { return SR.GetString("CannotRemoveChangeConflicts"); }
        }

        internal static string CannotRemoveUnattachedEntity
        {
            get { return SR.GetString("CannotRemoveUnattachedEntity"); }
        }

        internal static string CantAddAlreadyExistingItem
        {
            get { return SR.GetString("CantAddAlreadyExistingItem"); }
        }

        internal static string CantAddAlreadyExistingKey
        {
            get { return SR.GetString("CantAddAlreadyExistingKey"); }
        }

        internal static string CouldNotAttach
        {
            get { return SR.GetString("CouldNotAttach"); }
        }

        internal static string CycleDetected
        {
            get { return SR.GetString("CycleDetected"); }
        }

        internal static string DatabaseGeneratedAlreadyExistingKey
        {
            get { return SR.GetString("DatabaseGeneratedAlreadyExistingKey"); }
        }

        internal static string DataContextCannotBeUsedAfterDispose
        {
            get { return SR.GetString("DataContextCannotBeUsedAfterDispose"); }
        }

        internal static string DeferredLoadingRequiresObjectTracking
        {
            get { return SR.GetString("DeferredLoadingRequiresObjectTracking"); }
        }

        internal static string DeleteCallbackComment
        {
            get { return SR.GetString("DeleteCallbackComment"); }
        }

        internal static string EntityIsTheWrongType
        {
            get { return SR.GetString("EntityIsTheWrongType"); }
        }

        internal static string EntitySetAlreadyLoaded
        {
            get { return SR.GetString("EntitySetAlreadyLoaded"); }
        }

        internal static string EntitySetModifiedDuringEnumeration
        {
            get { return SR.GetString("EntitySetModifiedDuringEnumeration"); }
        }

        internal static string ExpectedUpdateDeleteOrChange
        {
            get { return SR.GetString("ExpectedUpdateDeleteOrChange"); }
        }

        internal static string IncludeCycleNotAllowed
        {
            get { return SR.GetString("IncludeCycleNotAllowed"); }
        }

        internal static string IncludeNotAllowedAfterFreeze
        {
            get { return SR.GetString("IncludeNotAllowedAfterFreeze"); }
        }

        internal static string InsertAutoSyncFailure
        {
            get { return SR.GetString("InsertAutoSyncFailure"); }
        }

        internal static string InsertCallbackComment
        {
            get { return SR.GetString("InsertCallbackComment"); }
        }

        internal static string InvalidLoadOptionsLoadMemberSpecification
        {
            get { return SR.GetString("InvalidLoadOptionsLoadMemberSpecification"); }
        }

        internal static string LoadOptionsChangeNotAllowedAfterQuery
        {
            get { return SR.GetString("LoadOptionsChangeNotAllowedAfterQuery"); }
        }

        internal static string ModifyDuringAddOrRemove
        {
            get { return SR.GetString("ModifyDuringAddOrRemove"); }
        }

        internal static string ObjectTrackingRequired
        {
            get { return SR.GetString("ObjectTrackingRequired"); }
        }

        internal static string OptionsCannotBeModifiedAfterQuery
        {
            get { return SR.GetString("OptionsCannotBeModifiedAfterQuery"); }
        }

        internal static string OriginalEntityIsWrongType
        {
            get { return SR.GetString("OriginalEntityIsWrongType"); }
        }

        internal static string OwningTeam
        {
            get { return SR.GetString("OwningTeam"); }
        }

        internal static string ProviderCannotBeUsedAfterDispose
        {
            get { return SR.GetString("ProviderCannotBeUsedAfterDispose"); }
        }

        internal static string ProviderTypeNull
        {
            get { return SR.GetString("ProviderTypeNull"); }
        }

        internal static string RefreshOfDeletedObject
        {
            get { return SR.GetString("RefreshOfDeletedObject"); }
        }

        internal static string RowNotFoundOrChanged
        {
            get { return SR.GetString("RowNotFoundOrChanged"); }
        }

        internal static string SubqueryMustBeSequence
        {
            get { return SR.GetString("SubqueryMustBeSequence"); }
        }

        internal static string SubqueryNotAllowedAfterFreeze
        {
            get { return SR.GetString("SubqueryNotAllowedAfterFreeze"); }
        }

        internal static string UnableToDetermineDataContext
        {
            get { return SR.GetString("UnableToDetermineDataContext"); }
        }

        internal static string UnrecognizedRefreshObject
        {
            get { return SR.GetString("UnrecognizedRefreshObject"); }
        }

        internal static string UpdateCallbackComment
        {
            get { return SR.GetString("UpdateCallbackComment"); }
        }


        internal static string ArgumentTypeHasNoIdentityKey(object p0)
        {
            return SR.GetString("ArgumentTypeHasNoIdentityKey", new[] {p0});
        }

        internal static string CannotChangeInheritanceType(object p0, object p1, object p2, object p3)
        {
            return SR.GetString("CannotChangeInheritanceType", new[] {p0, p1, p2, p3});
        }

        internal static string CannotPerformCUDOnReadOnlyTable(object p0)
        {
            return SR.GetString("CannotPerformCUDOnReadOnlyTable", new[] {p0});
        }

        internal static string ColumnMappedMoreThanOnce(object p0)
        {
            return SR.GetString("ColumnMappedMoreThanOnce", new[] {p0});
        }

        internal static string CouldNotConvert(object p0, object p1)
        {
            return SR.GetString("CouldNotConvert", new[] {p0, p1});
        }

        internal static string CouldNotGetTableForSubtype(object p0, object p1)
        {
            return SR.GetString("CouldNotGetTableForSubtype", new[] {p0, p1});
        }

        internal static string CouldNotRemoveRelationshipBecauseOneSideCannotBeNull(object p0, object p1, object p2)
        {
            return SR.GetString("CouldNotRemoveRelationshipBecauseOneSideCannotBeNull", new[] {p0, p1, p2});
        }

        internal static string DbGeneratedChangeNotAllowed(object p0, object p1)
        {
            return SR.GetString("DbGeneratedChangeNotAllowed", new[] {p0, p1});
        }

        internal static string EntitySetDataBindingWithAbstractBaseClass(object p0)
        {
            return SR.GetString("EntitySetDataBindingWithAbstractBaseClass", new[] {p0});
        }

        internal static string EntitySetDataBindingWithNonPublicDefaultConstructor(object p0)
        {
            return SR.GetString("EntitySetDataBindingWithNonPublicDefaultConstructor", new[] {p0});
        }

        internal static string ExpectedQueryableArgument(object p0, object p1)
        {
            return SR.GetString("ExpectedQueryableArgument", new[] {p0, p1});
        }

        internal static string IdentityChangeNotAllowed(object p0, object p1)
        {
            return SR.GetString("IdentityChangeNotAllowed", new[] {p0, p1});
        }

        internal static string InconsistentAssociationAndKeyChange(object p0, object p1)
        {
            return SR.GetString("InconsistentAssociationAndKeyChange", new[] {p0, p1});
        }

        internal static string KeyIsWrongSize(object p0, object p1)
        {
            return SR.GetString("KeyIsWrongSize", new[] {p0, p1});
        }

        internal static string KeyValueIsWrongType(object p0, object p1)
        {
            return SR.GetString("KeyValueIsWrongType", new[] {p0, p1});
        }

        internal static string NonEntityAssociationMapping(object p0, object p1, object p2)
        {
            return SR.GetString("NonEntityAssociationMapping", new[] {p0, p1, p2});
        }

        internal static string ProviderDoesNotImplementRequiredInterface(object p0, object p1)
        {
            return SR.GetString("ProviderDoesNotImplementRequiredInterface", new[] {p0, p1});
        }

        internal static string SubqueryDoesNotSupportOperator(object p0)
        {
            return SR.GetString("SubqueryDoesNotSupportOperator", new[] {p0});
        }

        internal static string SubqueryNotSupportedOn(object p0)
        {
            return SR.GetString("SubqueryNotSupportedOn", new[] {p0});
        }

        internal static string SubqueryNotSupportedOnType(object p0, object p1)
        {
            return SR.GetString("SubqueryNotSupportedOnType", new[] {p0, p1});
        }

        internal static string TypeCouldNotBeAdded(object p0)
        {
            return SR.GetString("TypeCouldNotBeAdded", new[] {p0});
        }

        internal static string TypeCouldNotBeRemoved(object p0)
        {
            return SR.GetString("TypeCouldNotBeRemoved", new[] {p0});
        }

        internal static string TypeCouldNotBeTracked(object p0)
        {
            return SR.GetString("TypeCouldNotBeTracked", new[] {p0});
        }

        internal static string TypeIsNotEntity(object p0)
        {
            return SR.GetString("TypeIsNotEntity", new[] {p0});
        }

        internal static string TypeIsNotMarkedAsTable(object p0)
        {
            return SR.GetString("TypeIsNotMarkedAsTable", new[] {p0});
        }

        internal static string UnhandledBindingType(object p0)
        {
            return SR.GetString("UnhandledBindingType", new[] {p0});
        }

        internal static string UnhandledExpressionType(object p0)
        {
            return SR.GetString("UnhandledExpressionType", new[] {p0});
        }

        internal static string UpdatesFailedMessage(object p0, object p1)
        {
            return SR.GetString("UpdatesFailedMessage", new[] {p0, p1});
        }

        public static string CannotContainsMethod(Type baseInterface, MethodInfo method)
        {
            return SR.GetString("CannotContainsMethod", baseInterface.Name, method.Name);
        }

        public static string CannotContainsProperty(Type baseInterface, PropertyInfo property)
        {
            return SR.GetString("CannotContainsProperty", baseInterface.Name, property.Name);
        }
    }
}