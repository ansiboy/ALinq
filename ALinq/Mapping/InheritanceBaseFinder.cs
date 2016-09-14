using System;

namespace ALinq.Mapping
{
    internal static class InheritanceBaseFinder
    {
        // Methods
        internal static MetaType FindBase(MetaType derivedType)
        {
            if (derivedType.Type == typeof(object))
            {
                return null;
            }
            Type baseType = derivedType.Type;
            Type type2 = derivedType.InheritanceRoot.Type;
            MetaTable table = derivedType.Table;
            MetaType inheritanceType = null;
            do
            {
                if ((baseType == typeof(object)) || (baseType == type2))
                {
                    return null;
                }
                baseType = baseType.BaseType;
                inheritanceType = derivedType.InheritanceRoot.GetInheritanceType(baseType);
            }
            while (inheritanceType == null);
            return inheritanceType;
        }
    }
}