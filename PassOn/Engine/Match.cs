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
            var destPropertys =
                GetProperties(typeof(Target));

            foreach (var srcProperty in GetProperties(typeof(Source)))
            {
                foreach (var destProperty in destPropertys)
                {
                    Func<bool> isAssignable = () =>
                        destProperty.PropertyType.IsAssignableFrom(srcProperty.PropertyType) ||
                        srcProperty.PropertyType.IsAssignableFrom(destProperty.PropertyType);

                    var srcAliases = srcProperty.GetAliases();
                    var destAliases = destProperty.GetAliases();

                    var namesMatch =
                        destProperty.Name.Equals(srcProperty.Name) ||
                        (srcAliases != null && Array.IndexOf(srcAliases, destProperty.Name, 0) > -1) ||
                        (destAliases != null && Array.IndexOf(destAliases, srcProperty.Name, 0) > -1);

                    if ((ignoreType || isAssignable()) && namesMatch)
                    {
                        whenMatch(srcProperty, destProperty);
                    }
                }
            }
        }
    }

}
