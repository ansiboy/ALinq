using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Xml.Schema;
using System.Globalization;

namespace ALinq.Mapping
{
    internal static class Error
    {
        // Methods
        internal static Exception AbstractClassAssignInheritanceDiscriminator(object p0)
        {
            return new InvalidOperationException(Strings.AbstractClassAssignInheritanceDiscriminator(p0));
        }

        internal static Exception ArgumentNull(string paramName)
        {
            return new ArgumentNullException(paramName);
        }

        internal static Exception ArgumentOutOfRange(string paramName)
        {
            return new ArgumentOutOfRangeException(paramName);
        }

        internal static Exception BadFunctionTypeInMethodMapping(object p0)
        {
            return new InvalidOperationException(Strings.BadFunctionTypeInMethodMapping(p0));
        }

        internal static Exception BadKeyMember(object p0, object p1, object p2)
        {
            return new InvalidOperationException(Strings.BadKeyMember(p0, p1, p2));
        }

        internal static Exception BadStorageProperty(object p0, object p1, object p2)
        {
            return new InvalidOperationException(Strings.BadStorageProperty(p0, p1, p2));
        }

        internal static Exception CannotGetInheritanceDefaultFromNonInheritanceClass()
        {
            return new InvalidOperationException(Strings.CannotGetInheritanceDefaultFromNonInheritanceClass);
        }

        internal static Exception CouldNotCreateAccessorToProperty(object p0, object p1, object p2)
        {
            return new ArgumentException(Strings.CouldNotCreateAccessorToProperty(p0, p1, p2));
        }

        internal static Exception CouldNotFindElementTypeInModel(object p0)
        {
            return new InvalidOperationException(Strings.CouldNotFindElementTypeInModel(p0));
        }

        internal static Exception CouldNotFindRequiredAttribute(object p0, object p1)
        {
            return new XmlSchemaException(Strings.CouldNotFindRequiredAttribute(p0, p1));
        }

        internal static Exception CouldNotFindRuntimeTypeForMapping(object p0)
        {
            return new InvalidOperationException(Strings.CouldNotFindRuntimeTypeForMapping(p0));
        }

        internal static Exception CouldNotFindTypeFromMapping(object p0)
        {
            return new InvalidOperationException(Strings.CouldNotFindTypeFromMapping(p0));
        }

        internal static Exception DatabaseNodeNotFound(object p0)
        {
            return new XmlSchemaException(Strings.DatabaseNodeNotFound(p0));
        }

        internal static Exception DiscriminatorClrTypeNotSupported(object p0, object p1, object p2)
        {
            return new NotSupportedException(Strings.DiscriminatorClrTypeNotSupported(p0, p1, p2));
        }

        internal static Exception EntityRefAlreadyLoaded()
        {
            return new InvalidOperationException(Strings.EntityRefAlreadyLoaded);
        }

        internal static Exception ExpectedEmptyElement(object p0, object p1, object p2)
        {
            return new XmlSchemaException(Strings.ExpectedEmptyElement(p0, p1, p2));
        }

        internal static Exception IdentityClrTypeNotSupported(object p0, object p1, object p2)
        {
            return new NotSupportedException(Strings.IdentityClrTypeNotSupported(p0, p1, p2));
        }

        internal static Exception IncorrectAutoSyncSpecification(object p0)
        {
            return new InvalidOperationException(Strings.IncorrectAutoSyncSpecification(p0));
        }

        internal static Exception IncorrectNumberOfParametersMappedForMethod(object p0)
        {
            return new InvalidOperationException(Strings.IncorrectNumberOfParametersMappedForMethod(p0));
        }

        internal static Exception InheritanceCodeMayNotBeNull()
        {
            return new InvalidOperationException(Strings.InheritanceCodeMayNotBeNull);
        }

        internal static Exception InheritanceCodeUsedForMultipleTypes(object p0)
        {
            return new InvalidOperationException(Strings.InheritanceCodeUsedForMultipleTypes(p0));
        }

        internal static Exception InheritanceHierarchyDoesNotDefineDefault(object p0)
        {
            return new InvalidOperationException(Strings.InheritanceHierarchyDoesNotDefineDefault(p0));
        }

        internal static Exception InheritanceSubTypeIsAlsoRoot(object p0)
        {
            return new InvalidOperationException(Strings.InheritanceSubTypeIsAlsoRoot(p0));
        }

