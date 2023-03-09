using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Engine.Extensions
{
    internal static class EmitLifeCycleFuncsExtensions
    {
        internal static void TryEmitBeforeFuncs<Source, Target>(this ILGenerator il)
        {
            var sourceBeforeFunc = typeof(Source).GetMethod("Before");

            if (sourceBeforeFunc != null)
            { EmitSourceFunc<Source, Target>(il, sourceBeforeFunc); }

            var targetBeforeFunc = typeof(Target).GetMethod("Before");

            if (targetBeforeFunc != null)
            { EmitTargetFunc<Target>(il, targetBeforeFunc); }
        }

        internal static void TryEmitAfterFuncs<Source, Target>(this ILGenerator il)
        {
            var sourceAfterFunc = typeof(Source).GetMethod("After");

            if (sourceAfterFunc != null)
            { EmitSourceFunc<Source, Target>(il, sourceAfterFunc); }

            var targetAfterFunc = typeof(Target).GetMethod("After");

            if (targetAfterFunc != null)
            { EmitTargetFunc<Target>(il, targetAfterFunc); }
        }

        private static void EmitTargetFunc<T>(ILGenerator il, MethodInfo func)
        {
            il.Emit(OpCodes.Ldloc_0);
            EmitFuncCall<T>(il, func);
        }

        private static void EmitSourceFunc<Source, Target>(ILGenerator il, MethodInfo func)
        {
            il.Emit(OpCodes.Ldarg_0);
            EmitFuncCall<Target>(il, func);
        }

        private static void EmitFuncCall<T>(ILGenerator il, MethodInfo func)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Callvirt, func);
            il.Emit(OpCodes.Nop);

            if (func.ReturnType == typeof(void)) { return; }

            if (func.ReturnType == typeof(object))
            { il.Emit(OpCodes.Castclass, typeof(T)); }

            if (func.ReturnType.IsAssignableFrom(typeof(T)))
            {
                il.Emit(OpCodes.Stloc_0);
            }
            else
            {
                il.Emit(OpCodes.Pop);
            }
        }
    }
}
