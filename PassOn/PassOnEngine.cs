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
        

        private static void CopyProperties(Type returnType, Type sourceType, ILGenerator il, Action<PropertyInfo, PropertyInfo> whenSame, bool ignoreType = false)
        {
            var destPropertys =
                GetProperties(returnType);

            foreach (var srcProperty in GetProperties(sourceType))
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

            Delegate myExec = null;

            var key =
                new Tuple<Type, Type, Type>(returnType, source.GetType(), destination.GetType());

            if (!_cachedILMerge.TryGetValue(key, out myExec))
            {
                // Create ILGenerator            
                DynamicMethod dymMethod = new DynamicMethod(
                    "DoDeepMerge",
                    destination.GetType(),
                    new Type[] { source.GetType(), destination.GetType() },
                    Assembly.GetExecutingAssembly().ManifestModule,
                    true);

                ILGenerator il = dymMethod.GetILGenerator();

                LocalBuilder cloneVariable = il.DeclareLocal(returnType);

                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Stloc, cloneVariable);

                CopyProperties(
                    destination.GetType(), 
                    source.GetType(), 
                    il,
                    (src, dest) =>
                    {
                        if ((dest.PropertyType.IsAssignableFrom(src.PropertyType) &&
                                         (GetCloneTypeForProperty(src) == Inspection.Shallow ||
                                         src.PropertyType.IsValueType ||
                                         src.PropertyType == typeof(string))))
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
                    .MakeGenericType(source.GetType(), destination.GetType(), returnType ?? destination.GetType());

                myExec = dymMethod.CreateDelegate(delType);
                _cachedILMerge.Add(key, myExec);
            }
            return myExec.DynamicInvoke(source, destination);
        }

        /// <summary>
        /// Generic cloning method that clones an object using IL.
        /// Only the first call of a certain type will hold back performance.
        /// After the first call, the compiled IL is executed. 
        /// </summary>
        /// <param name="left">Type of object to clone</param>
        /// <returns>Cloned object (deeply cloned)</returns>
        internal static object CloneObjectWithILDeep(Type returnType, object source)
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

            var key = new Tuple<Type, Type>(
                returnType, source.GetType());

            Delegate myExec = null;

            if (!_cachedILDeepClone.TryGetValue(key, out myExec))
            {
                // Create ILGenerator            
                var dymMethod = new DynamicMethod(
                    "DoDeepClone",
                    returnType,
                    new Type[] { source.GetType() },
                    Assembly.GetExecutingAssembly().ManifestModule,
                    true);

                var il =
                    dymMethod.GetILGenerator();

                var cloneVariable =
                    il.DeclareLocal(returnType);

                Construct(returnType, il);

                il.Emit(OpCodes.Stloc, cloneVariable);

                CopyProperties(returnType, source.GetType(), il,
                    (src, dest) =>
                    {
                        if ((dest.PropertyType.IsAssignableFrom(src.PropertyType) &&
                            (GetCloneTypeForProperty(src) == Inspection.Shallow ||
                            src.PropertyType.IsValueType ||
                            src.PropertyType == typeof(string))))
                        {
                            EmitPassOn(il, cloneVariable, src, dest);
                        }//Inspection.Deep
                        else if (src.PropertyType.IsClass)
                        {
                            CopyReferenceType(il, cloneVariable, src, dest);
                        }
                    }, ignoreType: true);

                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ret);

                var delType = typeof(Func<,>)
                    .MakeGenericType(source.GetType(), returnType);

                myExec = dymMethod.CreateDelegate(delType);
                _cachedILDeepClone.Add(key, myExec);
            }
            return myExec.DynamicInvoke(source);
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
            Delegate myExec = null;

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

            var key = new Tuple<Type, Type>(
                returnType, source.GetType());

            if (!_cachedILShallowClone.TryGetValue(key, out myExec))
            {
                var dymMethod =
                    new DynamicMethod("DoShallowClone", returnType, new Type[] { source.GetType() }, Assembly.GetExecutingAssembly().ManifestModule, true);

                var cInfo =
                    returnType.GetConstructor(new Type[] { });
                var il =
                    dymMethod.GetILGenerator();

                var local =
                    il.DeclareLocal(source.GetType());

                il.Emit(OpCodes.Newobj, cInfo);
                il.Emit(OpCodes.Stloc_0);

                CopyProperties(returnType, source.GetType(), il,
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
                    .MakeGenericType(source.GetType(), source.GetType());

                myExec = dymMethod.CreateDelegate(delType);
                _cachedILShallowClone.Add(key, myExec);
            }
            return myExec.DynamicInvoke(source);
        }
    }
}