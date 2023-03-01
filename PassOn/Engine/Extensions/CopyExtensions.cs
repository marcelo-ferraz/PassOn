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
    internal static class CopyExtensions
    {
        internal static void EmitReferenceTypeCopy(this ILGenerator il, LocalBuilder cloneVar, PropertyInfo source, PropertyInfo target)
        {
            // does not copy a delegate
            if (source.PropertyType.IsSubclassOf(typeof(Delegate))) { return; }

            il.Emit(OpCodes.Ldloc, cloneVar);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, source.GetGetMethod());
            il.Emit(OpCodes.Call,
                GetCorrectClonningMethod(source.PropertyType, target.PropertyType));
            il.Emit(OpCodes.Call, target.GetSetMethod());
        }

        private static MethodInfo GetCorrectClonningMethod(Type source, Type target)
        {
            if (!typeof(IEnumerable).IsAssignableFrom(source) &&
                !typeof(IEnumerable).IsAssignableFrom(target) &&
                !target.IsArray &&
                !source.IsArray)
            {
                return typeof(Pass)
                    .GetMethod("On", new Type[] { typeof(object) })
                    .MakeGenericMethod(target);
            }

            var srcItem =
                source.IsArray ? source.GetElementType() :
                source.IsGenericType ? source.GetGenericArguments()[0] :
                null;

            var destType =
                target.IsArray ? target.GetElementType() :
                target.IsGenericType ? target.GetGenericArguments()[0] :
                null;

            if (srcItem == null || destType == null)
            {
                throw new NotSupportedException();
            }

            var cloneType = typeof(Pass.ACollectionOf<>)
                .MakeGenericType(srcItem);

            return target.IsArray ?
                cloneType.GetToArrayOfMethod(source, destType) :
                cloneType.GetToListOfMethod(source, destType);
        }
    }
}