        internal static Exception InheritanceTypeDoesNotDeriveFromRoot(object p0, object p1)
        {
            return new InvalidOperationException(Strings.InheritanceTypeDoesNotDeriveFromRoot(p0, p1));
        }

        internal static Exception InheritanceTypeHasMultipleDefaults(object p0)
        {
            return new InvalidOperationException(Strings.InheritanceTypeHasMultipleDefaults(p0));
        }

        internal static Exception InheritanceTypeHasMultipleDiscriminators(object p0)
        {
            return new InvalidOperationException(Strings.InheritanceTypeHasMultipleDiscriminators(p0));
        }

        internal static Exception InvalidDeleteOnNullSpecification(object p0)
        {
            return new InvalidOperationException(Strings.InvalidDeleteOnNullSpecification(p0));
        }

        internal static Exception InvalidFieldInfo(object p0, object p1, object p2)
        {
            return new ArgumentException(Strings.InvalidFieldInfo(p0, p1, p2));
        }

        internal static Exception InvalidUseOfGenericMethodAsMappedFunction(object p0)
        {
            return new NotSupportedException(Strings.InvalidUseOfGenericMethodAsMappedFunction(p0));
        }

        internal static Exception LinkAlreadyLoaded()
        {
            return new InvalidOperationException(Strings.LinkAlreadyLoaded);
        }

        internal static Exception MappedMemberHadNoCorrespondingMemberInType(object p0, object p1)
        {
            return new NotSupportedException(Strings.MappedMemberHadNoCorrespondingMemberInType(p0, p1));
        }

        internal static Exception MappingForTableUndefined(object p0)
        {
            return new InvalidOperationException(Strings.MappingForTableUndefined(p0));
        }

        internal static Exception MappingOfInterfacesMemberIsNotSupported(object p0, object p1)
        {
            return new NotSupportedException(Strings.MappingOfInterfacesMemberIsNotSupported(p0, p1));
        }

        internal static Exception MemberMappedMoreThanOnce(object p0)
        {
            return new InvalidOperationException(Strings.MemberMappedMoreThanOnce(p0));
        }

        internal static Exception MethodCannotBeFound(object p0)
        {
            return new InvalidOperationException(Strings.MethodCannotBeFound(p0));
        }

        internal static Exception MismatchedThisKeyOtherKey(object p0, object p1)
        {
            return new InvalidOperationException(Strings.MismatchedThisKeyOtherKey(p0, p1));
        }

        internal static Exception NoDiscriminatorFound(object p0)
        {
            return new InvalidOperationException(Strings.NoDiscriminatorFound(p0));
        }

        internal static Exception NonInheritanceClassHasDiscriminator(object p0)
        {
            return new InvalidOperationException(Strings.NonInheritanceClassHasDiscriminator(p0));
        }

        internal static Exception NoResultTypesDeclaredForFunction(object p0)
        {
            return new InvalidOperationException(Strings.NoResultTypesDeclaredForFunction(p0));
        }

        internal static Exception NotImplemented()
        {
            return new NotImplementedException();
        }

        internal static Exception NotSupported()
        {
            return new NotSupportedException();
        }

        internal static Exception PrimaryKeyInSubTypeNotSupported(object p0, object p1)
        {
            return new NotSupportedException(Strings.PrimaryKeyInSubTypeNotSupported(p0, p1));
        }

        internal static Exception ProviderTypeNotFound(object p0)
        {
            return new InvalidOperationException(Strings.ProviderTypeNotFound(p0));
        }

        internal static Exception TooManyResultTypesDeclaredForFunction(object p0)
        {
            return new InvalidOperationException(Strings.TooManyResultTypesDeclaredForFunction(p0));
        }

        internal static Exception TwoMembersMarkedAsInheritanceDiscriminator(object p0, object p1)
        {
            return new InvalidOperationException(Strings.TwoMembersMarkedAsInheritanceDiscriminator(p0, p1));
        }

        internal static Exception TwoMembersMarkedAsPrimaryKeyAndDBGenerated(object p0, object p1)
        {
            return new InvalidOperationException(Strings.TwoMembersMarkedAsPrimaryKeyAndDBGenerated(p0, p1));
        }

        internal static Exception TwoMembersMarkedAsRowVersion(object p0, object p1)
        {
            return new InvalidOperationException(Strings.TwoMembersMarkedAsRowVersion(p0, p1));
        }

        internal static Exception UnableToAssignValueToReadonlyProperty(object p0)
        {
            return new InvalidOperationException(Strings.UnableToAssignValueToReadonlyProperty(p0));
        }

