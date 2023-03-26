using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Engine.Internals
{
    public class InternalValueMappers
    {
        public static IEnumerable<Target> MapIEnumerableToIEnumerable<Source, Target>(IEnumerable<Source> enumerable, PassOnEngine engine, int recursionIndex)
        {
            if (enumerable == null)
            { yield break; }

            foreach (var item in enumerable)
            {
                yield return (Target) (typeof(Target) == typeof(string)
                    ? (object) item.ToString()
                    : item);
            }

            yield break;
        }

        /// <summary>
        /// Maps or merges all the values of an IEnumerable to another list of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="enumerable">the source</param>
        /// <returns>An array of the target type</returns>
        public static IList<Target> MapIEnumerableToIList<Source, Target>(IEnumerable<Source> enumerable, PassOnEngine engine, int recursionIndex)            
        {
            return MapIEnumerableToIEnumerable<Source, Target>(
                enumerable, engine, recursionIndex
            ).ToList();
        }

        /// <summary>
        /// Maps or merges all the values of an array to another list of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="array">left array</param>
        /// <returns>A list of the target type</returns>
        public static IList<Target> MapArrayToIList<Source, Target>(Source[] array, PassOnEngine engine, int recursionIndex)
        {
            return MapIEnumerableToIEnumerable<Source, Target>(
                array, engine, recursionIndex
            ).ToList();
        }

        /// <summary>
        /// Maps or merges all the values of an left to another array of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="source">left</param>
        /// <returns>An array of the target type</returns>
        public static Target[] MapIEnumerableToArray<Source, Target>(IEnumerable<Source> source, PassOnEngine engine, int recursionIndex)
        {
            return MapIEnumerableToIEnumerable<Source, Target>(
                source, engine, recursionIndex
            ).ToArray();
        }

        /// <summary>
        /// Maps or merges all the values of an array to another array of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="array">left array</param>
        /// <returns>An array of the target type</returns>
        public static Target[] MapArrayToArray<Source, Target>(Source[] array, PassOnEngine engine, int recursionIndex)
        {
            return MapIEnumerableToIEnumerable<Source, Target>(
                array, engine, recursionIndex
            ).ToArray();
        }
    }

}
