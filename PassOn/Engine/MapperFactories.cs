using PassOn.Engine.Extensions;
using PassOn.Engine.Internals;
using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace PassOn.Engine
{
    public static class MapperFactories
    {
        public static int MAX_RECURSION = 64;
        private static string OVERFLOW_MESSAGE = "There might be a cyclical dependency on one of your models. Please review your code and try again";

        public static Func<Source, Target, MapperEngine, int, Target> CreateMerger<Source, Target>()
        {
            Delegate mapper = null;

            var dynMethod = new DynamicMethod(
                "DoDeepMerge",
                typeof(Target),
                new Type[] { typeof(Source), typeof(Target), typeof(MapperEngine), typeof(int) },
                Assembly.GetExecutingAssembly().ManifestModule,
                true);

            var il = dynMethod.GetILGenerator();

            var resultLocal = il.DeclareLocal(typeof(Target));
            EmitMapTargetToResult<Target>(il, resultLocal);

            il.TryEmitBeforeFuncs<Source, Target>();

            if (typeof(IEnumerable).IsAssignableFrom(typeof(Target)))
            {
                var (map, rec) = InternalMapperFactory.GetMerger(typeof(Source), typeof(Target));
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2); // engine                                          
                il.Emit(OpCodes.Ldarg_3); // recursionIndex
                il.Emit(OpCodes.Call, map);
                il.Emit(OpCodes.Stloc, resultLocal);
            }

            PropertyMatcher.WithStrategy<Source, Target>(
                (src, tgt) =>
                {
                    var srcStrategyType = src.GetStrategyType();

                    var emitted = il.TryEmitByStrategy<Source, Target>(resultLocal, src, tgt);

                    if (emitted) { return; }

                    if (tgt.PropertyType.IsAssignableFrom(src.PropertyType)
                        && (
                            srcStrategyType == Strategy.Shallow
                            || src.PropertyType.IsValueType
                            || src.PropertyType == typeof(string)
                    ))
                    {
                        il.EmitPropertyAssignment(resultLocal, src, tgt);
                        return;
                    }

                    //Inspection.Deep
                    if (src.PropertyType.IsClass)
                    {
                        EmitReferenceTypeCopy(il, dynMethod, resultLocal, src, tgt, isForMerging: true);
                        return;
                    }
                }, ignoreType: true);

            il.TryEmitAfterFuncs<Source, Target>();

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            var delType = typeof(Func<,,,,>)
                .MakeGenericType(typeof(Source), typeof(Target), typeof(MapperEngine), typeof(int), typeof(Target));

            mapper = dynMethod.CreateDelegate(delType);

            return (Func<Source, Target, MapperEngine, int, Target>)mapper;
        }

        public static Func<Source, MapperEngine, int, Target> CreateMapper<Source, Target>()
        {
            Delegate mapper = null;

            // Create ILGenerator            
            var dynMethod = new DynamicMethod(
                "DoDeepMap",
                typeof(Target),
                new Type[] { typeof(Source), typeof(MapperEngine), typeof(int) },
                Assembly.GetExecutingAssembly().ManifestModule,
                true);

            var il =
                dynMethod.GetILGenerator();

            var resultLocal =
                il.DeclareLocal(typeof(Target));

            il.Construct<Target>(resultLocal);

            EmitStackOverflowCheck(il, dynMethod);

            il.TryEmitBeforeFuncs<Source, Target>();
            
            if (typeof(IEnumerable).IsAssignableFrom(typeof(Target)))
            {
                var (map, rec) = InternalMapperFactory.GetMapper(typeof(Source), typeof(Target));
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1); // engine
                il.Emit(OpCodes.Ldarg_2); // recursionIndex                                          
                il.Emit(OpCodes.Call, map);
                il.Emit(OpCodes.Stloc, resultLocal);
            }

            PropertyMatcher.WithStrategy<Source, Target>((src, tgt) =>
            {
                var strategyType = src.GetStrategyType();

                var emitted = il.TryEmitByStrategy<Source, Target>(
                    resultLocal, src, tgt
                );

                if (emitted) { return; }

                if (tgt.PropertyType.IsAssignableFrom(src.PropertyType)
                    && (
                        strategyType == Strategy.Shallow
                        || src.PropertyType.IsValueType
                        || src.PropertyType == typeof(string)
                        || src.PropertyType.IsSubclassOf(typeof(Delegate))
                ))
                {
                    il.EmitPropertyAssignment(resultLocal, src, tgt);
                    return;
                }

                //Inspection.Deep
                if (src.PropertyType.IsClass)
                {
                    EmitReferenceTypeCopy(il, dynMethod, resultLocal, src, tgt);
                    return;
                }
            }, ignoreType: true);

            il.TryEmitAfterFuncs<Source, Target>();

            il.Emit(OpCodes.Ldloc, resultLocal);
            il.Emit(OpCodes.Ret);

            var delType = typeof(Func<,,,>)
                .MakeGenericType(typeof(Source), typeof(MapperEngine), typeof(int), typeof(Target));

            mapper = dynMethod.CreateDelegate(delType);

            return (Func<Source, MapperEngine, int, Target>)mapper;
        }

        internal static Func<Source, MapperEngine, int, Target> CreateRawMapper<Source, Target>()
        {
            Delegate mapper = null;

            // Create ILGenerator            
            var dynMethod = new DynamicMethod(
                "DoDeepRawMap",
                typeof(Target),
                new Type[] { typeof(Source), typeof(MapperEngine), typeof(int) },
                Assembly.GetExecutingAssembly().ManifestModule,
                true);

            var il =
                dynMethod.GetILGenerator();

            var resultLocal =
                il.DeclareLocal(typeof(Target));

            il.Construct<Target>(resultLocal);

            EmitStackOverflowCheck(il, dynMethod);

            PropertyMatcher.WithoutStrategy<Source, Target>((src, tgt) =>
            {
                if (tgt.PropertyType.IsAssignableFrom(src.PropertyType)
                    && (
                        src.PropertyType.IsValueType
                        || src.PropertyType == typeof(string)
                    )
                )
                {
                    il.EmitPropertyAssignment(resultLocal, src, tgt);
                    return;
                }

                //Inspection.Deep
                if (src.PropertyType.IsClass)
                {
                    EmitReferenceTypeCopy(il, dynMethod, resultLocal, src, tgt);
                    return;
                }
            }, ignoreType: true);


            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            var delType = typeof(Func<,,,>)
                .MakeGenericType(typeof(Source), typeof(MapperEngine), typeof(int), typeof(Target));

            mapper = dynMethod.CreateDelegate(delType);

            return (Func<Source, MapperEngine, int, Target>)mapper;
        }

        internal static void EmitMapTargetToResult<Target>(ILGenerator il, LocalBuilder resultLocal)
        {
            var endOfInitialMapLabel = il.DefineLabel();
            var valueIsNotNullLabel = il.DefineLabel();

            (
                MethodInfo mapTarget,
                bool isRecursive
            ) = InternalMapperFactory.GetMapper(
                typeof(Target), typeof(Target), rawClone: true);

            
            il.Emit(OpCodes.Nop);
            
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Brfalse_S, valueIsNotNullLabel);

            // if the target value is null, create a new one
            il.Construct<Target>(resultLocal);
            // go to the end of the function
            il.Emit(OpCodes.Br_S, endOfInitialMapLabel);

            // if the target has a value, copy
            il.MarkLabel(valueIsNotNullLabel);
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_1); // target (merging)
            il.Emit(OpCodes.Ldarg_2); // engine
            il.Emit(OpCodes.Ldarg_3); // recursionIndex
            il.Emit(OpCodes.Call, mapTarget);
            il.Emit(OpCodes.Stloc, resultLocal);
            il.Emit(OpCodes.Nop);

            il.MarkLabel(endOfInitialMapLabel);
        }

        internal static void EmitReferenceTypeCopy(
            ILGenerator il,
            DynamicMethod dynMethod,
            LocalBuilder resultLocal,
            PropertyInfo source,
            PropertyInfo target,
            bool isForMerging = false)
        {
            // don't copy a delegate
            if (source.PropertyType.IsSubclassOf(typeof(Delegate))) { return; }

            var srcType = source.PropertyType;
            var tgtType = target.PropertyType;

            var srcGetter = source.GetGetMethod();
            var tgtGetter = target.GetGetMethod();
            var tgtSetter = target.GetSetMethod();
            if (srcGetter == null || tgtGetter == null || tgtSetter?.GetParameters().Length != 1) {
                Debugger.Break();
                return;
            }

            var srcTypeIsIEnumerable =
                typeof(IEnumerable).IsAssignableFrom(srcType);
            var tgtTypeIsIEnumerable =
                typeof(IEnumerable).IsAssignableFrom(tgtType);

            var isMergeable = isForMerging;

            if (isForMerging && (srcTypeIsIEnumerable || tgtTypeIsIEnumerable)) {
                var (srcItemType, tgtItemType) =
                    (srcType, tgtType).GetCollectionItemTypes();
                isMergeable = !srcItemType.IsValueType && !tgtItemType.IsValueType;
            }

            (
                MethodInfo internalMapFunc,
                bool isRecursive
            ) = isForMerging 
                ? InternalMapperFactory.GetMerger(source, target)
                : InternalMapperFactory.GetMapper(source, target);

            var args = dynMethod.GetParameters();
            var recursionIndexArgIndex = args.Length - 1;
            var engineArgIndex = args.Length - 2;

            // check if src is null
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, srcGetter);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Cgt_Un);

            var valueIsNullLabel = il.DefineLabel();
            il.Emit(OpCodes.Brfalse_S, valueIsNullLabel);
            il.Emit(OpCodes.Nop);
            // check if src is null

            il.Emit(OpCodes.Ldloc, resultLocal);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, srcGetter);

            if (isMergeable)
            {
                il.Emit(OpCodes.Ldloc, resultLocal);
                il.Emit(OpCodes.Callvirt, tgtGetter);
            }

            il.Emit(OpCodes.Ldarg, engineArgIndex); // engine
            il.Emit(OpCodes.Ldarg, recursionIndexArgIndex);

            if (isRecursive)
            {
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Add); // recursionIndex + 1
            }

            il.Emit(OpCodes.Call, internalMapFunc);
            il.Emit(OpCodes.Callvirt, tgtSetter);
            il.Emit(OpCodes.Nop);

            il.MarkLabel(valueIsNullLabel);
        }

        internal static void EmitStackOverflowCheck(ILGenerator il, DynamicMethod dynMethod)
        {
            var args = dynMethod.GetParameters();
            var recursionIndexArgIndex = args.Length - 1;
            var recursionNotTooDeep = il.DefineLabel();
            var exceptionCtr = typeof(StackOverflowException)
                .GetConstructor(
                    new Type[] { typeof(string) }
                );

            il.Emit(OpCodes.Ldarg, recursionIndexArgIndex);
            il.Emit(OpCodes.Ldc_I4_S, MAX_RECURSION);
            il.Emit(OpCodes.Cgt);
            il.Emit(OpCodes.Brfalse, recursionNotTooDeep);

            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldstr, OVERFLOW_MESSAGE);
            il.Emit(OpCodes.Newobj, exceptionCtr);
            il.Emit(OpCodes.Throw);

            il.MarkLabel(recursionNotTooDeep);
        }
    }
}
