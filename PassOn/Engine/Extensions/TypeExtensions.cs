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
    internal static class TypeExtensions
    {
        internal static void Construct<T>(this ILGenerator il)
        {
            var type = typeof(T);
            var cInfo = type.GetConstructor(Type.EmptyTypes);

            if (cInfo != null)
            {
                il.Emit(OpCodes.Newobj, cInfo);
                return;
            }

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call,
                type.GetMethod("GetType"));
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Call,
                ((Func<Type, object>)FormatterServices.GetSafeUninitializedObject).Method);
            il.Emit(OpCodes.Castclass, type);
        }
    }
}