        internal static Exception UnableToResolveRootForType(object p0)
        {
            return new InvalidOperationException(Strings.UnableToResolveRootForType(p0));
        }

        internal static Exception UnexpectedElement(object p0, object p1)
        {
            return new XmlSchemaException(Strings.UnexpectedElement(p0, p1));
        }

        internal static Exception UnexpectedNull(object p0)
        {
            return new InvalidOperationException(Strings.UnexpectedNull(p0));
        }

        internal static Exception UnhandledDeferredStorageType(object p0)
        {
            return new InvalidOperationException(Strings.UnhandledDeferredStorageType(p0));
        }

        internal static Exception UnmappedClassMember(object p0, object p1)
        {
            return new InvalidOperationException(Strings.UnmappedClassMember(p0, p1));
        }

        internal static Exception UnrecognizedAttribute(object p0)
        {
            return new XmlSchemaException(Strings.UnrecognizedAttribute(p0));
        }

        internal static Exception UnrecognizedElement(object p0)
        {
            return new XmlSchemaException(Strings.UnrecognizedElement(p0));
        }

        internal static Exception MustBeInterface(Type type)
        {
            return new NotSupportedException(Strings.MustBeInterface(type));
        }

        internal static Exception CannotAccessInterface(Type type)
        {
            return new Exception(Strings.CannotAccessInterface(type));
        }

        internal static Exception CouldNotFindMappingForDataContextType(Type type)
        {
            return new Exception(Strings.CouldNotFindMappingForDataContextType(type));
        }

        internal static Exception DataContextTypeMappedMoreThanOnce(Type type)
        {
            return new Exception(Strings.DataContextTypeMappedMoreThanOnce(type));
        }

        internal static Exception TypeNotContainsMember(Type type, MemberInfo mi)
        {
            return new Exception(Strings.TypeNotContainsMember(type, mi));
        }
    }

    internal static class Strings
    {
        // Methods
        internal static string AbstractClassAssignInheritanceDiscriminator(object p0)
        {
            return SR.GetString("AbstractClassAssignInheritanceDiscriminator", new object[] { p0 });
        }

        internal static string BadFunctionTypeInMethodMapping(object p0)
        {
            return SR.GetString("BadFunctionTypeInMethodMapping", new object[] { p0 });
        }

        internal static string BadKeyMember(object p0, object p1, object p2)
        {
            return SR.GetString("BadKeyMember", new object[] { p0, p1, p2 });
        }

        internal static string BadStorageProperty(object p0, object p1, object p2)
        {
            return SR.GetString("BadStorageProperty", new object[] { p0, p1, p2 });
        }

        internal static string CouldNotCreateAccessorToProperty(object p0, object p1, object p2)
        {
            return SR.GetString("CouldNotCreateAccessorToProperty", new object[] { p0, p1, p2 });
        }

        internal static string CouldNotFindElementTypeInModel(object p0)
        {
            return SR.GetString("CouldNotFindElementTypeInModel", new object[] { p0 });
        }

        internal static string CouldNotFindRequiredAttribute(object p0, object p1)
        {
            return SR.GetString("CouldNotFindRequiredAttribute", new object[] { p0, p1 });
        }

        internal static string CouldNotFindRuntimeTypeForMapping(object p0)
        {
            return SR.GetString("CouldNotFindRuntimeTypeForMapping", new object[] { p0 });
        }

        internal static string CouldNotFindTypeFromMapping(object p0)
        {
            return SR.GetString("CouldNotFindTypeFromMapping", new object[] { p0 });
        }

        internal static string DatabaseNodeNotFound(object p0)
        {
            return SR.GetString("DatabaseNodeNotFound", new object[] { p0 });
        }

        internal static string DiscriminatorClrTypeNotSupported(object p0, object p1, object p2)
        {
            return SR.GetString("DiscriminatorClrTypeNotSupported", new object[] { p0, p1, p2 });
        }

        internal static string ExpectedEmptyElement(object p0, object p1, object p2)
        {
            return SR.GetString("ExpectedEmptyElement", new object[] { p0, p1, p2 });
        }

        internal static string IdentityClrTypeNotSupported(object p0, object p1, object p2)
        {
            return SR.GetString("IdentityClrTypeNotSupported", new object[] { p0, p1, p2 });
        }

        internal static string IncorrectAutoSyncSpecification(object p0)
        {
            return SR.GetString("IncorrectAutoSyncSpecification", new object[] { p0 });
        }

