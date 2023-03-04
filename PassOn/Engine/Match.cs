using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.EngineExtensions
{
    internal static class Match
    {
        private static PropertyInfo[] GetProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        internal static void Properties<Source, Target>(Action<PropertyInfo, PropertyInfo> whenMatch, bool ignoreType = false)
        {
            var destProperties =
                GetProperties(typeof(Target));

            foreach (var srcProperty in GetProperties(typeof(Source)))
            {
                var srcAliases = srcProperty.GetAliases();

                srcAliases = srcAliases?.Length > 0 
                    ? srcAliases
                    : new[] { srcProperty.Name };

                foreach (var destProperty in destProperties)
                {                       
                    var destAliases = destProperty.GetAliases() ?? new[] { destProperty.Name };

                    var nameOrAliasMatch = destAliases
                        .Any(dest => Array.IndexOf(srcAliases, dest, 0) > -1);

                    if (!nameOrAliasMatch) { continue; }

                    var isAssignable = ignoreType 
                        || destProperty.PropertyType.IsAssignableFrom(srcProperty.PropertyType)
                        || srcProperty.PropertyType.IsAssignableFrom(destProperty.PropertyType);

                    if (isAssignable) 
                    { whenMatch(srcProperty, destProperty); }                    
                }
            }
        }

        private static (PropertyInfo, string[]) GetPropAndAliases(PropertyInfo[] properties, int index)
        {
            var property = index < properties.Length? properties[index]: null;
            var aliases = property?.GetAliases();

            return (property, aliases);
        }
    }

}
