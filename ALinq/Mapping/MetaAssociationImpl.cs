using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;

namespace ALinq.Mapping
{
   internal abstract class MetaAssociationImpl : MetaAssociation
{
    // Fields
    private static char[] keySeparators = new char[] { ',' };

    // Methods
    protected MetaAssociationImpl()
    {
    }

    protected static bool AreEqual(IEnumerable<MetaDataMember> key1, IEnumerable<MetaDataMember> key2)
    {
        using (IEnumerator<MetaDataMember> enumerator = key1.GetEnumerator())
        {
            using (IEnumerator<MetaDataMember> enumerator2 = key2.GetEnumerator())
            {
                bool flag = enumerator.MoveNext();
                bool flag2 = enumerator2.MoveNext();
                while (flag && flag2)
                {
                    if (enumerator.Current != enumerator2.Current)
                    {
                        return false;
                    }
                    flag = enumerator.MoveNext();
                    flag2 = enumerator2.MoveNext();
                }
                if (flag != flag2)
                {
                    return false;
                }
            }
        }
        return true;
    }

    protected static ReadOnlyCollection<MetaDataMember> MakeKeys(MetaType mtype, string keyFields)
    {
        string[] strArray = keyFields.Split(keySeparators);
        MetaDataMember[] collection = new MetaDataMember[strArray.Length];
        for (int i = 0; i < strArray.Length; i++)
        {
            strArray[i] = strArray[i].Trim();
            MemberInfo[] member = mtype.Type.GetMember(strArray[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if ((member == null) || (member.Length != 1))
            {
                throw Error.BadKeyMember(strArray[i], keyFields, mtype.Name);
            }
            collection[i] = mtype.GetDataMember(member[0]);
            if (collection[i] == null)
            {
                throw Error.BadKeyMember(strArray[i], keyFields, mtype.Name);
            }
        }
        return new List<MetaDataMember>(collection).AsReadOnly();
    }

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0} ->{1} {2}", new object[] { this.ThisMember.DeclaringType.Name, this.IsMany ? "*" : "", this.OtherType.Name });
    }
}

 


}