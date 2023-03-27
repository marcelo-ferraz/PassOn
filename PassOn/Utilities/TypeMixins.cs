using System;
using System.Linq;
using System.Reflection;

namespace PassOn.Utilities
{
    public static class TypeMixins
    {
        internal static MethodInfo GetToListOfMethod(this Type type, Type sourceType, Type destType)
        {
            return GetPassMethod(
                type,
                "ToAListOf",
                sourceType,
                destType);
        }

        internal static MethodInfo GetToArrayOfMethod(this Type type, Type sourceType, Type destType)
        {
            return GetPassMethod(
                type,
                "ToAnArrayOf",
                sourceType,
                destType);
        }

        internal static MethodInfo GetPassMethod(this Type type,
            string name,
            Type sourceType,
            Type destType)
        {
            foreach (var method in type.GetMethods())
            {
                if (method.Name != name) { continue; }

                if (!method.IsGenericMethod && destType != null) { continue; }

                var args = method.GetParameters();

                if (args.Length == 0 || args.Length > 2) { continue; }

                var genArgs = method.GetGenericArguments();

                var firstArgCanBeAssigned = 
                    sourceType.IsAssignable(args[0])
                    || ArgTypeIsGenericFromFunc(method, args[0], genArgs);

                if (firstArgCanBeAssigned) {
                    return method.MakeGenericMethod(
                        genArgs.Length == 1 
                        ? new[] { destType } : 
                        new[] { sourceType, destType }                        
                    );
                }


                //bool argIsAssignable = sourceType.IsAssignable(args[0]);

                //var argGenericBoundToMethod = args.Any(
                //    a =>
                //    {
                //        return genArgs.Contains(a.ParameterType);

                //    });

                //if (argIsAssignable || argGenericBoundToMethod)
                //{ return method.MakeGenericMethod(new[] { destType }); }
            }
            return null;
        }

        private static bool ArgTypeIsGenericFromFunc(MethodInfo method, ParameterInfo arg, Type[] genArgs)
        {
            return arg.ParameterType.IsGenericParameter
                && method.ContainsGenericParameters
                && genArgs.Contains(arg.ParameterType);
        }

        private static bool IsAssignable(this Type sourceType, ParameterInfo arg)
        {
            return sourceType == arg.ParameterType
                || arg.ParameterType.IsAssignableFrom(sourceType);
        }
    }
}