        internal static string IncorrectNumberOfParametersMappedForMethod(object p0)
        {
            return SR.GetString("IncorrectNumberOfParametersMappedForMethod", new object[] { p0 });
        }

        internal static string InheritanceCodeUsedForMultipleTypes(object p0)
        {
            return SR.GetString("InheritanceCodeUsedForMultipleTypes", new object[] { p0 });
        }

        internal static string InheritanceHierarchyDoesNotDefineDefault(object p0)
        {
            return SR.GetString("InheritanceHierarchyDoesNotDefineDefault", new object[] { p0 });
        }

        internal static string InheritanceSubTypeIsAlsoRoot(object p0)
        {
            return SR.GetString("InheritanceSubTypeIsAlsoRoot", new object[] { p0 });
        }

        internal static string InheritanceTypeDoesNotDeriveFromRoot(object p0, object p1)
        {
            return SR.GetString("InheritanceTypeDoesNotDeriveFromRoot", new object[] { p0, p1 });
        }

        internal static string InheritanceTypeHasMultipleDefaults(object p0)
        {
            return SR.GetString("InheritanceTypeHasMultipleDefaults", new object[] { p0 });
        }

        internal static string InheritanceTypeHasMultipleDiscriminators(object p0)
        {
            return SR.GetString("InheritanceTypeHasMultipleDiscriminators", new object[] { p0 });
        }

        internal static string InvalidDeleteOnNullSpecification(object p0)
        {
            return SR.GetString("InvalidDeleteOnNullSpecification", new object[] { p0 });
        }

        internal static string InvalidFieldInfo(object p0, object p1, object p2)
        {
            return SR.GetString("InvalidFieldInfo", new object[] { p0, p1, p2 });
        }

        internal static string InvalidUseOfGenericMethodAsMappedFunction(object p0)
        {
            return SR.GetString("InvalidUseOfGenericMethodAsMappedFunction", new object[] { p0 });
        }

        internal static string MappedMemberHadNoCorrespondingMemberInType(object p0, object p1)
        {
            return SR.GetString("MappedMemberHadNoCorrespondingMemberInType", new object[] { p0, p1 });
        }

        internal static string MappingForTableUndefined(object p0)
        {
            return SR.GetString("MappingForTableUndefined", new object[] { p0 });
        }

        internal static string MappingOfInterfacesMemberIsNotSupported(object p0, object p1)
        {
            return SR.GetString("MappingOfInterfacesMemberIsNotSupported", new object[] { p0, p1 });
        }

        internal static string MemberMappedMoreThanOnce(object p0)
        {
            return SR.GetString("MemberMappedMoreThanOnce", new object[] { p0 });
        }

        internal static string MethodCannotBeFound(object p0)
        {
            return SR.GetString("MethodCannotBeFound", new object[] { p0 });
        }

        internal static string MismatchedThisKeyOtherKey(object p0, object p1)
        {
            return SR.GetString("MismatchedThisKeyOtherKey", new object[] { p0, p1 });
        }

        internal static string NoDiscriminatorFound(object p0)
        {
            return SR.GetString("NoDiscriminatorFound", new object[] { p0 });
        }

        internal static string NonInheritanceClassHasDiscriminator(object p0)
        {
            return SR.GetString("NonInheritanceClassHasDiscriminator", new object[] { p0 });
        }

        internal static string NoResultTypesDeclaredForFunction(object p0)
        {
            return SR.GetString("NoResultTypesDeclaredForFunction", new object[] { p0 });
        }

        internal static string PrimaryKeyInSubTypeNotSupported(object p0, object p1)
        {
            return SR.GetString("PrimaryKeyInSubTypeNotSupported", new object[] { p0, p1 });
        }

        internal static string ProviderTypeNotFound(object p0)
        {
            return SR.GetString("ProviderTypeNotFound", new object[] { p0 });
        }

        internal static string TooManyResultTypesDeclaredForFunction(object p0)
        {
            return SR.GetString("TooManyResultTypesDeclaredForFunction", new object[] { p0 });
        }

        internal static string TwoMembersMarkedAsInheritanceDiscriminator(object p0, object p1)
        {
            return SR.GetString("TwoMembersMarkedAsInheritanceDiscriminator", new object[] { p0, p1 });
        }

