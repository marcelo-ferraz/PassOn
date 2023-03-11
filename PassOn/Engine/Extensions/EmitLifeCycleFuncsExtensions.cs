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
            var sourceBeforeFunc = typeof(Source)
                .GetMethods()
                .FirstOrDefault(m => m.GetCustomAttribute<BeforeMappingAttribute>() != null);

            if (sourceBeforeFunc != null)
            { EmitSourceFunc<Source, Target>(il, sourceBeforeFunc); }

            var targetBeforeFunc = typeof(Target)
                .GetMethods()
                .FirstOrDefault(m => m.GetCustomAttribute<BeforeMappingAttribute>() != null);

            if (targetBeforeFunc != null)
            { EmitTargetFunc<Source, Target>(il, targetBeforeFunc); }
        }

        internal static void TryEmitAfterFuncs<Source, Target>(this ILGenerator il)
        {
            var sourceAfterFunc = typeof(Source)
                .GetMethods()
                .FirstOrDefault(m => m.GetCustomAttribute<AfterMappingAttribute>() != null);

            if (sourceAfterFunc != null)
            { EmitSourceFunc<Source, Target>(il, sourceAfterFunc); }

            var targetAfterFunc = typeof(Target)
                .GetMethods()
                .FirstOrDefault(m => m.GetCustomAttribute<AfterMappingAttribute>() != null);

            if (targetAfterFunc != null)
            { EmitTargetFunc<Source, Target>(il, targetAfterFunc); }
        }

        private static void EmitTargetFunc<Source, Target>(ILGenerator il, MethodInfo func)
        {
            il.Emit(OpCodes.Ldloc_0);
            EmitFuncCall<Source, Target>(il, func);
        }

        private static void EmitSourceFunc<Source, Target>(ILGenerator il, MethodInfo func)
        {
            il.Emit(OpCodes.Ldarg_0);
            EmitFuncCall<Source, Target>(il, func);
        }

        private static void EmitFuncCall<Source, Target>(ILGenerator il, MethodInfo func)
        {
            var args = func.GetParameters();

            if (args.Length > 2)
            { throw new InvalidArgCountLifeCycleFunction(func); }

            var srcWasEmitted = false;
            var srcAttrWasFound = false;
            var tgtWasEmitted = false;
            var tgtAttrWasFound = false;

            foreach (var arg in args) {
                var argIsObject = arg.ParameterType == typeof(object);

                if (arg.ParameterType.IsValueType) {
                    throw new ValueTypeArgLifeCycleFunction(arg, func);
                }

                var isAssignableToSrc = arg
                    .ParameterType
                    .IsAssignableFrom(typeof(Source)) && !argIsObject;

                var isAssignableToTgt = arg
                    .ParameterType
                    .IsAssignableFrom(typeof(Target)) && !argIsObject;

                var hasSrcAttr = arg                    
                    .GetCustomAttribute<SourceAttribute>() != null;

                var hasTgtAttr = arg                    
                    .GetCustomAttribute<TargetAttribute>() != null;

                var isAssignableToBoth = isAssignableToSrc && isAssignableToTgt;

                if (hasSrcAttr && srcAttrWasFound)
                { throw new BothArgumentsAreSourcesException(func); }
                else { srcAttrWasFound = true; }

                if (hasTgtAttr && tgtAttrWasFound)
                { throw new BothArgumentsAreTargetsException(func); }
                else { tgtAttrWasFound = hasTgtAttr; }

                // is ambiguous when has both attributes
                var isAmbiguous = (hasSrcAttr && hasTgtAttr);

                // or when it has neither and either is assignable to both or is an object
                isAmbiguous |= !hasSrcAttr && !hasTgtAttr && (
                    isAssignableToBoth || argIsObject                        
                );
                    
                if (isAmbiguous)
                { throw new AmbiguousArgumentMatchException(func); }

                if (
                    (isAssignableToBoth && hasSrcAttr)
                    || isAssignableToSrc
                    || (hasSrcAttr && argIsObject)                    
                ) {
                    if (srcWasEmitted)
                    { throw new BothArgumentsAreSourcesException(func); }

                    il.Emit(OpCodes.Ldarg_0);
                    srcWasEmitted = true; 
                }
                else if (
                    (isAssignableToBoth && hasTgtAttr)
                    || isAssignableToTgt
                    || (hasTgtAttr && argIsObject))
                {
                    if (tgtWasEmitted)
                    { throw new BothArgumentsAreTargetsException(func); }

                    il.Emit(OpCodes.Ldloc_0);
                    tgtWasEmitted= true;
                }
                else
                {
                    il.Emit(OpCodes.Ldnull);                    
                }
            }             
            

            il.Emit(OpCodes.Callvirt, func);
            il.Emit(OpCodes.Nop);

            if (func.ReturnType == typeof(void)) { return; }

            if (func.ReturnType == typeof(object))
            { il.Emit(OpCodes.Castclass, typeof(Target)); }

            if (func.ReturnType.IsAssignableFrom(typeof(Target)))
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
