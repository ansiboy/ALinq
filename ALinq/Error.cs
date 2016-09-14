using System;
using System.ComponentModel;
using ALinq.Resources;

namespace ALinq
{
    internal static class Error
    {
        // Methods
        public static Exception ArgumentNull(string paramName)
        {
            return new ArgumentNullException(paramName);
        }

        public static Exception ArgumentOutOfRange(string paramName)
        {
            return new ArgumentOutOfRangeException(paramName);
        }

        public static Exception ArgumentTypeHasNoIdentityKey(object p0)
        {
            return new ArgumentException(Strings.ArgumentTypeHasNoIdentityKey(p0));
        }

        public static Exception CannotAddChangeConflicts()
        {
            return new NotSupportedException(Strings.CannotAddChangeConflicts);
        }

        public static Exception CannotAttachAddNonNewEntities()
        {
            return new NotSupportedException(Strings.CannotAttachAddNonNewEntities);
        }

        public static Exception CannotAttachAlreadyExistingEntity()
        {
            return new InvalidOperationException(Strings.CannotAttachAlreadyExistingEntity);
        }

        public static Exception CannotAttachAsModifiedWithoutOriginalState()
        {
            return new InvalidOperationException(Strings.CannotAttachAsModifiedWithoutOriginalState);
        }

        public static Exception CannotChangeInheritanceType(object p0, object p1, object p2, object p3)
        {
            return new InvalidOperationException(Strings.CannotChangeInheritanceType(p0, p1, p2, p3));
        }

        public static Exception CannotPerformCUDOnReadOnlyTable(object p0)
        {
            return new InvalidOperationException(Strings.CannotPerformCUDOnReadOnlyTable(p0));
        }

        public static Exception CannotPerformOperationDuringSubmitChanges()
        {
            return new InvalidOperationException(Strings.CannotPerformOperationDuringSubmitChanges);
        }

        public static Exception CannotPerformOperationForUntrackedObject()
        {
            return new InvalidOperationException(Strings.CannotPerformOperationForUntrackedObject);
        }

        public static Exception CannotPerformOperationOutsideSubmitChanges()
        {
            return new InvalidOperationException(Strings.CannotPerformOperationOutsideSubmitChanges);
        }

        public static Exception CannotRemoveChangeConflicts()
        {
            return new NotSupportedException(Strings.CannotRemoveChangeConflicts);
        }

        public static Exception CannotRemoveUnattachedEntity()
        {
            return new InvalidOperationException(Strings.CannotRemoveUnattachedEntity);
        }

        public static Exception CantAddAlreadyExistingItem()
        {
            return new InvalidOperationException(Strings.CantAddAlreadyExistingItem);
        }

        public static Exception ColumnMappedMoreThanOnce(object p0)
        {
            return new InvalidOperationException(Strings.ColumnMappedMoreThanOnce(p0));
        }

        public static Exception CouldNotAttach()
        {
            return new InvalidOperationException(Strings.CouldNotAttach);
        }

        public static Exception CouldNotConvert(object p0, object p1)
        {
            return new InvalidCastException(Strings.CouldNotConvert(p0, p1));
        }

        public static Exception CouldNotGetTableForSubtype(object p0, object p1)
        {
            return new InvalidOperationException(Strings.CouldNotGetTableForSubtype(p0, p1));
        }

        public static Exception CouldNotRemoveRelationshipBecauseOneSideCannotBeNull(object p0, object p1, object p2)
        {
            return new InvalidOperationException(Strings.CouldNotRemoveRelationshipBecauseOneSideCannotBeNull(p0, p1, p2));
        }

        public static Exception CycleDetected()
        {
            return new InvalidOperationException(Strings.CycleDetected);
        }

        public static Exception DataContextCannotBeUsedAfterDispose()
        {
            return new ObjectDisposedException(Strings.DataContextCannotBeUsedAfterDispose);
        }

        public static Exception DbGeneratedChangeNotAllowed(object p0, object p1)
        {
            return new InvalidOperationException(Strings.DbGeneratedChangeNotAllowed(p0, p1));
        }

        public static Exception DeferredLoadingRequiresObjectTracking()
        {
            return new InvalidOperationException(Strings.DeferredLoadingRequiresObjectTracking);
        }