        internal static string TwoMembersMarkedAsPrimaryKeyAndDBGenerated(object p0, object p1)
        {
            return SR.GetString("TwoMembersMarkedAsPrimaryKeyAndDBGenerated", new object[] { p0, p1 });
        }

        internal static string TwoMembersMarkedAsRowVersion(object p0, object p1)
        {
            return SR.GetString("TwoMembersMarkedAsRowVersion", new object[] { p0, p1 });
        }

        internal static string UnableToAssignValueToReadonlyProperty(object p0)
        {
            return SR.GetString("UnableToAssignValueToReadonlyProperty", new object[] { p0 });
        }

        internal static string UnableToResolveRootForType(object p0)
        {
            return SR.GetString("UnableToResolveRootForType", new object[] { p0 });
        }

        internal static string UnexpectedElement(object p0, object p1)
        {
            return SR.GetString("UnexpectedElement", new object[] { p0, p1 });
        }

        internal static string UnexpectedNull(object p0)
        {
            return SR.GetString("UnexpectedNull", new object[] { p0 });
        }

        internal static string UnhandledDeferredStorageType(object p0)
        {
            return SR.GetString("UnhandledDeferredStorageType", new object[] { p0 });
        }

        internal static string UnmappedClassMember(object p0, object p1)
        {
            return SR.GetString("UnmappedClassMember", new object[] { p0, p1 });
        }

        internal static string UnrecognizedAttribute(object p0)
        {
            return SR.GetString("UnrecognizedAttribute", new object[] { p0 });
        }

        internal static string UnrecognizedElement(object p0)
        {
            return SR.GetString("UnrecognizedElement", new object[] { p0 });
        }

        // Properties
        internal static string CannotGetInheritanceDefaultFromNonInheritanceClass
        {
            get
            {
                return SR.GetString("CannotGetInheritanceDefaultFromNonInheritanceClass");
            }
        }

        internal static string EntityRefAlreadyLoaded
        {
            get
            {
                return SR.GetString("EntityRefAlreadyLoaded");
            }
        }

        internal static string InheritanceCodeMayNotBeNull
        {
            get
            {
                return SR.GetString("InheritanceCodeMayNotBeNull");
            }
        }

        internal static string LinkAlreadyLoaded
        {
            get
            {
                return SR.GetString("LinkAlreadyLoaded");
            }
        }

        internal static string OwningTeam
        {
            get
            {
                return SR.GetString("OwningTeam");
            }
        }

        public static string CannotAccessInterface(Type type)
        {
            return SR.GetString("CannotAccessInterface", type);
        }

        internal static string MustBeInterface(Type type)
        {
            return SR.GetString("MustBeInterface", type.Name);
        }

        internal static string CouldNotFindMappingForDataContextType(Type type)
        {
            return SR.GetString("CouldNotFindMappingForDataContextType", type.Name);
        }

        public static string DataContextTypeMappedMoreThanOnce(Type type)
        {
            return SR.GetString("DataContextTypeMappedMoreThanOnce", type.Name);
        }

        public static string TypeNotContainsMember(Type type, MemberInfo mi)
        {
            return SR.GetString("TypeNotContainsMember", type.Name, mi.Name);
        }
    }

