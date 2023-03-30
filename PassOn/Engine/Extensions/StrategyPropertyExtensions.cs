using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Engine.Extensions
{
    internal static class StrategyPropertyExtensions
    {
        /// <summary>
        /// Returns the type of cloning to apply on a certain srcProperty when in custom mode.
        /// Otherwise the main cloning method is returned.
        /// You can invoke custom mode by invoking the method Clone(T obj)
        /// </summary>
        /// <param name="srcProperty">Property to examine</param>
        /// <returns>Type of cloning to use for this srcProperty.</returns>
        internal static MapStrategyAttribute GetCloneStrategy(this PropertyInfo prop)
        {
            var attributes =
                prop.GetCustomAttributes(typeof(MapStrategyAttribute), true);

            return attributes != null && attributes.Length > 0 ?
                (attributes[0] as MapStrategyAttribute) :
                null;
        }

        /// <summary>
        /// Returns the type of cloning to apply on a certain srcProperty when in custom mode.
        /// Otherwise the main cloning method is returned.
        /// You can invoke custom mode by invoking the method Clone(T obj)
        /// </summary>
        /// <param name="srcProperty">Property to examine</param>
        /// <returns>Type of cloning to use for this srcProperty.</returns>
        internal static Strategy GetStrategyType(this PropertyInfo prop)
        {
            return prop.GetCloneStrategy()?.Type
                ?? MapStrategyAttribute.DEFAULT_TYPE;
        }

        internal static string[] GetAliases(this PropertyInfo prop)
        {
            return prop.GetCloneStrategy()?.Aliases;
        }
    }
}
