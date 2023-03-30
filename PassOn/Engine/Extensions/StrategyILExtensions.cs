using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PassOn.Utilities;

namespace PassOn.Engine.Extensions
{
    internal static class StrategyILExtensions
    {
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
                var mapInfo = MapStrategyAttribute
                    .GetMapperInfo<Source>(srcStrategy.Mapper, src.Name);

                il.EmitPropertyAssignment(cloneVariable, mapInfo, tgt);
                return true;
            }

            if (tgtInspectionType == Strategy.CustomMap)
            {
                var mapInfo = MapStrategyAttribute
                    .GetMapperInfo<Target>(tgtStrategy.Mapper, tgt.Name);

                il.EmitPropertyAssignment(cloneVariable, src, mapInfo);
                return true;
            }

            if (tgt.PropertyType.IsAssignableFrom(src.PropertyType)
                && (
                    srcInspectionType == Strategy.Shallow
                    || src.PropertyType.IsValueType
                    || src.PropertyType == typeof(string)
            ))
            {
                il.EmitPropertyAssignment(cloneVariable, src, tgt);
                return true;
            }

            return false;
        }

    }
}
