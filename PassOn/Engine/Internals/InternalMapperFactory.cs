using System;
using System.Collections;
using System.Reflection;

namespace PassOn.Engine.Internals
{
    internal class InternalMapperFactory
    {
        internal static (MethodInfo, bool) GetMapper(PropertyInfo source, PropertyInfo target)
        {
            return GetMapper(source.PropertyType, target.PropertyType);
        }

        internal static (MethodInfo, bool) GetMapper(Type source, Type target, bool rawClone = false)
        {
            var srcTypeIsIEnumerable = typeof(IEnumerable).IsAssignableFrom(source);
            var tgtTypeIsIEnumerable = typeof(IEnumerable).IsAssignableFrom(target);

            if (!srcTypeIsIEnumerable && !tgtTypeIsIEnumerable)
            {
                var name = rawClone
                    ? "MapObjectRawWithILDeepInternal"
                    : "MapObjectWithILDeepInternal";

                var objMapper = typeof(InternalRefMappers)
                    .GetMethod(name)
                    .MakeGenericMethod(source, target);
                return (objMapper, true);
            }

            var srcItemType = source.IsArray
                ? source.GetElementType()
                : source.GetGenericArguments()[0];

            var tgtItemType = target.IsArray
                ? target.GetElementType()
                : target.GetGenericArguments()[0];

            MethodInfo mapper = null;

            var mappersHolderType =
                (srcItemType.IsValueType || tgtItemType.IsValueType)
                ? typeof(InternalValueMappers)
                : typeof(InternalRefMappers);

            if (srcTypeIsIEnumerable && target.IsArray)
            {
                mapper = mappersHolderType
                    .GetMethod("MapIEnumerableToArray")
                    .MakeGenericMethod(srcItemType, tgtItemType);
            }
            else if (tgtTypeIsIEnumerable && source.IsArray)
            {
                mapper = mappersHolderType
                    .GetMethod("MapArrayToIEnumerable")
                    .MakeGenericMethod(srcItemType, tgtItemType);
            }
            else if (source.IsArray && target.IsArray)
            {
                mapper = mappersHolderType
                    .GetMethod("MapIEnumerableToArray")
                    .MakeGenericMethod(srcItemType, tgtItemType);
            }
            else if (srcTypeIsIEnumerable && tgtTypeIsIEnumerable)
            {
                mapper = mappersHolderType
                    .GetMethod("MapIEnumerableToList")
                    .MakeGenericMethod(srcItemType, tgtItemType);
            }

            if (mapper == null)
            {
                throw new NotSupportedException(
                    $"This mapping \"{source.Name}\" <-> \"{target.Name}\" is not supported!"
                );
            }

            return (mapper, false);
        }
    }
}
