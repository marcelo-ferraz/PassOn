using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using PassOn.Utilities;
using PassOn.Collections;

namespace PassOn
{

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
        private static Cache _cachedILMerge = new Cache();
        private static Cache _cachedILShallowClone = new Cache();
        private static Cache _cachedILDeepClone = new Cache();

        /// <summary>
        /// Generic cloning method that clones an object using IL.
        /// Only the first call of a certain type will hold back performance.
        /// After the first call, the compiled IL is executed. 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        internal static object MergeWithILDeep(object source, object destination, Type returnType = null)
        {
            if(source == null || destination == null || returnType == null)
            {
                if (returnType == null)
                {
                    if (source != null || destination != null)
                    {
                        returnType =
                            destination == null ?
                            destination.GetType() :
                            source.GetType();
                    }
                    else { throw new ArgumentNullException("There are no means to infer the returned type. All arguments are null"); }
                }
                
                if (destination == null)
                { return CloneObjectWithILDeep(returnType, source); } 
                
                if (source == null) 
                { return CloneObjectWithILDeep(returnType, destination);  }            
            }

            CacheItem item = 
                _cachedILMerge.Get(returnType, source.GetType(), destination.GetType());

            if (item == null)
            {
                // Create ILGenerator            
                DynamicMethod dymMethod = new DynamicMethod(
                    "DoDeepMerge",
                    returnType,
                    new Type[] { source.GetType(), typeof(Delegate[]), destination.GetType() },
                    Assembly.GetExecutingAssembly().ManifestModule,
                    true);

                ILGenerator il = dymMethod.GetILGenerator();

                LocalBuilder cloneVariable = il.DeclareLocal(returnType);

                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Stloc, cloneVariable);

                List<Delegate> parsers = null;

                Copy.Properties(destination.GetType(), source.GetType(), il,
                    (src, dest, hasParsing, i) =>
                        Copy.PropertyDeeply(src, dest, hasParsing, i, il, cloneVariable), 
                    true,
                    out parsers);

                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ret);

                var delType = typeof(Func<,,,>).MakeGenericType(
                    source.GetType(),
                    typeof(Delegate[]), 
                    destination.GetType(), 
                    returnType);