        public static Exception EntityIsTheWrongType()
        {
            return new InvalidOperationException(Strings.EntityIsTheWrongType);
        }

        public static Exception EntitySetAlreadyLoaded()
        {
            return new InvalidOperationException(Strings.EntitySetAlreadyLoaded);
        }

        public static Exception EntitySetDataBindingWithAbstractBaseClass(object p0)
        {
            return new InvalidOperationException(Strings.EntitySetDataBindingWithAbstractBaseClass(p0));
        }

        public static Exception EntitySetDataBindingWithNonPublicDefaultConstructor(object p0)
        {
            return new InvalidOperationException(Strings.EntitySetDataBindingWithNonPublicDefaultConstructor(p0));
        }

        public static Exception EntitySetModifiedDuringEnumeration()
        {
            return new InvalidOperationException(Strings.EntitySetModifiedDuringEnumeration);
        }

        public static Exception ExpectedQueryableArgument(object p0, object p1)
        {
            return new ArgumentException(Strings.ExpectedQueryableArgument(p0, p1));
        }

        public static Exception ExpectedUpdateDeleteOrChange()
        {
            return new InvalidOperationException(Strings.ExpectedUpdateDeleteOrChange);
        }

        public static Exception IdentityChangeNotAllowed(object p0, object p1)
        {
            return new InvalidOperationException(Strings.IdentityChangeNotAllowed(p0, p1));
        }

        public static Exception IncludeCycleNotAllowed()
        {
            return new InvalidOperationException(Strings.IncludeCycleNotAllowed);
        }

        public static Exception IncludeNotAllowedAfterFreeze()
        {
            return new InvalidOperationException(Strings.IncludeNotAllowedAfterFreeze);
        }

        public static Exception InconsistentAssociationAndKeyChange(object p0, object p1)
        {
            return new InvalidOperationException(Strings.InconsistentAssociationAndKeyChange(p0, p1));
        }

        public static Exception InsertAutoSyncFailure()
        {
            return new InvalidOperationException(Strings.InsertAutoSyncFailure);
        }

        public static Exception InvalidLoadOptionsLoadMemberSpecification()
        {
            return new InvalidOperationException(Strings.InvalidLoadOptionsLoadMemberSpecification);
        }

        public static Exception KeyIsWrongSize(object p0, object p1)
        {
            return new InvalidOperationException(Strings.KeyIsWrongSize(p0, p1));
        }

        public static Exception KeyValueIsWrongType(object p0, object p1)
        {
            return new InvalidOperationException(Strings.KeyValueIsWrongType(p0, p1));
        }

        public static Exception LoadOptionsChangeNotAllowedAfterQuery()
        {
            return new InvalidOperationException(Strings.LoadOptionsChangeNotAllowedAfterQuery);
        }

        public static Exception ModifyDuringAddOrRemove()
        {
            return new ArgumentException(Strings.ModifyDuringAddOrRemove);
        }

        public static Exception NonEntityAssociationMapping(object p0, object p1, object p2)
        {
            return new InvalidOperationException(Strings.NonEntityAssociationMapping(p0, p1, p2));
        }

        public static Exception NotImplemented()
        {
            return new NotImplementedException();
        }

        public static Exception NotSupported()
        {
            return new NotSupportedException();
        }

        public static Exception ObjectTrackingRequired()
        {
            return new InvalidOperationException(Strings.ObjectTrackingRequired);
        }

        public static Exception OptionsCannotBeModifiedAfterQuery()
        {
            return new InvalidOperationException(Strings.OptionsCannotBeModifiedAfterQuery);
        }

        public static Exception OriginalEntityIsWrongType()
        {
            return new InvalidOperationException(Strings.OriginalEntityIsWrongType);
        }

        public static Exception ProviderDoesNotImplementRequiredInterface(object p0, object p1)
        {
            return new InvalidOperationException(Strings.ProviderDoesNotImplementRequiredInterface(p0, p1));
        }

        public static Exception ProviderTypeNull()
        {
            return new InvalidOperationException(Strings.ProviderTypeNull);
        }

