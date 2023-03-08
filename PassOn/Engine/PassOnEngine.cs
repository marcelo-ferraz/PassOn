﻿using Microsoft.Win32;
using PassOn.EngineExtensions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

namespace PassOn
{
    using CloneKey = Tuple<Type, Type>;
    using MergeKey = Tuple<Type, Type, Type>;

    using DicClone = ConcurrentDictionary<Tuple<Type, Type>, Delegate>;
    using DicMerge = ConcurrentDictionary<Tuple<Type, Type, Type>, Delegate>;
    
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
    public static class PassOnEngine
    {       
        // Dictionaries for caching the (pre)compiled generated IL code.
        private static DicMerge _cachedILMerge = new DicMerge();
        private static DicClone _cachedILShallowClone = new DicClone();
        private static DicClone _cachedILDeepClone = new DicClone();

        internal static Delegate GetOrCreate<Source, Target>(Type targetType)
        {
            Delegate mapper = null;

            var key = new MergeKey(
                typeof(Target),
                typeof(Source),
                targetType
            );

            if (_cachedILMerge.TryGetValue(key, out mapper))
            { return mapper; }

            var dymMethod = new DynamicMethod(
                "DoDeepMerge",
                targetType,
                new Type[] { typeof(Source), targetType },
                Assembly.GetExecutingAssembly().ManifestModule,
                true);

            var il = dymMethod.GetILGenerator();

            var cloneVariable = il.DeclareLocal(typeof(Target));

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stloc, cloneVariable);

            Match.Properties<Source, Target>(
                (src, tgt) =>
                {
                    var srcStrategyType = src.GetStrategyType();

                    var emitted = il.TryEmitByStrategy<Source, Target>(cloneVariable, src, tgt);

                    if (emitted) { return; }

                    if (tgt.PropertyType.IsAssignableFrom(src.PropertyType)
                        && (
                            srcStrategyType == Strategy.Shallow
                            || src.PropertyType.IsValueType
                            || src.PropertyType == typeof(string)
                    ))
                    {
                        il.EmitPropertyPassing(cloneVariable, src, tgt);
                        return;
                    }

                    //Inspection.Deep
                    if (src.PropertyType.IsClass)
                    {
                        il.EmitReferenceTypeCopy(cloneVariable, src, tgt);
                        return;
                    }
                });

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            var delType = typeof(Func<,,>)
                .MakeGenericType(typeof(Source), targetType, typeof(Target));

            mapper = dymMethod.CreateDelegate(delType);
            _cachedILMerge.TryAdd(key, mapper);
            
            return mapper;
        }

        internal static Delegate GetOrCreate<Source, Target>() {
            var key = new CloneKey(
                typeof(Target), typeof(Source));

            Delegate mapper = null;

            if (_cachedILDeepClone.TryGetValue(key, out mapper))
            { return mapper;  }

            // Create ILGenerator            
            var dymMethod = new DynamicMethod(
                "DoDeepClone",
                typeof(Target),
                new Type[] { typeof(Source) },
                Assembly.GetExecutingAssembly().ManifestModule,
                true);

            var il =
                dymMethod.GetILGenerator();

            var cloneVariable =
                il.DeclareLocal(typeof(Target));

            il.Construct<Target>();

            il.Emit(OpCodes.Stloc, cloneVariable);

            Match.Properties<Source, Target>((src, tgt) =>
            {
                var strategyType = src.GetStrategyType();

                var emitted = il.TryEmitByStrategy<Source, Target>(cloneVariable, src, tgt);

                if (emitted) { return; }

                if (tgt.PropertyType.IsAssignableFrom(src.PropertyType)
                    && (
                        strategyType == Strategy.Shallow
                        || src.PropertyType.IsValueType
                        || src.PropertyType == typeof(string)
                ))
                {
                    il.EmitPropertyPassing(cloneVariable, src, tgt);
                    return;
                }

                //Inspection.Deep
                if (src.PropertyType.IsClass)
                {
                    il.EmitReferenceTypeCopy(cloneVariable, src, tgt);
                    return;
                }
            }, ignoreType: true);

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            var delType = typeof(Func<,>)
                .MakeGenericType(typeof(Source), typeof(Target));

            mapper = dymMethod.CreateDelegate(delType);
            _cachedILDeepClone.TryAdd(key, mapper);
            
            return mapper;
        }

        internal static Delegate GetOrCreateShallow<Source, Target>() {
            Delegate mapper = null;

            var key = new CloneKey(
                typeof(Target), typeof(Source));

            if (_cachedILShallowClone.TryGetValue(key, out mapper))
            {
                return mapper;
            }

            var dymMethod = new DynamicMethod(
                "DoShallowClone",
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

            Match.Properties<Source, Target>((src, target) =>
            {
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, src.GetGetMethod());
                il.Emit(OpCodes.Call, target.GetSetMethod());
            });

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            var delType = typeof(Func<,>)
                .MakeGenericType(typeof(Source), typeof(Source));

            mapper = dymMethod.CreateDelegate(delType);
            _cachedILShallowClone.TryAdd(key, mapper);

            return mapper;
        }

        internal static Target MergeWithILDeep<Source, Target>(Source source, Target target)
        {
            if (target == null)
            { return MapObjectWithILDeep<Source, Target>(source); }

            if (source == null)
            { return MapObjectWithILDeep<Target, Target>(target); }

            var mapper = GetOrCreate<Source, Target>(target?.GetType() ?? typeof(Target));

            return ((Func<Source, Target, Target>) mapper)(source, target);
        }

        internal static Target MapObjectWithILDeep<Source, Target>(Source source)
        {
            if (source == null)
            { return (Target) Activator.CreateInstance(typeof(Target)); }

            var key = new Tuple<Type, Type>(
                typeof(Target), typeof(Source));

            var mapper = GetOrCreate<Source, Target>();

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
        internal static Target CloneObjectWithILShallow<Source, Target>(Source source)
        {            
            if (source == null)
            { return (Target)Activator.CreateInstance(typeof(Target)); }

            var mapper = GetOrCreateShallow<Source, Target>();

            return ((Func<Source, Target>)mapper)(source);
        }

        public static void ClearCache() {
            _cachedILShallowClone.Clear();
            _cachedILDeepClone.Clear();
            _cachedILMerge.Clear();
        }
    }
}