    internal sealed class SR
    {
        // Fields
        internal const string AbstractClassAssignInheritanceDiscriminator = "AbstractClassAssignInheritanceDiscriminator";
        internal const string BadFunctionTypeInMethodMapping = "BadFunctionTypeInMethodMapping";
        internal const string BadKeyMember = "BadKeyMember";
        internal const string BadStorageProperty = "BadStorageProperty";
        internal const string CannotGetInheritanceDefaultFromNonInheritanceClass = "CannotGetInheritanceDefaultFromNonInheritanceClass";
        internal const string CouldNotCreateAccessorToProperty = "CouldNotCreateAccessorToProperty";
        internal const string CouldNotFindElementTypeInModel = "CouldNotFindElementTypeInModel";
        internal const string CouldNotFindRequiredAttribute = "CouldNotFindRequiredAttribute";
        internal const string CouldNotFindRuntimeTypeForMapping = "CouldNotFindRuntimeTypeForMapping";
        internal const string CouldNotFindTypeFromMapping = "CouldNotFindTypeFromMapping";
        internal const string DatabaseNodeNotFound = "DatabaseNodeNotFound";
        internal const string DiscriminatorClrTypeNotSupported = "DiscriminatorClrTypeNotSupported";
        internal const string EntityRefAlreadyLoaded = "EntityRefAlreadyLoaded";
        internal const string ExpectedEmptyElement = "ExpectedEmptyElement";
        internal const string IdentityClrTypeNotSupported = "IdentityClrTypeNotSupported";
        internal const string IncorrectAutoSyncSpecification = "IncorrectAutoSyncSpecification";
        internal const string IncorrectNumberOfParametersMappedForMethod = "IncorrectNumberOfParametersMappedForMethod";
        internal const string InheritanceCodeMayNotBeNull = "InheritanceCodeMayNotBeNull";
        internal const string InheritanceCodeUsedForMultipleTypes = "InheritanceCodeUsedForMultipleTypes";
        internal const string InheritanceHierarchyDoesNotDefineDefault = "InheritanceHierarchyDoesNotDefineDefault";
        internal const string InheritanceSubTypeIsAlsoRoot = "InheritanceSubTypeIsAlsoRoot";
        internal const string InheritanceTypeDoesNotDeriveFromRoot = "InheritanceTypeDoesNotDeriveFromRoot";
        internal const string InheritanceTypeHasMultipleDefaults = "InheritanceTypeHasMultipleDefaults";
        internal const string InheritanceTypeHasMultipleDiscriminators = "InheritanceTypeHasMultipleDiscriminators";
        internal const string InvalidDeleteOnNullSpecification = "InvalidDeleteOnNullSpecification";
        internal const string InvalidFieldInfo = "InvalidFieldInfo";
        internal const string InvalidUseOfGenericMethodAsMappedFunction = "InvalidUseOfGenericMethodAsMappedFunction";
        internal const string LinkAlreadyLoaded = "LinkAlreadyLoaded";
        private static SR loader;
        internal const string MappedMemberHadNoCorrespondingMemberInType = "MappedMemberHadNoCorrespondingMemberInType";
        internal const string MappingForTableUndefined = "MappingForTableUndefined";
        internal const string MappingOfInterfacesMemberIsNotSupported = "MappingOfInterfacesMemberIsNotSupported";
        internal const string MemberMappedMoreThanOnce = "MemberMappedMoreThanOnce";
        internal const string MethodCannotBeFound = "MethodCannotBeFound";
        internal const string MismatchedThisKeyOtherKey = "MismatchedThisKeyOtherKey";
        internal const string NoDiscriminatorFound = "NoDiscriminatorFound";
        internal const string NonInheritanceClassHasDiscriminator = "NonInheritanceClassHasDiscriminator";
        internal const string NoResultTypesDeclaredForFunction = "NoResultTypesDeclaredForFunction";
        internal const string OwningTeam = "OwningTeam";
        internal const string PrimaryKeyInSubTypeNotSupported = "PrimaryKeyInSubTypeNotSupported";
        internal const string ProviderTypeNotFound = "ProviderTypeNotFound";
        private ResourceManager resources;
        internal const string TooManyResultTypesDeclaredForFunction = "TooManyResultTypesDeclaredForFunction";
        internal const string TwoMembersMarkedAsInheritanceDiscriminator = "TwoMembersMarkedAsInheritanceDiscriminator";
        internal const string TwoMembersMarkedAsPrimaryKeyAndDBGenerated = "TwoMembersMarkedAsPrimaryKeyAndDBGenerated";
        internal const string TwoMembersMarkedAsRowVersion = "TwoMembersMarkedAsRowVersion";
        internal const string UnableToAssignValueToReadonlyProperty = "UnableToAssignValueToReadonlyProperty";
        internal const string UnableToResolveRootForType = "UnableToResolveRootForType";
        internal const string UnexpectedElement = "UnexpectedElement";
        internal const string UnexpectedNull = "UnexpectedNull";
        internal const string UnhandledDeferredStorageType = "UnhandledDeferredStorageType";
        internal const string UnmappedClassMember = "UnmappedClassMember";
        internal const string UnrecognizedAttribute = "UnrecognizedAttribute";
        internal const string UnrecognizedElement = "UnrecognizedElement";

        // Methods
        internal SR()
        {
            this.resources = new ResourceManager("ALinq.Resources.ALinq.Mapping", GetType().Assembly);
        }

        private static SR GetLoader()
        {
            if (loader == null)
            {
                SR sr = new SR();
                Interlocked.CompareExchange<SR>(ref loader, sr, null);
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
        private static CultureInfo Culture
        {
            get
            {
                return null;
            }
        }

        public static ResourceManager Resources
        {
            get
            {
                return GetLoader().resources;
            }
        }
    }


}
