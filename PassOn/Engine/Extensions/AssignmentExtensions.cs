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
    internal static class AssignmentExtensions
    {
        internal static void EmitPropertyPassing(this ILGenerator il, LocalBuilder cloneVariable, MethodInfo sourceMap, MethodInfo destMap)
        {
            il.Emit(OpCodes.Ldloc, cloneVariable);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, sourceMap);
            il.Emit(OpCodes.Call, destMap);
        }

        internal static void EmitPropertyPassing(this ILGenerator il, LocalBuilder cloneVariable, MethodInfo sourceMap, PropertyInfo destProperty)
        {
            EmitPropertyPassing(il, cloneVariable, sourceMap, destProperty.GetSetMethod());
        }

        internal static void EmitPropertyPassing(this ILGenerator il, LocalBuilder cloneVariable, PropertyInfo srcProperty, MethodInfo destMap)
        {
            il.Emit(OpCodes.Ldloc, cloneVariable);
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

        internal static void EmitPropertyPassing(this ILGenerator il, LocalBuilder cloneVariable, PropertyInfo srcProperty, PropertyInfo destProperty)
        {
            EmitPropertyPassing(il, cloneVariable, srcProperty.GetGetMethod(), destProperty.GetSetMethod());
        }   
    }
}
