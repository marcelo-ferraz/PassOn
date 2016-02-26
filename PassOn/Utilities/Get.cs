using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PassOn.Utilities
{
    internal static class Get
    {
        internal static MethodInfo CorrectClonningMethod(Type source, Type destination)
        {
            if (!typeof(IEnumerable).IsAssignableFrom(source) &&
                !typeof(IEnumerable).IsAssignableFrom(destination) &&
                !destination.IsArray &&
                !source.IsArray)
            {
                return typeof(Pass)
                    .GetMethod("Onto", new Type[] { typeof(object) })
                    .MakeGenericMethod(destination);
            }

            Type srctType;
            Type destType;

            if (source.IsArray)
            { srctType = source.GetElementType(); }
            else if (source.IsGenericType)
            { srctType = source.GetGenericArguments()[0]; }
            else
            { throw new NotSupportedException("The type of the source object is not supported"); }

            if (destination.IsArray)
            { destType = destination.GetElementType(); }
            else if (destination.IsGenericType)
            { destType = destination.GetGenericArguments()[0]; }
            else
            { throw new NotSupportedException("The type of the source object is not supported"); }

            var cloneType = typeof(Pass.ACollectionOf<>)
                .MakeGenericType(srctType);

            return destination.IsArray ?
                cloneType.GetToArrayOfMethod(source, destType) :
                cloneType.GetToListOfMethod(source, destType);
        }

        internal static string[] AliasesForProperty(PropertyInfo prop)
        {
            var attributes =
                prop.GetCustomAttributes(typeof(CloneAttribute), true);

            return attributes != null && attributes.Length > 0 ?
                (attributes[0] as CloneAttribute).Aliases :
                null;
        }

        internal static bool CustomInfoFromProperties(PropertyInfo src, PropertyInfo dest, out string[] aliases, out Delegate customPassOn)
        {
            customPassOn = null;
            aliases = null;
            Func<PropertyInfo, CloneAttribute> getCloneInfo =
                (prop) =>
                {
                    var attrs =
                        prop.GetCustomAttributes(typeof(CloneAttribute), true);

                    return attrs != null && attrs.Length > 0 ?
                        (CloneAttribute)attrs[0] :
                        null;
                };

            var srcAttr = getCloneInfo(src);
            var destAttr = getCloneInfo(dest);

            if (destAttr == null && srcAttr == null) { return true; }

            // should ignore
            if((srcAttr != null && srcAttr.InspectionType == Inspection.Ignore) ||
                (destAttr != null && destAttr.InspectionType == Inspection.Ignore))
            {
                return false;
            }
            
            customPassOn =
                srcAttr != null ? srcAttr.CustomParsing :
                destAttr != null ? destAttr.CustomParsing :
                null;

            // getting those aliases
            int aliasesLength = 0;

            var destHasAliases =
                destAttr != null && destAttr.Aliases != null && destAttr.Aliases.Length > 0;

            var srcHasAliases =
                srcAttr != null && srcAttr.Aliases != null && srcAttr.Aliases.Length > 0;

            aliasesLength =
                (srcHasAliases ? srcAttr.Aliases.Length : 0) +
                (destHasAliases ? destAttr.Aliases.Length : 0);
                        
            aliases = aliasesLength > 0 ?
                (string[])Array.CreateInstance(typeof(string), aliasesLength) :
                null;

            if (srcHasAliases)
            { Array.Copy(srcAttr.Aliases, aliases, srcAttr.Aliases.Length); }

            if (destHasAliases)
            {
                Array.Copy(
                    destAttr.Aliases,
                    0,
                    aliases,
                    srcHasAliases ? srcAttr.Aliases.Length : 0,
                    destAttr.Aliases.Length);
            }

            return true;
        }

        internal static PropertyInfo[] PropertiesFrom(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Returns the type of cloning to apply on a certain srcProperty when in custom mode.
        /// Otherwise the main cloning method is returned.
        /// You can invoke custom mode by invoking the method Clone(T obj)
        /// </summary>
        /// <param name="srcProperty">Property to examine</param>
        /// <returns>Type of cloning to use for this srcProperty.</returns>
        internal static Inspection CloneTypeForProperty(PropertyInfo prop)
        {
            var attributes =
                prop.GetCustomAttributes(typeof(CloneAttribute), true);

            return attributes != null && attributes.Length > 0 ?
                (attributes[0] as CloneAttribute).InspectionType :
                Inspection.Deep;
        }
    }
}
