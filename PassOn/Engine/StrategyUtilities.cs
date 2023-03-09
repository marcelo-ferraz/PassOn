using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.EngineExtensions
{
    internal static class StrategyUtilities
    {
        public static MethodInfo GetMapperInfo<T>(
            string mapperName,
            string propName
        )
        {
            var propMapName = !string.IsNullOrEmpty(mapperName)
                ? mapperName
                : $"Map{propName}";

            var propMapInfo = typeof(T).GetMethod(propMapName);

            if (propMapInfo == null)
            { throw new CustomMapNoMatchException<T>(propMapName, propName); }

            if (propMapInfo.IsStatic)
            { throw new StaticCustomMapFoundException<T>(propMapName, propName); }

            return propMapInfo;
        }

        internal static bool TryEmitByStrategy<Source, Target>(this ILGenerator il, LocalBuilder cloneVariable, PropertyInfo src, PropertyInfo tgt)
        {
            var srcStrategy = src.GetCloneStrategy();
            var tgtStrategy = tgt.GetCloneStrategy();

            if (srcStrategy == null && tgtStrategy == null)
            {
                return false;
            }

            var srcInspectionType = srcStrategy?.Type ?? MapStrategyAttribute.DEFAULT_TYPE;
            var tgtInspectionType = tgtStrategy?.Type ?? MapStrategyAttribute.DEFAULT_TYPE;


            if (srcInspectionType == Strategy.Ignore
                || tgtInspectionType == Strategy.Ignore)
            { return true; }

            if (srcInspectionType == Strategy.CustomMap)
            {
                var mapInfo = GetMapperInfo<Source>(srcStrategy.Mapper, src.Name);
                il.EmitPropertyPassing(cloneVariable, mapInfo, tgt);
                return true;
            }

            if (tgtInspectionType == Strategy.CustomMap)
            {
                var mapInfo = GetMapperInfo<Target>(tgtStrategy.Mapper, tgt.Name);
                il.EmitPropertyPassing(cloneVariable, src, mapInfo);
                return true;
            }

            if (tgt.PropertyType.IsAssignableFrom(src.PropertyType)
                && (
                    srcInspectionType == Strategy.Shallow
                    || src.PropertyType.IsValueType
                    || src.PropertyType == typeof(string)
            ))
            {
                il.EmitPropertyPassing(cloneVariable, src, tgt);
                return true;
            }

            return false;
        }
    }
}
