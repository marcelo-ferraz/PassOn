using PassOn.Engine.Extensions;
using PassOn.Engine.Internals;
using PassOn.EngineExtensions;
using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace PassOn
{
    public static class MapperFactories
    {
        public static int MAX_RECURSION = 64;
        private static string OVERFLOW_MESSAGE = "There might be a cyclical dependency on one of your models. Please review your code and try again";

        public static Func<Source, Target, PassOnEngine, int, Target> CreateMerger<Source, Target>()
        {
            Delegate mapper = null;

            var dynMethod = new DynamicMethod(
                "DoDeepMerge",
                typeof(Target),
                new Type[] { typeof(Source), typeof(Target), typeof(PassOnEngine), typeof(int) },
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

            Match.Properties<Source, Target>(
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
                        il.EmitPropertyPassing(resultLocal, src, tgt);
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
                .MakeGenericType(typeof(Source), typeof(Target), typeof(PassOnEngine), typeof(int), typeof(Target));

            mapper = dynMethod.CreateDelegate(delType);

            return (Func<Source, Target, PassOnEngine, int, Target>)mapper;
        }

        public static Func<Source, PassOnEngine, int, Target> CreateMapper<Source, Target>()
        {
            Delegate mapper = null;

            // Create ILGenerator            
            var dynMethod = new DynamicMethod(
                "DoDeepMap",
                typeof(Target),
                new Type[] { typeof(Source), typeof(PassOnEngine), typeof(int) },
                Assembly.GetExecutingAssembly().ManifestModule,
                true);

            var il =
                dynMethod.GetILGenerator();

            var resultLocal =
                il.DeclareLocal(typeof(Target));

            il.Construct<Target>();
            il.Emit(OpCodes.Stloc, resultLocal);

            EmitStackOverflowCheck(il, dynMethod);

            il.TryEmitBeforeFuncs<Source, Target>();

            Match.Properties<Source, Target>((src, tgt) =>
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
                ))
                {
                    il.EmitPropertyPassing(resultLocal, src, tgt);
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

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            var delType = typeof(Func<,,,>)
                .MakeGenericType(typeof(Source), typeof(PassOnEngine), typeof(int), typeof(Target));

            mapper = dynMethod.CreateDelegate(delType);

            return (Func<Source, PassOnEngine, int, Target>)mapper;
        }

        internal static Func<Source, PassOnEngine, int, Target> CreateRawMapper<Source, Target>()
        {
            Delegate mapper = null;

            // Create ILGenerator            
            var dynMethod = new DynamicMethod(
                "DoDeepRawMap",
                typeof(Target),
                new Type[] { typeof(Source), typeof(PassOnEngine), typeof(int) },
                Assembly.GetExecutingAssembly().ManifestModule,
                true);

            var il =
                dynMethod.GetILGenerator();

            var resultLocal =
                il.DeclareLocal(typeof(Target));

            il.Construct<Target>();
            il.Emit(OpCodes.Stloc, resultLocal);

            EmitStackOverflowCheck(il, dynMethod);

            Match.PropertiesNoStrategy<Source, Target>((src, tgt) =>
            {
                if (tgt.PropertyType.IsAssignableFrom(src.PropertyType)
                    && (
                        src.PropertyType.IsValueType
                        || src.PropertyType == typeof(string)
                    )
                )
                {
                    il.EmitPropertyPassing(resultLocal, src, tgt);
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
                .MakeGenericType(typeof(Source), typeof(PassOnEngine), typeof(int), typeof(Target));

            mapper = dynMethod.CreateDelegate(delType);

            return (Func<Source, PassOnEngine, int, Target>)mapper;
        }

        internal static void EmitMapTargetToResult<Target>(ILGenerator il, LocalBuilder resultLocal)
        {
            //il.Emit(OpCodes.Ldarg_1);
            //il.Emit(OpCodes.Stloc, resultLocal);
            //return;


            var valueIsNullLabel = il.DefineLabel();
            var valueIsNotNullLabel = il.DefineLabel();

            (
                MethodInfo mapTarget,
                bool isRecursive
            ) = InternalMapperFactory.GetMapper(
                typeof(Target), typeof(Target), rawClone: true);

            //IL_0000: nop
            il.Emit(OpCodes.Nop);
            //IL_0001: ldarg.1
            il.Emit(OpCodes.Ldarg_1);
            //IL_0002: ldnull
            il.Emit(OpCodes.Ldnull);
            //IL_0003: ceq
            il.Emit(OpCodes.Ceq);
            //IL_0005: stloc.1
            //// sequence point: hidden
            //IL_0006: ldloc.1
            //IL_0007: brfalse.s IL_0013
            il.Emit(OpCodes.Brfalse_S, valueIsNotNullLabel);

            //IL_0009: nop
            //IL_000a: newobj instance void C/ Target::.ctor()
            if (typeof(Target).IsArray)
            {
                il.Emit(OpCodes.Ldc_I4_S, 255);
                il.Emit(OpCodes.Newarr, typeof(Target).GetElementType());
            }
            else
            {
                il.Emit(OpCodes.Newobj, typeof(Target).GetConstructor(Type.EmptyTypes));
            }

            // il.Construct<Target>(); // maybe here?

            //IL_000f: stloc.0
            il.Emit(OpCodes.Stloc, resultLocal);
            //IL_0010: nop
            //// sequence point: hidden
            //IL_0011: br.s IL_001e
            il.Emit(OpCodes.Br_S, valueIsNullLabel);

            //IL_0013: nop
            il.MarkLabel(valueIsNotNullLabel);
            il.Emit(OpCodes.Nop);
            //IL_0014: ldarg.1
            il.Emit(OpCodes.Ldarg_1); // target (merging)
            //IL_0015: ldarg.2
            il.Emit(OpCodes.Ldarg_2); // engine
            //IL_0016: ldarg.3
            il.Emit(OpCodes.Ldarg_3); // recursionIndex
            //IL_0017: call!!1 C::MapObjectWithILDeepInternal <class C/Target, class C/Target>(!!0, class C/Engine, int32)
            il.Emit(OpCodes.Call, mapTarget);
            //IL_001c: stloc.0
            il.Emit(OpCodes.Stloc, resultLocal);
            //IL_001d: nop
            il.Emit(OpCodes.Nop);
            il.MarkLabel(valueIsNullLabel);
        }

        internal static void EmitReferenceTypeCopy(
            ILGenerator il,
            DynamicMethod dynMethod,
            LocalBuilder resultLocal,
            PropertyInfo source,
            PropertyInfo target,
            bool isForMerging = false)
        {
            // does not copy a delegate
            if (source.PropertyType.IsSubclassOf(typeof(Delegate))) { return; }

            var srcType = source.PropertyType;
            var tgtType = target.PropertyType;

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
            il.Emit(OpCodes.Callvirt, source.GetGetMethod());
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Cgt_Un);

            var valueIsNullLabel = il.DefineLabel();
            il.Emit(OpCodes.Brfalse_S, valueIsNullLabel);
            il.Emit(OpCodes.Nop);
            // check if src is null

            il.Emit(OpCodes.Ldloc, resultLocal);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, source.GetGetMethod());

            if (isMergeable)
            {
                il.Emit(OpCodes.Ldloc, resultLocal);
                il.Emit(OpCodes.Callvirt, target.GetGetMethod());
            }

            il.Emit(OpCodes.Ldarg, engineArgIndex); // engine
            il.Emit(OpCodes.Ldarg, recursionIndexArgIndex);

            if (isRecursive)
            {
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Add); // recursionIndex + 1
            }

            il.Emit(OpCodes.Call, internalMapFunc);
            il.Emit(OpCodes.Callvirt, target.GetSetMethod());
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
