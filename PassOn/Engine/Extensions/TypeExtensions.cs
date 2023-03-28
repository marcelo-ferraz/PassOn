using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace PassOn.EngineExtensions
{
    internal static class TypeExtensions
    {
        private static ConcurrentDictionary<Type, Type> _interfaceToConcrete;

        static TypeExtensions() {
            _interfaceToConcrete = new ConcurrentDictionary<Type, Type>();
            _interfaceToConcrete.TryAdd(typeof(IList<>), typeof(List<>));
            _interfaceToConcrete.TryAdd(typeof(IList), typeof(ArrayList));
        }

        internal static void Construct<T>(this ILGenerator il, LocalBuilder resultLocal)
        {
            var type = TryInferType(typeof(T));

            if (type.IsArray) {
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Newarr, type.GetElementType());
                il.Emit(OpCodes.Stloc, resultLocal);
                return;
            }

            var cInfo = type.GetConstructor(Type.EmptyTypes);

            if (cInfo != null)
            {
                il.Emit(OpCodes.Newobj, cInfo);
                il.Emit(OpCodes.Stloc, resultLocal);
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
            il.Emit(OpCodes.Stloc, resultLocal);
        }

        internal static (Type, Type) GetCollectionItemTypes(this (Type, Type) tuple)
        {
            var (source, target) = tuple;

            var srcItemType = source.IsArray
                ? source.GetElementType()
                : source.GetGenericArguments()[0];

            var tgtItemType = target.IsArray
                ? target.GetElementType()
                : target.GetGenericArguments()[0];

            return (srcItemType, tgtItemType);
        }

        private static Type TryInferType(Type type)
        {
            if (!type.IsInterface && type.IsAbstract)
            { throw new NotSupportedException($"The result type ({type.Name}) is an abstract class. Abstract classes can't be constructed."); }

            if (!type.IsInterface) { return type; }

            if (type.IsGenericTypeDefinition)
            { throw new NotSupportedException($"The result type ({type.Name}) is a generic type definition. A generic class without a constraint is not supported."); }

            if (type.IsGenericType)
            {
                var key = type.GetGenericTypeDefinition();

                if (key == null)
                {
                    throw new InvalidOperationException($"Can't get the generic type definition of {type.Name}.");
                }

                if (!_interfaceToConcrete.TryGetValue(key, out Type concreteTypeDef))
                {
                    throw new InvalidOperationException($"The result type ({type.Name}) is an interface. Interfaces can't be constructed.");
                }

                var genArg = type.GetGenericArguments().FirstOrDefault();
                if (genArg == null)
                {
                    throw new InvalidOperationException($"Can't get the generic argument of {type.Name}.");
                }

                return concreteTypeDef.MakeGenericType(genArg);
            }

            if (!_interfaceToConcrete.TryGetValue(type, out Type concrete))
            {
                throw new InvalidOperationException($"Can't get the generic argument of {type.Name}.");
            }

            return concrete;
        }
    }
}
