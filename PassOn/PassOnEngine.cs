using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

namespace PassOn
{
    using DicMerge = Dictionary<Tuple<Type, Type, Type>, Delegate>;
    using DicClone = Dictionary<Tuple<Type, Type>, Delegate>;

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
        

        private static void CopyProperties<Src, Ret>(ILGenerator il, Action<PropertyInfo, PropertyInfo> whenSame, bool ignoreType = false)
        {
            var destPropertys =
                GetProperties(typeof(Ret));

            foreach (var srcProperty in GetProperties(typeof(Src)))
            {
                foreach (var destProperty in destPropertys)
                {
                    Func<bool> isAssignable = () =>
                        destProperty.PropertyType.IsAssignableFrom(srcProperty.PropertyType) ||
                        srcProperty.PropertyType.IsAssignableFrom(destProperty.PropertyType);

                    var srcAliases = GetAliasesForProperty(srcProperty);
                    var destAliases = GetAliasesForProperty(destProperty);

                    var namesAreEqual =
                        destProperty.Name.Equals(srcProperty.Name) ||
                        (srcAliases != null && Array.IndexOf(srcAliases, destProperty.Name, 0) > -1) ||                        
                        (destAliases != null && Array.IndexOf(destAliases, srcProperty.Name, 0) > -1);

                    if ((ignoreType || isAssignable()) && namesAreEqual)
                    {
                        whenSame(srcProperty, destProperty);
                    }
                }
            }
        }

        private static void Construct(Type type, ILGenerator il)
        {
            ConstructorInfo cInfo = type.GetConstructor(Type.EmptyTypes);
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

        private static void EmitPassOn(ILGenerator il, LocalBuilder cloneVariable, PropertyInfo srcProperty, PropertyInfo destProperty)
        {
            il.Emit(OpCodes.Ldloc, cloneVariable);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, srcProperty.GetGetMethod());
            il.Emit(OpCodes.Call, destProperty.GetSetMethod());
        }

