using System;
using System.Linq;
using System.Reflection;

namespace PassOn.EngineExtensions
{
    internal static class Match
    {
        private static PropertyInfo[] GetProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public/* | BindingFlags.NonPublic*/ | BindingFlags.Instance);
        }

        internal static void PropertiesNoStrategy<Source, Target>(Action<PropertyInfo, PropertyInfo> whenMatch, bool ignoreType = false)
        {
            var destProperties =
                GetProperties(typeof(Target));

            foreach (var srcProperty in GetProperties(typeof(Source)))
            {
                foreach (var destProperty in destProperties)
                {                       
                    if (srcProperty.Name != destProperty.Name) { continue; }

                    var srcType = srcProperty.PropertyType;
                    var destType = destProperty.PropertyType;

                    var isAssignable = ignoreType
                        || (destType.IsClass && destType.IsClass)
                        || destType.IsAssignableFrom(srcType)
                        || srcType.IsAssignableFrom(destType);

                    if (isAssignable)
                    { whenMatch(srcProperty, destProperty); }
                }
            }
        }

        internal static void Properties<Source, Target>(Action<PropertyInfo, PropertyInfo> whenMatch, bool ignoreType = false)
        {
            var destProperties =
                GetProperties(typeof(Target));

            foreach (var srcProperty in GetProperties(typeof(Source)))
            {
                var srcStrategy = srcProperty.GetCloneStrategy();
                
                var srcAliases = srcStrategy?.Aliases;

                srcAliases = srcAliases?.Length > 0 
                    ? srcAliases
                    : new[] { srcProperty.Name };

                foreach (var destProperty in destProperties)
                {
                    var destStrategy = destProperty.GetCloneStrategy();
                    var destAliases = destStrategy?.Aliases;

                    destAliases = destAliases?.Length > 0
                        ? destAliases
                        : new[] { destProperty.Name };

                    var nameOrAliasMatch = destAliases
                        .Any(dest => Array.IndexOf(srcAliases, dest, 0) > -1);

                    if (!nameOrAliasMatch) { continue; }

                    var srcType = srcProperty.PropertyType;
                    var destType = destProperty.PropertyType;

                    if (srcStrategy?.Type == Strategy.CustomMap) {
                        srcType = StrategyUtilities
                            .GetMapperInfo<Source>(srcStrategy.Mapper, srcProperty.Name)
                            .ReturnType;
                    }

                    if (destStrategy?.Type == Strategy.CustomMap) {
                        var mapper = StrategyUtilities
                            .GetMapperInfo<Target>(destStrategy.Mapper, destProperty.Name);

                        destType = mapper.GetParameters()[0].ParameterType;                            
                    } 

                    var isAssignable = ignoreType 
                        || (destType.IsClass && destType.IsClass)
                        || destType.IsAssignableFrom(srcType)
                        || srcType.IsAssignableFrom(destType);

                    if (isAssignable) 
                    { whenMatch(srcProperty, destProperty); }                    
                }
            }
        }
    }
}
