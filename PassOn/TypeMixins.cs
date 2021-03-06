﻿using System;
using System.Reflection;

namespace PassOn
{
    public static class TypeMixins
    {
        public static MethodInfo GetToListOfMethod(this Type type, Type sourceType, Type destType)
        {
            return GetMethod(
                type,
                "ToAListOf",
                sourceType,
                destType);
        }

        public static MethodInfo GetToArrayOfMethod(this Type type, Type sourceType, Type destType)
        {
            return GetMethod(
                type,
                "ToAnArrayOf",
                sourceType,
                destType);
        }

        public static MethodInfo GetMethod(this Type type,
            string name,
            Type sourceType,
            Type destType)
        {
            foreach (var method in type.GetMethods())
            {
                if (method.Name != name) { continue; }

                if (!method.IsGenericMethod && destType != null) { continue; }

                if (sourceType == method.GetParameters()[0].ParameterType || method.GetParameters()[0].ParameterType.IsAssignableFrom(sourceType))
                {
                    return method.MakeGenericMethod(new[] { destType });
                }
            }
            return null;
        }
    }
}