        /// <summary>
        /// Helper method to clone a reference type.
        /// This method clones IList and IEnumerables and other reference types (classes)
        /// Arrays are not yet supported (ex. string[])
        /// </summary>
        /// <param name="il">IL il to emit code to.</param>
        /// <param name="cloneVar">Local store wheren the clone object is located. (or child of)</param>
        /// <param name="srcProperty">Property definition of the reference type to clone.</param>
        private static void CopyReferenceType(ILGenerator il, LocalBuilder cloneVar, PropertyInfo source, PropertyInfo destination)
        {
            // does not copy a delegate
            if (source.PropertyType.IsSubclassOf(typeof(Delegate))) { return; }

            il.Emit(OpCodes.Ldloc, cloneVar);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, source.GetGetMethod());
            il.Emit(OpCodes.Call, 
                GetCorrectClonningMethod(source.PropertyType, destination.PropertyType));
            il.Emit(OpCodes.Call, destination.GetSetMethod());
        }

        /// <summary>
        /// Returns the type of cloning to apply on a certain srcProperty when in custom mode.
        /// Otherwise the main cloning method is returned.
        /// You can invoke custom mode by invoking the method Clone(T obj)
        /// </summary>
        /// <param name="srcProperty">Property to examine</param>
        /// <returns>Type of cloning to use for this srcProperty.</returns>
        private static Inspection GetCloneTypeForProperty(PropertyInfo prop)
        {
            var attributes =
                prop.GetCustomAttributes(typeof(CloneAttribute), true);

            return attributes != null && attributes.Length > 0 ?
                (attributes[0] as CloneAttribute).InspectionType :
                Inspection.Deep;
        }

        private static string[] GetAliasesForProperty(PropertyInfo prop)
        {
            var attributes =
                prop.GetCustomAttributes(typeof(CloneAttribute), true);

            return attributes != null && attributes.Length > 0 ?
                (attributes[0] as CloneAttribute).Aliases :
                null;
        }

        private static PropertyInfo[] GetProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        internal static MethodInfo GetCorrectClonningMethod(Type source, Type destination)
        {
            if (!typeof(IEnumerable).IsAssignableFrom(source) &&
                !typeof(IEnumerable).IsAssignableFrom(destination) &&
                !destination.IsArray &&
                !source.IsArray)
            {
                return typeof(Pass)
                    .GetMethod("On", new Type[] { typeof(object) })
                    .MakeGenericMethod(destination);
            }

            var srcItem =
                source.IsArray ? source.GetElementType() :
                source.IsGenericType ? source.GetGenericArguments()[0] :
                null;

            var destType =
                destination.IsArray ? destination.GetElementType() :
                destination.IsGenericType ? destination.GetGenericArguments()[0] :
                null;

            if (srcItem == null || destType == null)
            {
                throw new NotSupportedException();
            }

            var cloneType = typeof(Pass.ACollectionOf<>)
                .MakeGenericType(srcItem);

            return destination.IsArray ?
                cloneType.GetToArrayOfMethod(source, destType) :
                cloneType.GetToListOfMethod(source, destType);
        }

        internal static Ret MergeWithILDeep<Src, Ret>(Src source, Ret destination)
        {
            //if(source == null || destination == null || typeof(Ret) == null)
            //{
            //    if (typeof(Ret) == null)
            //    {
            //        if (source != null || destination != null)
            //        {
            //            typeof(Ret) =
            //                destination == null ?
            //                destination.GetType() :
            //                typeof(Src);
            //        }
            //        else { throw new ArgumentNullException("There are no means to infer the returned type. All arguments are null"); }
            //    }

            //    if (destination == null)
            //    { return CloneObjectWithILDeep(typeof(Ret), source); } 

            //    if (source == null) 
            //    { return CloneObjectWithILDeep(typeof(Ret), destination);  }            
            //}

            if (destination == null)
            { return CloneObjectWithILDeep<Src, Ret>(source); }

            if (source == null)
            { return CloneObjectWithILDeep<Ret, Ret>(destination); }

            Delegate mapper = null;

            var key =
                new Tuple<Type, Type, Type>(typeof(Ret), typeof(Src), destination.GetType());

            if (!_cachedILMerge.TryGetValue(key, out mapper))
            {
                // Create ILGenerator            
                DynamicMethod dymMethod = new DynamicMethod(
                    "DoDeepMerge",
                    destination.GetType(),
                    new Type[] { typeof(Src), destination.GetType() },
                    Assembly.GetExecutingAssembly().ManifestModule,
                    true);

                ILGenerator il = dymMethod.GetILGenerator();

                LocalBuilder cloneVariable = il.DeclareLocal(typeof(Ret));

                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stloc, cloneVariable);

                CopyProperties<Src, Ret>(il,
                    (src, dest) =>
                    {
                        var inspectionType = GetCloneTypeForProperty(src);

                        if (inspectionType == Inspection.Ignore) { return; }    

                        if (dest.PropertyType.IsAssignableFrom(src.PropertyType) 
                            && (
                                inspectionType == Inspection.Shallow
                                || src.PropertyType.IsValueType
                                || src.PropertyType == typeof(string)
                        ))
                        {
                            EmitPassOn(il, cloneVariable, src, dest);
                        }//Inspection.Deep
                        else if (src.PropertyType.IsClass)
                        {
                            CopyReferenceType(il, cloneVariable, src, dest);
                        }
                    });

                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ret);

                var delType = typeof(Func<,,>)
                    .MakeGenericType(typeof(Src), destination.GetType(), typeof(Ret) ?? destination.GetType());

                mapper = dymMethod.CreateDelegate(delType);
                _cachedILMerge.Add(key, mapper);
            }
            return ((Func<Src, Ret, Ret>)mapper)(source, destination);
        }

        /// <summary>
        /// Generic cloning method that clones an object using IL.
        /// Only the first call of a certain type will hold back performance.
        /// After the first call, the compiled IL is executed. 
        /// </summary>
        /// <param name="left">Type of object to clone</param>
        /// <returns>Cloned object (deeply cloned)</returns>
        internal static Ret CloneObjectWithILDeep<Src, Ret>(Src source)
        {
            if (source == null)
            { return (Ret) Activator.CreateInstance(typeof(Ret)); }

            var key = new Tuple<Type, Type>(
                typeof(Ret), typeof(Src));

            Delegate mapper = null;

            if (!_cachedILDeepClone.TryGetValue(key, out mapper))
            {
                // Create ILGenerator            
                var dymMethod = new DynamicMethod(
                    "DoDeepClone",
                    typeof(Ret),
                    new Type[] { typeof(Src) },
                    Assembly.GetExecutingAssembly().ManifestModule,
                    true);

                var il =
                    dymMethod.GetILGenerator();

                var cloneVariable =
                    il.DeclareLocal(typeof(Ret));

                Construct(typeof(Ret), il);

                il.Emit(OpCodes.Stloc, cloneVariable);

                CopyProperties<Src, Ret>(il,
                    (src, dest) =>
                    {
                        var inspectionType = GetCloneTypeForProperty(src);

                        if (inspectionType == Inspection.Ignore) { return; }

                        if ((dest.PropertyType.IsAssignableFrom(src.PropertyType) &&
                            (inspectionType == Inspection.Shallow ||
                            src.PropertyType.IsValueType ||
                            src.PropertyType == typeof(string))))
                        {
                            EmitPassOn(il, cloneVariable, src, dest);
                        }//Inspection.Deep
                        else if (src.PropertyType.IsClass || src.PropertyType.IsInterface)
                        {
                            CopyReferenceType(il, cloneVariable, src, dest);
                        }
                    }, ignoreType: true);

                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ret);

                var delType = typeof(Func<,>)
                    .MakeGenericType(typeof(Src), typeof(Ret));

                mapper = dymMethod.CreateDelegate(delType);
                _cachedILDeepClone.Add(key, mapper);
            }
            return ((Func<Src, Ret>)mapper)(source);
        }

        /// <summary>    
        /// Generic cloning method that clones an object using IL.    
        /// Only the first call of a certain type will hold back performance.    
        /// After the first call, the compiled IL is executed.    
        /// </summary>    
        /// <typeparam name="T">Type of object to clone</typeparam>    
        /// <param name="left">Object to clone</param>    
        /// <returns>Cloned object (shallow)</returns>    
        internal static Ret CloneObjectWithILShallow<Src, Ret>(Src source)
        {
            Delegate mapper = null;

            if (source == null)
            { return (Ret)Activator.CreateInstance(typeof(Ret)); }

            var key = new Tuple<Type, Type>(
                typeof(Ret), typeof(Src));

            if (!_cachedILShallowClone.TryGetValue(key, out mapper))
            {
                var dymMethod =
                    new DynamicMethod("DoShallowClone", typeof(Ret), new Type[] { typeof(Src) }, Assembly.GetExecutingAssembly().ManifestModule, true);

                var cInfo =
                    typeof(Ret).GetConstructor(new Type[] { });
                var il =
                    dymMethod.GetILGenerator();

                var local =
                    il.DeclareLocal(typeof(Src));

                il.Emit(OpCodes.Newobj, cInfo);
                il.Emit(OpCodes.Stloc_0);

                CopyProperties<Src, Ret>(il,
                    (src, dest) =>
                    {
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Call, src.GetGetMethod());
                        il.Emit(OpCodes.Call, dest.GetSetMethod());
                    });

                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ret);

                var delType = typeof(Func<,>)
                    .MakeGenericType(typeof(Src), typeof(Src));

                mapper = dymMethod.CreateDelegate(delType);
                _cachedILShallowClone.Add(key, mapper);
            }
            return ((Func<Src, Ret>)mapper)(source);
        }
    }
}