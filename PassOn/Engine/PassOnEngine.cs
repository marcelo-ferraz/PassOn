using PassOn.Engine.Extensions;
using PassOn.EngineExtensions;
using PassOn.Utilities;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

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

        public Func<Source, Target> GetOrCreateShallowMapper<Source, Target>() 
        {
            Delegate mapper = null;

            var key = new ShallowMapKey(
                typeof(Target), typeof(Source));

            if (_cachedILShallowMap.TryGetValue(key, out mapper))
            {
                return (Func<Source, Target>)mapper;
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

            return (Func<Source, Target>) mapper;
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
}