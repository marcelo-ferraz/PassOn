using System.Reflection.Emit;
using System.Reflection;
using PassOn.Exceptions;

namespace PassOn.Engine.Extensions
{
    internal static class PropertyAssignmentILExtensions
    {
        internal static void EmitPropertyAssignment(this ILGenerator il, LocalBuilder resultLocal, MethodInfo sourceMap, PropertyInfo destProperty)
        {
            EmitPropertyAssignment(il, resultLocal, sourceMap, destProperty.GetSetMethod());
        }

        internal static void EmitPropertyAssignment(this ILGenerator il, LocalBuilder resultLocal, PropertyInfo srcProperty, PropertyInfo destProperty)
        {
            var tgtSetter = destProperty.GetSetMethod();

            // this property is the kind: this[int] 
            if (tgtSetter?.GetParameters().Length != 1) { return; }

            EmitPropertyAssignment(il, resultLocal, srcProperty.GetGetMethod(), tgtSetter);
        }

        internal static void EmitPropertyAssignment(this ILGenerator il, LocalBuilder resultLocal, PropertyInfo srcProperty, MethodInfo destMap)
        {
            il.Emit(OpCodes.Ldloc, resultLocal);
            il.Emit(OpCodes.Ldarg_0);

            var srcGetter = srcProperty.GetGetMethod();

            il.Emit(OpCodes.Call, srcGetter);

            var @params = destMap.GetParameters();

            if (@params.Length != 1)
            {
                throw new WrongArgCountCustomMapException(destMap, expected: 1);
            }

            if (@params.Length == 1 && @params[0].ParameterType == typeof(object))
            {
                il.Emit(OpCodes.Box, srcGetter.ReturnType);
            }

            il.Emit(OpCodes.Call, destMap);
        }
        
        internal static void EmitPropertyAssignment(this ILGenerator il, LocalBuilder resultLocal, MethodInfo sourceMap, MethodInfo destMap)
        {
            if (sourceMap == null || destMap == null) { return; }

            il.Emit(OpCodes.Ldloc, resultLocal);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, sourceMap);
            il.Emit(OpCodes.Call, destMap);
        }

    }
}
