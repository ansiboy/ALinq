using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using ALinq;
using ALinq.Mapping;
using System.Linq;
using System.Text;

namespace ALinq
{
    internal abstract class ChangeTracker
    {
        // Methods

        internal abstract void AcceptChanges();

        internal static ChangeTracker CreateChangeTracker(CommonDataServices dataServices, bool asReadOnly)
        {
            if (asReadOnly)
            {
                return new ReadOnlyChangeTracker();
            }
            return new StandardChangeTracker(dataServices);
        }

        internal abstract void FastTrack(object obj);
        internal abstract IEnumerable<TrackedObject> GetInterestingObjects();
        internal abstract TrackedObject GetTrackedObject(object obj);
        internal abstract bool IsTracked(object obj);
        internal abstract void StopTracking(object obj);
        internal abstract TrackedObject Track(object obj);
        internal abstract TrackedObject Track(object obj, bool recurse);

        // Nested Types
        private class ReadOnlyChangeTracker : ChangeTracker
        {
            // Methods
            internal override void AcceptChanges()
            {
            }

            internal override void FastTrack(object obj)
            {
            }

            internal override IEnumerable<TrackedObject> GetInterestingObjects()
            {
                return new TrackedObject[0];
            }

            internal override TrackedObject GetTrackedObject(object obj)
            {
                return null;
            }

            internal override bool IsTracked(object obj)
            {
                return false;
            }

            internal override void StopTracking(object obj)
            {
            }

            internal override TrackedObject Track(object obj)
            {
                return null;
            }

            internal override TrackedObject Track(object obj, bool recurse)
            {
                return null;
            }
        }

        private class StandardChangeTracker : ChangeTracker
        {
            // Fields
            private readonly Dictionary<object, StandardTrackedObject> items;
            private readonly PropertyChangingEventHandler onPropertyChanging;
            private readonly CommonDataServices services;

            // Methods
            internal StandardChangeTracker(CommonDataServices services)
            {
                this.services = services;
                items = new Dictionary<object, StandardTrackedObject>();
                onPropertyChanging = OnPropertyChanging;
            }

            internal override void AcceptChanges()
            {
                var list = new List<StandardTrackedObject>(items.Values);
                foreach (var obj2 in list)
                {
                    obj2.AcceptChanges();
                }
            }

            private void Attach(object obj)
            {
                var changing = obj as INotifyPropertyChanging;
                if (changing != null)
                {
                    changing.PropertyChanging += onPropertyChanging;
                }
                else
                {
                    OnPropertyChanging(obj, null);
                }
            }

            private void Detach(object obj)
            {
                var changing = obj as INotifyPropertyChanging;
                if (changing != null)
                {
                    changing.PropertyChanging -= onPropertyChanging;
                }
            }

            internal override void FastTrack(object obj)
            {
                Attach(obj);
            }

            internal override IEnumerable<TrackedObject> GetInterestingObjects()
            {
                foreach (var item in items)
                {
                    if (item.Value.IsInteresting)
                        yield return item.Value;
                }
            }

            internal override TrackedObject GetTrackedObject(object obj)
            {
                StandardTrackedObject obj2;
                if (!items.TryGetValue(obj, out obj2) && IsFastTracked(obj))
                {
                    return PromoteFastTrackedObject(obj);
                }
                return obj2;
            }

            private bool IsFastTracked(object obj)
            {
                MetaType rowType = services.Model.GetTable(obj.GetType()).RowType;
                return services.IsCachedObject(rowType, obj);
            }

            private static bool IsSameDiscriminator(object discriminator1, object discriminator2)
            {
                return ((discriminator1 == discriminator2) ||
                        (((discriminator1 != null) && (discriminator2 != null)) && discriminator1.Equals(discriminator2)));
            }

            internal override bool IsTracked(object obj)
            {
                if (!items.ContainsKey(obj))
                {
                    return IsFastTracked(obj);
                }
                return true;
            }

            private void OnPropertyChanging(object sender, PropertyChangingEventArgs args)
            {
                StandardTrackedObject obj2;
                if (items.TryGetValue(sender, out obj2))
                {
                    obj2.StartTracking();
                }
                else if (IsFastTracked(sender))
                {
                    PromoteFastTrackedObject(sender).StartTracking();
                }
            }

            private StandardTrackedObject PromoteFastTrackedObject(object obj)
            {
                Type type = obj.GetType();
                MetaType inheritanceType = services.Model.GetTable(type).RowType.GetInheritanceType(type);
                return PromoteFastTrackedObject(inheritanceType, obj);
            }