                item = _cachedILMerge.Add(
                    dymMethod.CreateDelegate(delType),
                    parsers.ToArray(),
                    returnType, source.GetType(), destination.GetType());
            }
            return item.Parser.DynamicInvoke(
                source, item.CustomParsers, destination);
        }
        
        /// <summary>
        /// Generic cloning method that clones an object using IL.
        /// Only the first call of a certain type will hold back performance.
        /// After the first call, the compiled IL is executed. 
        /// </summary>
        /// <param name="returnType">Type of return object with the cloned values</param>
        /// <param name="source">source object that will be cloned</param>
        /// <returns>Cloned object (deeply cloned)</returns>
        internal static object CloneObjectWithILDeep(Type returnType, object source)
        {
            if (source == null || returnType == null)
            {
                if (returnType == null)
                {
                    if (source == null)
                    { throw new ArgumentNullException("There are no means to infer the returned type. All arguments are null"); }

                    returnType = source.GetType();
                }
                                
                if (source == null)
                { return Activator.CreateInstance(returnType); }
            }


            if (source.GetType() == typeof(int))
            {
                System.Diagnostics.Debugger.Break();
            }

            if (!source.GetType().IsClass)
            {
                return CloneObjectWithILShallow(returnType, source);
            }

            CacheItem item =
                _cachedILMerge.Get(returnType, source.GetType());

            if (item == null)
            {
                // Create ILGenerator            
                var dymMethod = new DynamicMethod(
                    "DoDeepClone",
                    returnType,
                    new Type[] { source.GetType(), typeof(Delegate[]) },
                    Assembly.GetExecutingAssembly().ManifestModule,
                    true);

                var il =
                    dymMethod.GetILGenerator();

                var cloneVariable =
                    il.DeclareLocal(returnType);

                il.Construct(returnType);
                il.Emit(OpCodes.Stloc, cloneVariable);

                List<Delegate> parsers = null;

                Copy.Properties(
                    returnType, 
                    source.GetType(), 
                    il,
                    (src, dest, hasParsing, i) => 
                        Copy.PropertyDeeply(src, dest, hasParsing, i, il, cloneVariable), 
                    true, 
                    out parsers);

                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ret);

                var delType = typeof(Func<,,>)
                    .MakeGenericType(source.GetType(), typeof(Delegate[]), returnType);

                item = _cachedILDeepClone.Add(
                    dymMethod.CreateDelegate(delType),
                    parsers.ToArray(),
                    returnType, source.GetType());
            }
            return item.Parser.DynamicInvoke(
                source, item.CustomParsers);
        }
        
        public static void SaveMethod(Type returnType, object source)
        {
            if (source == null || returnType == null)
            {
                if (returnType == null)
                {
                    if (source == null)
                    { throw new ArgumentNullException("There are no means to infer the returned type. All arguments are null"); }

                    returnType = source.GetType();
                }

                if (source == null)
                { return; }
            }
            
            var ass = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("nhonho"),
                AssemblyBuilderAccess.RunAndSave);

            var mod = ass.DefineDynamicModule(
                "nhonho.dll",
                string.Concat(ass.GetName().Name, ".dll"));

            var holderType = mod.DefineType("Holder");

            var copyMethod =
                holderType.DefineMethod("Copy", MethodAttributes.Static | MethodAttributes.Public, returnType,
                           new Type[] { source.GetType(), typeof(Delegate[]) });

            var il =
                copyMethod.GetILGenerator();

            var cloneVariable =
                il.DeclareLocal(returnType);

            il.Construct(returnType);
            il.Emit(OpCodes.Stloc, cloneVariable);

            List<Delegate> parsers = null;

            Copy.Properties(
                returnType,
                source.GetType(),
                il,
                (src, dest, hasParsing, i) =>
                    Copy.PropertyDeeply(src, dest, hasParsing, i, il, cloneVariable),
                true,
                out parsers);

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);


            holderType.CreateType();

            ass.Save("nhonho.dll");
        }

        /// <summary>    
        /// Generic cloning method that clones an object using IL.    
        /// Only the first call of a certain type will hold back performance.    
        /// After the first call, the compiled IL is executed.    
        /// </summary>    
        /// <typeparam name="T">Type of object to clone</typeparam>    
        /// <param name="left">Object to clone</param>    
        /// <returns>Cloned object (shallow)</returns>    
        internal static object CloneObjectWithILShallow(Type returnType, object source)
        {
            if (source == null || returnType == null)
            {
                if (returnType == null)
                {
                    if (source == null)
                    {
                        throw new ArgumentNullException("There are no means to infer the returned type. All arguments are null");
                    }

                    returnType = source.GetType();
                }

                if (source == null)
                { return Activator.CreateInstance(returnType); }
            }

            CacheItem item =
                _cachedILMerge.Get(returnType, source.GetType());

            if (item == null)
            {
                var dymMethod = new DynamicMethod(
                    "DoShallowClone", 
                    returnType,
                    new Type[] { source.GetType(), typeof(Delegate[]) }, 
                    Assembly.GetExecutingAssembly().ManifestModule, true);

                var cInfo =
                    returnType.GetConstructor(new Type[] { });
                
                var il =
                    dymMethod.GetILGenerator();

                var local =
                    il.DeclareLocal(returnType);

                if (returnType.IsClass)
                { il.Emit(OpCodes.Newobj, cInfo); }
                else 
                { il.EmitDefaultValue(returnType); }

                il.Emit(OpCodes.Stloc_0);

                Copy.Properties(
                    returnType, 
                    source.GetType(), 
                    il,
                    (src, dest, hasParsing, i) =>
                    {
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Call, src.GetGetMethod());
                        il.Emit(OpCodes.Call, dest.GetSetMethod());
                    }, 
                    false);

                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ret);

                var delType = typeof(Func<,,>)
                    .MakeGenericType(source.GetType(), typeof(Delegate[]), source.GetType());

                item = _cachedILDeepClone.Add(
                    dymMethod.CreateDelegate(delType),
                    null,
                    returnType, source.GetType());
            }
            return item.Parser.DynamicInvoke(source, null);
        }

        public static object PassValue(Type returnType, object source)
        {
            if (source == null || returnType == null)
            {
                if (returnType == null)
                {
                    if (source == null)
                    {
                        throw new ArgumentNullException("There are no means to infer the returned type. All arguments are null");
                    }

                    returnType = source.GetType();
                }

                if (source == null)
                { return Activator.CreateInstance(returnType); }
            }

            CacheItem item =
                _cachedILMerge.Get(returnType, source.GetType());

            if (item == null)
            {
                var dymMethod = new DynamicMethod(
                    "DoPassValue",
                    returnType,
                    new Type[] { source.GetType(), typeof(Delegate[]) },
                    Assembly.GetExecutingAssembly().ManifestModule, true);

                var il =
                    dymMethod.GetILGenerator();

                //if (Get.CloneTypeForProperty(src) == Inspection.Custom && hasParsing)
                //{
                //    il.EmitCopyWithCustomParser(cloneVariable, src, dest, index);
                //}
                //else
                { il.Emit(OpCodes.Ldarg_0); }
                il.Emit(OpCodes.Ret);

                var delType = typeof(Func<,,>)
                    .MakeGenericType(source.GetType(), typeof(Delegate[]), source.GetType());

                item = _cachedILDeepClone.Add(
                    dymMethod.CreateDelegate(delType),
                    null,
                    returnType, source.GetType());
            }
            return item.Parser.DynamicInvoke(source, null);
        }
    }
}