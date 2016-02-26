using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace PassOn.Utilities
{
    public static class Copy
    {
        internal static void Properties(Type returnType, Type sourceType, ILGenerator il, Action<PropertyInfo, PropertyInfo, bool, int> whenSame, bool ignoreType)
        {
            List<Delegate> parsers = null;
            Properties(returnType, sourceType, il, whenSame, ignoreType, out parsers);
        }

        internal static void Properties(Type returnType, Type sourceType, ILGenerator il, Action<PropertyInfo, PropertyInfo, bool, int> whenSame, bool ignoreType, out List<Delegate> parsers)
        {
            var destPropertys =
                Get.PropertiesFrom(returnType);

            int parserIndex = -1;
            parsers = new List<Delegate>();

            foreach (var srcProperty in Get.PropertiesFrom(sourceType))
            {
                foreach (var destProperty in destPropertys)
                {
                    Func<bool> isAssignable =
                        () =>
                            destProperty.PropertyType.IsAssignableFrom(srcProperty.PropertyType) ||
                            srcProperty.PropertyType.IsAssignableFrom(destProperty.PropertyType);

                    string[] aliases = null;
                    Delegate customParser = null;

                    var ignore = !Get.CustomInfoFromProperties(
                        srcProperty,
                        destProperty,
                        out aliases,
                        out customParser);

                    if (ignore) { continue; }

                    if (customParser != null)
                    {
                        parsers.Add(customParser);
                        parserIndex++;
                    }

                    var namesRelate =
                        destProperty.Name.Equals(srcProperty.Name) ||
                        (aliases != null && Array.IndexOf(aliases, destProperty.Name, 0) > -1);

                    if ((ignoreType || isAssignable()) && namesRelate)
                    {
                        whenSame(srcProperty, destProperty, customParser != null, parserIndex);
                    }
                }
            }
        }

        internal static void PropertyDeeply(PropertyInfo src, PropertyInfo dest, bool hasParsing, int index, ILGenerator il, LocalBuilder cloneVariable)
        {
            var shouldPassOn =
                dest.PropertyType.IsAssignableFrom(src.PropertyType)
                &&
                (Get.CloneTypeForProperty(src) == Inspection.Shallow || src.PropertyType.IsValueType || src.PropertyType == typeof(string));

            if (Get.CloneTypeForProperty(src) == Inspection.Custom && hasParsing)
            {
                il.EmitCopyWithCustomParser(cloneVariable, src, dest, index);
            }
            else if (shouldPassOn)
            {
                il.EmitDefaultCopy(cloneVariable, src, dest);
            }//Inspection.Deep
            else if (src.PropertyType.IsClass)
            {
                il.EmitCopyRefType(cloneVariable, src, dest);
            }
        }
    }
}