            private StandardTrackedObject PromoteFastTrackedObject(MetaType type, object obj)
            {
                var obj2 = new StandardTrackedObject(this, type, obj, obj);
                items.Add(obj, obj2);
                return obj2;
            }

            internal override void StopTracking(object obj)
            {
                Detach(obj);
                items.Remove(obj);
            }

            internal override TrackedObject Track(object obj)
            {
                return Track(obj, false);
            }

            internal override TrackedObject Track(object obj, bool recurse)
            {
                MetaType metaType = services.Model.GetMetaType(obj.GetType());
                var visited = new Dictionary<object, object>();
                return Track(metaType, obj, visited, recurse, 1);
            }

            private TrackedObject Track(MetaType mt, object obj, IDictionary<object, object> visited, bool recurse,
                                        int level)
            {
                var trackedObject = (StandardTrackedObject)GetTrackedObject(obj);
                if ((trackedObject == null) && !visited.ContainsKey(obj))
                {
                    bool isWeaklyTracked = level > 1;
                    trackedObject = new StandardTrackedObject(this, mt, obj, obj, isWeaklyTracked);
                    if (trackedObject.HasDeferredLoaders)
                    {
                        throw Error.CannotAttachAddNonNewEntities();
                    }
                    items.Add(obj, trackedObject);
                    Attach(obj);
                    visited.Add(obj, obj);
                    if (!recurse)
                    {
                        return trackedObject;
                    }
                    foreach (RelatedItem item in services.GetParents(mt, obj))
                    {
                        Track(item.Type, item.Item, visited, recurse, level + 1);
                    }
                    foreach (RelatedItem item2 in services.GetChildren(mt, obj))
                    {
                        Track(item2.Type, item2.Item, visited, recurse, level + 1);
                    }
                }

                return trackedObject;
            }

            private static MetaType TypeFromDiscriminator(MetaType root, object discriminator)
            {
                foreach (MetaType type in root.InheritanceTypes)
                {
                    if (IsSameDiscriminator(discriminator, type.InheritanceCode))
                    {
                        return type;
                    }
                }
                return root.InheritanceDefault;
            }

            // Nested Types


            private class StandardTrackedObject : TrackedObject
            {
                // Fields
                private object current;
                private readonly BitArray dirtyMemberCache;
                private bool haveInitializedDeferredLoaders;
                private bool isWeaklyTracked;
                private object original;
                private State state;
                private readonly StandardChangeTracker tracker;
                private readonly MetaType type;

                // Methods
                internal StandardTrackedObject(StandardChangeTracker tracker, MetaType type, object current,
                                               object original)
                {
                    if (current == null)
                    {
                        throw Error.ArgumentNull("current");
                    }
                    this.tracker = tracker;
                    this.type = type.GetInheritanceType(current.GetType());
                    this.current = current;
                    this.original = original;
                    state = State.PossiblyModified;
                    //if (type is DynamicMetaType)
                    //{
                    //    //TODO:由于 DynamicMetaType.DataMembers 数量是变值，只能暂时给最个大值，
                    //    dirtyMemberCache = new BitArray(DynamicMetaType.MaxDataMembersCount);
                    //}
                    //else
                    dirtyMemberCache = new BitArray(this.type.DataMembers.Count);
                }

                internal StandardTrackedObject(StandardChangeTracker tracker, MetaType type, object current,
                                               object original, bool isWeaklyTracked)
                    : this(tracker, type, current, original)
                {
                    this.isWeaklyTracked = isWeaklyTracked;
                }

                internal override void AcceptChanges()
                {
                    if (IsWeaklyTracked)
                    {
                        InitializeDeferredLoaders();
                        isWeaklyTracked = false;
                    }
                    if (IsDeleted)
                    {
                        ConvertToDead();
                    }
                    else if (IsNew)
                    {
                        InitializeDeferredLoaders();
                        ConvertToUnmodified();
                    }
                    else if (IsPossiblyModified)
                    {
                        ConvertToUnmodified();
                    }
                }

                private void AssignMember(object instance, MetaDataMember mm, object value)
                {
                    if (!(current is INotifyPropertyChanging))
                    {
                        mm.StorageAccessor.SetBoxedValue(ref instance, value);
                    }
                    else
                    {
                        mm.MemberAccessor.SetBoxedValue(ref instance, value);
                    }
                }

