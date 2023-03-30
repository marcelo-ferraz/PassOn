using PassOn.Engine.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
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

            string result = GetLabel(target);
            string mapperName = $"MapIEnumerableTo{result}";

            var (srcItemType, tgtItemType) =
                (source, target).GetCollectionItemTypes();

            var mappersHolderType =
                (srcItemType.IsValueType || tgtItemType.IsValueType)
                ? typeof(InternalValueMappers)
                : typeof(InternalRefMappers);

            var mapper = mappersHolderType
                   .GetMethod(mapperName);

            if (mapper == null)
            {
                throw new NotSupportedException(
                    $"This mapping \"{source.Name}\" <-> \"{target.Name}\" is not supported!"
                );
            }

            mapper = mapper.MakeGenericMethod(srcItemType, tgtItemType);

            return (mapper, false);
        }

        internal static (MethodInfo, bool) GetMerger(PropertyInfo source, PropertyInfo target)
        {
            return GetMerger(source.PropertyType, target.PropertyType);
        }

        internal static (MethodInfo, bool) GetMerger(Type source, Type target)
        {
            var srcTypeIsIEnumerable = typeof(IEnumerable).IsAssignableFrom(source);
            var tgtTypeIsIEnumerable = typeof(IEnumerable).IsAssignableFrom(target);

            if (!srcTypeIsIEnumerable && !tgtTypeIsIEnumerable)
            {
                var objMapper = typeof(InternalRefMergers)
                    .GetMethod("MergeObjectWithILDeepInternal")
                    .MakeGenericMethod(source, target);
                return (objMapper, true);
            }

            var (srcItemType, tgtItemType) =
                (source, target).GetCollectionItemTypes();

            var itemIsValueType = (srcItemType.IsValueType || tgtItemType.IsValueType);

            var mappersHolderType = itemIsValueType
                ? typeof(InternalValueMappers)
                : typeof(InternalRefMergers);

            var operation = itemIsValueType ? "Map" : "Merge";
            string result = GetLabel(target);

            if (target.IsArray && typeof(IList).IsAssignableFrom(source)) { }

            var mergerName = $"{operation}IEnumerableTo{result}";

            var merger = mappersHolderType.GetMethod(mergerName);

            if (merger == null)
            {
                throw new NotSupportedException(
                    $"This merging \"{source.Name}\" <-> \"{target.Name}\" is not supported!"
                );
            }

            merger = merger.MakeGenericMethod(srcItemType, tgtItemType);

            return (merger, false);
        }

        private static string GetLabel(Type source)
        {
            if (source.IsArray)
            {
                return "Array";
            }
            
            if (
                typeof(IList).IsAssignableFrom(source) 
                || (
                    source.IsGenericType 
                    && typeof(IList<>).IsAssignableFrom(source.GetGenericTypeDefinition())
                )
            )
            {
                return "IList";
            }

            else if (typeof(IEnumerable).IsAssignableFrom(source))
            {
                return "IEnumerable";
            }

            return null;
        }
    }
}