        public static Exception RefreshOfDeletedObject()
        {
            return new InvalidOperationException(Strings.RefreshOfDeletedObject);
        }

        public static Exception SubqueryDoesNotSupportOperator(object p0)
        {
            return new NotSupportedException(Strings.SubqueryDoesNotSupportOperator(p0));
        }

        public static Exception SubqueryMustBeSequence()
        {
            return new InvalidOperationException(Strings.SubqueryMustBeSequence);
        }

        public static Exception SubqueryNotAllowedAfterFreeze()
        {
            return new InvalidOperationException(Strings.SubqueryNotAllowedAfterFreeze);
        }

        public static Exception SubqueryNotSupportedOn(object p0)
        {
            return new NotSupportedException(Strings.SubqueryNotSupportedOn(p0));
        }

        public static Exception SubqueryNotSupportedOnType(object p0, object p1)
        {
            return new NotSupportedException(Strings.SubqueryNotSupportedOnType(p0, p1));
        }

        public static Exception TypeCouldNotBeAdded(object p0)
        {
            return new InvalidOperationException(Strings.TypeCouldNotBeAdded(p0));
        }

        public static Exception TypeCouldNotBeRemoved(object p0)
        {
            return new InvalidOperationException(Strings.TypeCouldNotBeRemoved(p0));
        }

        public static Exception TypeCouldNotBeTracked(object p0)
        {
            return new InvalidOperationException(Strings.TypeCouldNotBeTracked(p0));
        }

        public static Exception TypeIsNotEntity(object p0)
        {
            return new InvalidOperationException(Strings.TypeIsNotEntity(p0));
        }

        public static Exception TypeIsNotMarkedAsTable(object p0)
        {
            return new InvalidOperationException(Strings.TypeIsNotMarkedAsTable(p0));
        }

        public static Exception UnableToDetermineDataContext()
        {
            return new InvalidOperationException(Strings.UnableToDetermineDataContext);
        }

        public static Exception UnhandledBindingType(object p0)
        {
            return new ArgumentException(Strings.UnhandledBindingType(p0));
        }

        public static Exception UnhandledExpressionType(object p0)
        {
            return new ArgumentException(Strings.UnhandledExpressionType(p0));
        }

        public static Exception UnrecognizedRefreshObject()
        {
            return new ArgumentException(Strings.UnrecognizedRefreshObject);
        }

        //public static void NoMethodInTypeMatchingArguments(object param0)
        //{
        //    throw new NotImplementedException();
        //}

        public static Exception ProviderCannotBeUsedAfterDispose()
        {
            return new ObjectDisposedException(Strings.ProviderCannotBeUsedAfterDispose);
        }

#if !FREE
        public static Exception LicenseFail(Type type, object instance, string s)
        {
            return new LicenseException(type, instance, s);
        }
#endif

        internal static Exception CannotContainsMethod(Type baseInterface, System.Reflection.MethodInfo method)
        {
            return new NotSupportedException(Strings.CannotContainsMethod(baseInterface, method));
        }

        internal static Exception CannotContainsProperty(Type baseInterface, System.Reflection.PropertyInfo property)
        {
            return new NotSupportedException(Strings.CannotContainsProperty(baseInterface, property));
        }


        public static Exception LicenseFileErrorFormat(Type type, object instance)
        {
            var msg = Messages.LicenseFileErrorFormat;
            return new LicenseException(type, instance, msg);
        }

        public static Exception CannotGetUserName(Type type, object instance)
        {
            var msg = Messages.CannotGetUserName;
            return new LicenseException(type, instance, msg);
        }

        public static Exception CannotGetLicenseKey(Type type, object instance)
        {
            var msg = Messages.CannotGetLicenseKey;
            return new LicenseException(type, instance, msg);
        }

        public static Exception CannotGetAssemblyName(Type type, object instance)
        {
            var msg = Messages.CannotGetAssemblyName;
            return new LicenseException(type, instance, msg);
        }

        public static Exception AssemblyNameNotMatch(Type type, object instance, string expectedAssemblyName, string actualAssemblyName)
        {
            var msg = string.Format(Messages.AssemblyNameNotMatch, expectedAssemblyName, actualAssemblyName);
            return new LicenseException(type, instance, msg);
        }


    }
}