                internal override bool CanInferDelete()
                {
                    if ((state == State.Modified) || (state == State.PossiblyModified))
                    {
                        foreach (MetaAssociation association in Type.Associations)
                        {
                            if (((association.DeleteOnNull && association.IsForeignKey) &&
                                 (!association.IsNullable && !association.IsMany)) &&
                                (association.ThisMember.StorageAccessor.HasAssignedValue(Current) &&
                                 (association.ThisMember.StorageAccessor.GetBoxedValue(Current) == null)))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                internal override void ConvertToDead()
                {
                    state = State.Dead;
                    isWeaklyTracked = false;
                }

                internal override void ConvertToDeleted()
                {
                    state = State.Deleted;
                    isWeaklyTracked = false;
                }

                internal override void ConvertToModified()
                {
                    state = State.Modified;
                    isWeaklyTracked = false;
                }

                internal override void ConvertToNew()
                {
                    original = null;
                    state = State.New;
                }

                internal override void ConvertToPossiblyModified()
                {
                    state = State.PossiblyModified;
                    isWeaklyTracked = false;
                }

                internal override void ConvertToPossiblyModified(object originalState)
                {
                    state = State.PossiblyModified;
                    original = CreateDataCopy(originalState);
                    isWeaklyTracked = false;
                }

                internal override void ConvertToRemoved()
                {
                    state = State.Removed;
                    isWeaklyTracked = false;
                }

                internal override void ConvertToUnmodified()
                {
                    state = State.PossiblyModified;
                    if (current is INotifyPropertyChanging)
                    {
                        original = current;
                    }
                    else
                    {
                        original = CreateDataCopy(current);
                    }
                    ResetDirtyMemberTracking();
                    isWeaklyTracked = false;
                }

                internal override object CreateDataCopy(object instance)
                {
                    Type objType = instance.GetType();
                    object obj2 = Activator.CreateInstance(Type.Type);
                    foreach (
                        MetaDataMember member in
                            tracker.services.Model.GetTable(objType).RowType.InheritanceRoot.GetInheritanceType(objType).
                                PersistentDataMembers)
                    {
                        if ((Type.Type != objType) && !member.DeclaringType.Type.IsAssignableFrom(objType))
                        {
                            continue;
                        }
                        if (member.IsDeferred)
                        {
                            if (!member.IsAssociation)
                            {
                                if (member.StorageAccessor.HasValue(instance))
                                {
                                    object obj3 = member.DeferredValueAccessor.GetBoxedValue(instance);
                                    member.DeferredValueAccessor.SetBoxedValue(ref obj2, obj3);
                                }
                                else
                                {
                                    IEnumerable enumerable =
                                        tracker.services.GetDeferredSourceFactory(member).CreateDeferredSource(obj2);
                                    member.DeferredSourceAccessor.SetBoxedValue(ref obj2, enumerable);
                                }
                            }
                            continue;
                        }
                        object boxedValue = member.StorageAccessor.GetBoxedValue(instance);
                        member.StorageAccessor.SetBoxedValue(ref obj2, boxedValue);
                    }
                    return obj2;
                }

                private IEnumerable<MetaDataMember> GetAssociationsForKey(MetaDataMember key)
                {
                    foreach (var dataMember in type.PersistentDataMembers)
                    {
                        if (dataMember.IsAssociation && dataMember.Association.ThisKey.Contains(key))
                            yield return dataMember;
                    }
                }

                internal override IEnumerable<ModifiedMemberInfo> GetModifiedMembers()
                {
                    foreach (var mm in type.PersistentDataMembers)
                    {
                        if (IsModifiedMember(mm))
                        {
                            var currentValue = mm.MemberAccessor.GetBoxedValue(current);
                            if (original != null && mm.StorageAccessor.HasValue(original))
                            {
                                var originalValue = mm.MemberAccessor.GetBoxedValue(original);
                                yield return new ModifiedMemberInfo(mm.Member, currentValue, originalValue);
                            }
                            if (original == null || (mm.IsDeferred && !mm.StorageAccessor.HasLoadedValue(current)))
                            {
                                yield return new ModifiedMemberInfo(mm.Member, currentValue, null);
                            }
                        }
                    }
                }

                private string GetState()
                {
                    switch (state)
                    {
                        case State.New:
                        case State.Deleted:
                        case State.Removed:
                        case State.Dead:
                            return state.ToString();
                    }
                    if (IsModified)
                    {
                        return "Modified";
                    }
                    return "Unmodified";
                }

                internal override bool HasChangedValue(MetaDataMember mm)
                {
                    if (current != original)
                    {
                        if (mm.IsAssociation && mm.Association.IsMany)
                        {
                            return mm.StorageAccessor.HasAssignedValue(original);
                        }
                        if (mm.StorageAccessor.HasValue(current))
                        {
                            if ((original != null) && mm.StorageAccessor.HasValue(original))
                            {
                                Debug.Assert(mm.Ordinal < dirtyMemberCache.Count);
                                if (dirtyMemberCache.Get(mm.Ordinal))
                                {
                                    return true;
                                }
                                object boxedValue = mm.MemberAccessor.GetBoxedValue(original);
                                return !Equals(mm.MemberAccessor.GetBoxedValue(current), boxedValue);
                            }
                            if (mm.IsDeferred && mm.StorageAccessor.HasAssignedValue(current))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                internal override bool HasChangedValues()
                {
                    if (current != original)
                    {
                        if (IsNew)
                        {
                            return true;
                        }
                        foreach (MetaDataMember member in type.PersistentDataMembers)
                        {
                            if (!member.IsAssociation && HasChangedValue(member))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                private bool HasDeferredLoader(MetaDataMember deferredMember)
                {
                    if (!deferredMember.IsDeferred)
                    {
                        return false;
                    }
                    MetaAccessor storageAccessor = deferredMember.StorageAccessor;
                    if (storageAccessor.HasAssignedValue(current) || storageAccessor.HasLoadedValue(current))
                    {
                        return false;
                    }
                    var boxedValue =
                        (IEnumerable)deferredMember.DeferredSourceAccessor.GetBoxedValue(current);
#if DEBUG
                    boxedValue = (IEnumerable)deferredMember.DeferredSourceAccessor.GetBoxedValue(current);
#endif
                    return (boxedValue != null);
                }

                private void InitializeDeferredLoader(MetaDataMember deferredMember)
                {
                    MetaAccessor storageAccessor = deferredMember.StorageAccessor;
                    if (!storageAccessor.HasAssignedValue(current) && !storageAccessor.HasLoadedValue(current))
                    {
                        var deferredSourceAccessor = deferredMember.DeferredSourceAccessor;
                        var boxedValue = (IEnumerable)deferredSourceAccessor.GetBoxedValue(current);
                        if (boxedValue == null)
                        {
                            boxedValue = tracker.services.GetDeferredSourceFactory(deferredMember)
                                                         .CreateDeferredSource(current);
                            deferredSourceAccessor.SetBoxedValue(ref current, boxedValue);
                        }
                        else if (!haveInitializedDeferredLoaders)
                        {
                            throw Error.CannotAttachAddNonNewEntities();
                        }
                    }
                }

                internal override void InitializeDeferredLoaders()
                {
                    if (tracker.services.Context.DeferredLoadingEnabled)
                    {
                        foreach (MetaAssociation association in Type.Associations)
                        {
                            if (!IsPendingGeneration(association.ThisKey))
                            {
                                InitializeDeferredLoader(association.ThisMember);
                            }
                        }
                        foreach (MetaDataMember member in
                                 Type.PersistentDataMembers.Where(p => p.IsDeferred && !p.IsAssociation))
                        {
                            if (!IsPendingGeneration(Type.IdentityMembers))
                            {
                                InitializeDeferredLoader(member);
                            }
                        }
                        haveInitializedDeferredLoaders = true;
                    }
                }

                internal override bool IsMemberPendingGeneration(MetaDataMember keyMember)
                {
                    if (IsNew && keyMember.IsDbGenerated)
                    {
                        return true;
                    }
                    foreach (MetaAssociation association in type.Associations)
                    {
                        if (!association.IsForeignKey)
                        {
                            continue;
                        }
                        int index = association.ThisKey.IndexOf(keyMember);
                        if (index > -1)
                        {
                            var boxedValue = association.ThisMember.IsDeferred
                                                    ? association.ThisMember.DeferredValueAccessor.GetBoxedValue(current)
                                                    : association.ThisMember.StorageAccessor.GetBoxedValue(current);
                            if ((boxedValue != null) && !association.IsMany)
                            {
                                var trackedObject = (StandardTrackedObject)tracker.GetTrackedObject(boxedValue);
                                if (trackedObject == null)
                                {
                                    continue;
                                }
                                var member = association.OtherKey[index];
                                return trackedObject.IsMemberPendingGeneration(member);
                            }
                        }
                    }
                    return false;
                }

                private bool IsModifiedMember(MetaDataMember member)
                {
                    Debug.Assert(member != null);
                    Debug.Assert(member.StorageAccessor != null);
                    if (member.IsAssociation || member.IsPrimaryKey || member.IsVersion || member.IsDbGenerated ||
                        !member.StorageAccessor.HasAssignedValue(current))
                    {
                        return false;
                    }
                    return ((state == State.Modified) ||
                            ((state == State.PossiblyModified) && HasChangedValue(member)));
                }

                internal override bool IsPendingGeneration(IEnumerable<MetaDataMember> key)
                {
                    if (IsNew)
                    {
                        foreach (MetaDataMember member in key)
                        {
                            if (IsMemberPendingGeneration(member))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                internal override void Refresh(RefreshMode mode, object freshInstance)
                {
                    SynchDependentData();
                    UpdateDirtyMemberCache();
                    Type c = freshInstance.GetType();
                    foreach (MetaDataMember member in type.PersistentDataMembers)
                    {
                        RefreshMode mode2 = member.IsDbGenerated ? RefreshMode.OverwriteCurrentValues : mode;
                        if (((mode2 != RefreshMode.KeepCurrentValues) && !member.IsAssociation) &&
                            ((Type.Type == c) || member.DeclaringType.Type.IsAssignableFrom(c)))
                        {
                            object boxedValue = member.StorageAccessor.GetBoxedValue(freshInstance);
                            RefreshMember(member, mode2, boxedValue);
                        }
                    }
                    original = CreateDataCopy(freshInstance);
                    if (mode == RefreshMode.OverwriteCurrentValues)
                    {
                        ResetDirtyMemberTracking();
                    }
                }

                internal override void RefreshMember(MetaDataMember mm, RefreshMode mode, object freshValue)
                {
                    if ((mode != RefreshMode.KeepCurrentValues) &&
                        (!HasChangedValue(mm) || (mode == RefreshMode.OverwriteCurrentValues)))
                    {
                        object boxedValue = mm.StorageAccessor.GetBoxedValue(current);
                        if (!Equals(freshValue, boxedValue))
                        {
                            mm.StorageAccessor.SetBoxedValue(ref current, freshValue);
                            foreach (MetaDataMember member in GetAssociationsForKey(mm))
                            {
                                if (!member.Association.IsMany)
                                {
                                    IEnumerable source =
                                        tracker.services.GetDeferredSourceFactory(member).CreateDeferredSource(
                                            current);
                                    if (member.StorageAccessor.HasValue(current))
                                    {
                                        AssignMember(current, member,
                                                          source.Cast<object>().SingleOrDefault());
                                    }
                                }
                            }
                        }
                    }
                }

                private void ResetDirtyMemberTracking()
                {
                    dirtyMemberCache.SetAll(false);
                }

                internal void StartTracking()
                {
                    if (original == current)
                    {
                        original = CreateDataCopy(current);
                    }
                }

                internal override void SynchDependentData()
                {
                    foreach (MetaAssociation association in Type.Associations)
                    {
                        MetaDataMember thisMember = association.ThisMember;
                        if (association.IsForeignKey)
                        {
                            bool flag = thisMember.StorageAccessor.HasAssignedValue(current);
                            bool flag2 = thisMember.StorageAccessor.HasLoadedValue(current);
                            if (flag || flag2)
                            {
                                object boxedValue = thisMember.StorageAccessor.GetBoxedValue(current);
                                if (boxedValue != null)
                                {
                                    int num = 0;
                                    int count = association.ThisKey.Count;
                                    while (num < count)
                                    {
                                        MetaDataMember member2 = association.ThisKey[num];
                                        MetaDataMember member3 = association.OtherKey[num];
                                        object obj3 = member3.StorageAccessor.GetBoxedValue(boxedValue);
                                        member2.StorageAccessor.SetBoxedValue(ref current, obj3);
                                        num++;
                                    }
                                    continue;
                                }
                                if (association.IsNullable)
                                {
                                    if (thisMember.IsDeferred ||
                                        ((original != null) &&
                                         (thisMember.MemberAccessor.GetBoxedValue(original) != null)))
                                    {
                                        int num3 = 0;
                                        int num4 = association.ThisKey.Count;
                                        while (num3 < num4)
                                        {
                                            MetaDataMember mm = association.ThisKey[num3];
                                            if (mm.CanBeNull)
                                            {
                                                if ((original != null) && HasChangedValue(mm))
                                                {
                                                    if (mm.StorageAccessor.GetBoxedValue(current) != null)
                                                    {
                                                        throw Error.InconsistentAssociationAndKeyChange(mm.Member.Name,
                                                                                                        thisMember.
                                                                                                            Member.Name);
                                                    }
                                                }
                                                else
                                                {
                                                    mm.StorageAccessor.SetBoxedValue(ref current, null);
                                                }
                                            }
                                            num3++;
                                        }
                                    }
                                    continue;
                                }
                                if (!flag2)
                                {
                                    var builder = new StringBuilder();
                                    foreach (MetaDataMember member5 in association.ThisKey)
                                    {
                                        if (builder.Length > 0)
                                        {
                                            builder.Append(", ");
                                        }
                                        builder.AppendFormat("{0}.{1}", Type.Name, member5.Name);
                                    }
                                    throw Error.CouldNotRemoveRelationshipBecauseOneSideCannotBeNull(
                                        association.OtherType.Name, Type.Name, builder);
                                }
                            }
                        }
                    }
                    if (type.HasInheritance)
                    {
                        if (original != null)
                        {
                            object discriminator = type.Discriminator.MemberAccessor.GetBoxedValue(current);
                            MetaType t = TypeFromDiscriminator(type, discriminator);
                            object obj5 = type.Discriminator.MemberAccessor.GetBoxedValue(original);
                            MetaType type2 = TypeFromDiscriminator(type, obj5);
                            if (t != type2)
                            {
                                throw Error.CannotChangeInheritanceType(obj5, discriminator,
                                                                        original.GetType().Name, t);
                            }
                        }
                        else
                        {
                            MetaType inheritanceType = type.GetInheritanceType(current.GetType());
                            if (inheritanceType.HasInheritanceCode)
                            {
                                object inheritanceCode = inheritanceType.InheritanceCode;
                                type.Discriminator.MemberAccessor.SetBoxedValue(ref current, inheritanceCode);
                            }
                        }
                    }
                }

                public override string ToString()
                {
                    return (type.Name + ":" + GetState());
                }

                private void UpdateDirtyMemberCache()
                {
                    foreach (MetaDataMember member in type.PersistentDataMembers)
                    {
                        if ((!member.IsAssociation || !member.Association.IsMany) &&
                            (!dirtyMemberCache.Get(member.Ordinal) && HasChangedValue(member)))
                        {
                            dirtyMemberCache.Set(member.Ordinal, true);
                        }
                    }
                }

                // Properties
                internal override object Current
                {
                    get { return current; }
                }

                internal override bool HasDeferredLoaders
                {
                    get
                    {
                        foreach (MetaAssociation association in Type.Associations)
                        {
                            if (HasDeferredLoader(association.ThisMember))
                            {
                                return true;
                            }
                        }
                        foreach (var member in
                                 Type.PersistentDataMembers.Where(p => p.IsDeferred && !p.IsAssociation))
                        {
                            if (HasDeferredLoader(member))
                                return true;
                        }
                        return false;
                    }
                }

                internal override bool IsDead
                {
                    get { return (state == State.Dead); }
                }

                internal override bool IsDeleted
                {
                    get { return (state == State.Deleted); }
                }

                internal override bool IsInteresting
                {
                    get
                    {
                        return (((((state == State.New) || (state == State.Deleted)) ||
                                  (state == State.Modified)) ||
                                 ((state == State.PossiblyModified) && (current != original))) ||
                                CanInferDelete());
                    }
                }

                internal override bool IsModified
                {
                    get
                    {
                        return (state == State.Modified ||
                                (state == State.PossiblyModified && current != original && HasChangedValues()));
                    }
                }

                internal override bool IsNew
                {
                    get { return (state == State.New); }
                }

                internal override bool IsPossiblyModified
                {
                    get
                    {
                        if (state != State.Modified)
                        {
                            return (state == State.PossiblyModified);
                        }
                        return true;
                    }
                }

                internal override bool IsRemoved
                {
                    get { return (state == State.Removed); }
                }

                internal override bool IsUnmodified
                {
                    get
                    {
                        if (state != State.PossiblyModified)
                        {
                            return false;
                        }
                        if (current != original)
                        {
                            return !HasChangedValues();
                        }
                        return true;
                    }
                }

                internal override bool IsWeaklyTracked
                {
                    get { return isWeaklyTracked; }
                }

                internal override object Original
                {
                    get { return original; }
                }

                internal override MetaType Type
                {
                    get { return type; }
                }

                // Nested Types

                private enum State
                {
                    New,
                    Deleted,
                    PossiblyModified,
                    Modified,
                    Removed,
                    Dead
                }
            }
        }
    }
}