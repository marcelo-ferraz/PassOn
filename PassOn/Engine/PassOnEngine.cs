using PassOn.Engine.Extensions;
using PassOn.EngineExtensions;
using PassOn.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace PassOn
{
    using MapKey = Tuple<Type, Type, bool>;
    using ShallowMapKey = Tuple<Type, Type>;
    using MergeKey = Tuple<Type, Type>;

    using MapDic = ConcurrentDictionary<Tuple<Type, Type, bool>, Delegate>;
    using ShallowMapDic = ConcurrentDictionary<Tuple<Type, Type>, Delegate>;
    using MergeDic = ConcurrentDictionary<Tuple<Type, Type>, Delegate>;
    
    /// <summary>
    /// Class that clones objects
    /// </summary>
    /// <remarks>
    /// Currently can deepclone to 1 level deep.
    /// Ex. Person.Addresses (Person.List<Address>) 
    /// -> Clones 'Person' deep
    /// -> Clones the objects of the 'Address' list deep
    /// -> Clones the sub-objects of the Address object shallow. (at the moment)
    /// </remarks>
    public class PassOnEngine
    {
        // Dictionaries for caching the (pre)compiled generated IL code.
        private MergeDic _cachedILMerge = new MergeDic();
        private ShallowMapDic _cachedILShallowMap = new ShallowMapDic();
        private MapDic _cachedILDeepMap = new MapDic();

        public Func<Source, Target, Target> GetOrCreateMerger<Source, Target>()
        {         
            var mapperFunc = GetOrCreateInternalMerger<Source, Target>();

            return new Func<Source, Target, Target>(
                (src, tgt) => mapperFunc(src, tgt, this, 0)
            );
        }

        public Func<Source, Target> GetOrCreateMapper<Source, Target>()
        {
            var mapperFunc = 
                GetOrCreateInternalMapper<Source, Target>();

            return new Func<Source, Target>(
                (src) => mapperFunc(src, this, 0)
            );
        }

        public Delegate GetOrCreateShallowMapper<Source, Target>() 
        {
            Delegate mapper = null;

            var key = new ShallowMapKey(
                typeof(Target), typeof(Source));

            if (_cachedILShallowMap.TryGetValue(key, out mapper))
            {
                return mapper;
            }

            var dymMethod = new DynamicMethod(
                "DoShallowMap",
                typeof(Target),
                new Type[] { typeof(Source) },
                Assembly.GetExecutingAssembly().ManifestModule,
                true);

            var cInfo =
                typeof(Target).GetConstructor(new Type[] { });

            var il =
                dymMethod.GetILGenerator();

            var local =
                il.DeclareLocal(typeof(Source));

            il.Emit(OpCodes.Newobj, cInfo);
            il.Emit(OpCodes.Stloc_0);

            il.TryEmitBeforeFuncs<Source, Target>();

            Match.Properties<Source, Target>((src, target) =>
            {
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, src.GetGetMethod());
                il.Emit(OpCodes.Call, target.GetSetMethod());
            });

            il.TryEmitAfterFuncs<Source, Target>();

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            var delType = typeof(Func<,>)
                .MakeGenericType(typeof(Source), typeof(Source));

            mapper = dymMethod.CreateDelegate(delType);
            _cachedILShallowMap.TryAdd(key, mapper);

            return mapper;
        }

        public Target MergeWithILDeep<Source, Target>(Source source, Target target)
        {   
            if (source == null)
            { throw new ArgumentNullException("source"); }

            if (target == null)
            { throw new ArgumentNullException("target"); }

            var mapper = GetOrCreateMerger<Source, Target>();

            return mapper(source, target);
        }

        public Target MapObjectWithILDeep<Source, Target>(Source source)
        {
            if (source == null)
            { throw new ArgumentNullException("source"); }

            var key = new Tuple<Type, Type>(
                typeof(Target), typeof(Source));

            var mapper = GetOrCreateMapper<Source, Target>();

            return ((Func<Source, Target>)mapper)(source);
        }

        /// <summary>    
        /// Generic cloning method that clones an object using IL.    
        /// Only the first call of a certain type will hold back performance.    
        /// After the first call, the compiled IL is executed.    
        /// </summary>    
        /// <typeparam name="T">Type of object to clone</typeparam>    
        /// <param name="left">Object to clone</param>    
        /// <returns>Cloned object (shallow)</returns>    
        public Target CloneObjectWithILShallow<Source, Target>(Source source)
        {            
            if (source == null)
            { return (Target)Activator.CreateInstance(typeof(Target)); }

            var mapper = GetOrCreateShallowMapper<Source, Target>();

            return ((Func<Source, Target>)mapper)(source);
        }

        public void ClearCache()
        {
            _cachedILShallowMap.Clear();
            _cachedILDeepMap.Clear();
            _cachedILMerge.Clear();
        }

        internal Func<Source, Target, PassOnEngine, int, Target> GetOrCreateInternalMerger<Source, Target>()
        {
            Delegate mapperDel = null;

            var key = new MergeKey(
                typeof(Target),
                typeof(Source)
            );

            if (!_cachedILMerge.TryGetValue(key, out mapperDel))
            {
                mapperDel = MapperFactories.CreateMerger<Source, Target>();

                _cachedILMerge.TryAdd(key, mapperDel);
            }

            return (Func<Source, Target, PassOnEngine, int, Target>)mapperDel;
        }

        internal Func<Source, PassOnEngine, int, Target> GetOrCreateInternalMapper<Source, Target>(bool raw = false)
        {
            var key = new MapKey(
               typeof(Target), typeof(Source), raw);

            Delegate mapperDel = null;

            if (!_cachedILDeepMap.TryGetValue(key, out mapperDel))
            {
                mapperDel = raw 
                    ? MapperFactories.CreateRawMapper<Source, Target>()
                    : MapperFactories.CreateMapper<Source, Target>();

                _cachedILDeepMap.TryAdd(key, mapperDel);
            }
            return (Func<Source, PassOnEngine, int, Target>)mapperDel;
        }

        private static MethodInfo GetMappingForCollections(Type source, Type target)
        {
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

    public static class MapperFactories {
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
                        EmitReferenceTypeCopy(il, dynMethod, resultLocal, src, tgt);
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

        internal static void EmitMapTargetToResult<Target>(ILGenerator il, LocalBuilder resultLocal)
        {
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
            il.Emit(OpCodes.Newobj, typeof(Target).GetConstructor(Type.EmptyTypes));
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

            (
                MethodInfo internalMapFunc,
                bool isRecursive
            ) = InternalMapperFactory.GetMapper(source, target);

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
            
            if (isForMerging) {
                il.Emit(OpCodes.Ldarg_1);
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

    public class InternalMapperFactory
    {
        internal static (MethodInfo, bool) GetMapper(PropertyInfo source, PropertyInfo target)
        {
            return GetMapper(source.PropertyType, target.PropertyType);
        }

        internal static (MethodInfo, bool) GetMapper(Type source, Type target, bool rawClone = false)
        {
            var srcTypeIsIEnumerable = typeof(IEnumerable).IsAssignableFrom(source);
            var tgtTypeIsIEnumerable = typeof(IEnumerable).IsAssignableFrom(target);

            if (!srcTypeIsIEnumerable && !tgtTypeIsIEnumerable)
            {
                var name = rawClone
                    ? "MapObjectRawWithILDeepInternal"
                    : "MapObjectWithILDeepInternal";

                var objMapper = typeof(InternalRefMappers)
                    .GetMethod(name)
                    .MakeGenericMethod(source, target);
                return (objMapper, true);
            }

            var srcItemType = source.IsArray
                ? source.GetElementType()
                : source.GetGenericArguments()[0];

            var tgtItemType = target.IsArray
                ? target.GetElementType()
                : target.GetGenericArguments()[0];

            MethodInfo mapper = null;

            var mappersHolderType =
                (srcItemType.IsValueType || tgtItemType.IsValueType)
                ? typeof(InternalValueMappers)
                : typeof(InternalRefMappers);

            if (srcTypeIsIEnumerable && target.IsArray)
            {
                mapper = mappersHolderType
                    .GetMethod("MapIEnumerableToArray")
                    .MakeGenericMethod(srcItemType, tgtItemType);
            }
            else if (tgtTypeIsIEnumerable && source.IsArray)
            {
                mapper = mappersHolderType
                    .GetMethod("MapArrayToIEnumerable")
                    .MakeGenericMethod(srcItemType, tgtItemType);
            }
            else if (source.IsArray && target.IsArray)
            {
                mapper = mappersHolderType
                    .GetMethod("MapIEnumerableToArray")
                    .MakeGenericMethod(srcItemType, tgtItemType);
            }
            else if (srcTypeIsIEnumerable && tgtTypeIsIEnumerable)
            {
                mapper = mappersHolderType
                    .GetMethod("MapIEnumerableToList")
                    .MakeGenericMethod(srcItemType, tgtItemType);
            }

            if (mapper == null)
            {
                throw new NotSupportedException(
                    $"This mapping \"{source.Name}\" <-> \"{target.Name}\" is not supported!"
                );
            }

            return (mapper, false);
        }
    }

    public class InternalValueMappers {
        public static IEnumerable<Target> MapIEnumerableToIEnumerable<Source, Target>(IEnumerable<Source> enumerable, PassOnEngine engine, int recursionIndex)
            where Source : Target
        {
            if (enumerable == null)
            { yield break; }

            foreach (var item in enumerable)
            {
                yield return (Target)(typeof(Target) == typeof(string)
                    ? (object)item.ToString()
                    : item);
            }

            yield break;
        }

        public static IEnumerable<Target> MapArrayToIEnumerable<Source, Target>(Source[] array, PassOnEngine engine, int recursionIndex)
           where Source : Target
        {
            if (array == null) { yield break; }

            var mapper = engine.GetOrCreateInternalMapper<Source, Target>();

            for (int i = 0; i < array.Length; i++)
            {
                yield return (Target)(typeof(Target) == typeof(string)
                    ? (object)array[i].ToString()
                    : array[i]);
            }

            yield break;
        }

        /// <summary>
        /// Maps or merges all the values of an IEnumerable to another list of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="enumerable">the source</param>
        /// <returns>An array of the target type</returns>
        public static List<Target> MapIEnumerableToList<Source, Target>(IEnumerable<Source> enumerable, PassOnEngine engine, int recursionIndex)
            where Source: Target
        {
            return MapIEnumerableToIEnumerable<Source, Target>(
                enumerable, engine, recursionIndex
            ).ToList();
        }

        /// <summary>
        /// Maps or merges all the values of an array to another list of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="array">left array</param>
        /// <returns>A list of the target type</returns>
        public static List<Target> MapArrayToList<Source, Target>(Source[] array, PassOnEngine engine, int recursionIndex)
             where Source : Target
        {
            return MapIEnumerableToIEnumerable<Source, Target>(
                array, engine, recursionIndex
            ).ToList();
        }

        /// <summary>
        /// Maps or merges all the values of an left to another array of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="source">left</param>
        /// <returns>An array of the target type</returns>
        public static Target[] MapIEnumerableToArray<Source, Target>(IEnumerable<Source> source, PassOnEngine engine, int recursionIndex)
            where Source : Target
        {
            return MapIEnumerableToIEnumerable<Source, Target>(
                source, engine, recursionIndex
            ).ToArray();
        }

        /// <summary>
        /// Maps or merges all the values of an array to another array of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="array">left array</param>
        /// <returns>An array of the target type</returns>
        public static Target[] MapArrayToArray<Source, Target>(Source[] array, PassOnEngine engine, int recursionIndex)
            where Source : Target
        {
            return MapIEnumerableToIEnumerable<Source, Target>(
                array, engine, recursionIndex
            ).ToArray();
        }
    }

    public class InternalRefMappers {

        public static Target MapObjectRawWithILDeepInternal<Source, Target>(Source source, PassOnEngine engine, int recursionIndex)
        {
            System.Diagnostics.Debugger.Break();
            var mapper = engine.GetOrCreateInternalMapper<Source, Target>(raw: true);
            return mapper(source, engine, recursionIndex);
        }

        public static Target MapObjectWithILDeepInternal<Source, Target>(Source source, PassOnEngine engine, int recursionIndex)
        {
            System.Diagnostics.Debugger.Break();
            var mapper = engine.GetOrCreateInternalMapper<Source, Target>();
            return mapper(source, engine, recursionIndex);
        }
               

        /// <summary>
        /// Maps or merges all the values of an IEnumerable to another list of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="enumerable">the source</param>
        /// <returns>An array of the target type</returns>
        public static IEnumerable<Target> MapIEnumerableToIEnumerable<Source, Target>(IEnumerable<Source> enumerable, PassOnEngine engine, int recursionIndex)
        {
            if (enumerable == null)
            { yield break; }

            var mapper = engine.GetOrCreateInternalMapper<Source, Target>();

            foreach (var item in enumerable)
            {
                yield return mapper(item, engine, recursionIndex);
            }

            yield break;
        }

        /// <summary>
        /// Maps or merges all the values of an array to another list of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="array">left array</param>
        /// <returns>A list of the target type</returns>
        public static IEnumerable<Target> MapArrayToIEnumerable<Source, Target>(Source[] array, PassOnEngine engine, int recursionIndex)
            where Target : class
        {
            if (array == null) { yield break; }

            var mapper = engine.GetOrCreateInternalMapper<Source, Target>();

            for (int i = 0; i < array.Length; i++)
            {
                yield return mapper(array[i], engine, recursionIndex);
            }

            yield break;
        }

        /// <summary>
        /// Maps or merges all the values of an IEnumerable to another list of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="enumerable">the source</param>
        /// <returns>An array of the target type</returns>
        public static List<Target> MapIEnumerableToList<Source, Target>(IEnumerable<Source> enumerable, PassOnEngine engine, int recursionIndex)
        {
            return MapIEnumerableToIEnumerable<Source, Target>(
                enumerable, engine, recursionIndex
            ).ToList();
        }

        /// <summary>
        /// Maps or merges all the values of an array to another list of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="array">left array</param>
        /// <returns>A list of the target type</returns>
        public static List<Target> MapArrayToList<Source, Target>(Source[] array, PassOnEngine engine, int recursionIndex)
            where Target : class
        {
            return MapIEnumerableToIEnumerable<Source, Target>(
                array, engine, recursionIndex
            ).ToList();
        }

        /// <summary>
        /// Maps or merges all the values of an left to another array of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="source">left</param>
        /// <returns>An array of the target type</returns>
        public static Target[] MapIEnumerableToArray<Source, Target>(IEnumerable<Source> source, PassOnEngine engine, int recursionIndex)
        {
            return MapIEnumerableToIEnumerable<Source, Target>(
                source, engine, recursionIndex
            ).ToArray();
        }

        /// <summary>
        /// Maps or merges all the values of an array to another array of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="array">left array</param>
        /// <returns>An array of the target type</returns>
        public static Target[] MapArrayToArray<Source, Target>(Source[] array, PassOnEngine engine, int recursionIndex)
            where Target : class
        {
            return MapIEnumerableToIEnumerable<Source, Target>(
                array, engine, recursionIndex
            ).ToArray();
        }
    }
}