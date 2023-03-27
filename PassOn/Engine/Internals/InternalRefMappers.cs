using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Engine.Internals
{
    public class InternalRefMappers
    {
        public static Target MapObjectRawWithILDeepInternal<Source, Target>(Source source, PassOnEngine engine, int recursionIndex)
        {
            var mapper = engine.GetOrCreateInternalMapper<Source, Target>(raw: true);
            return mapper(source, engine, recursionIndex);
        }

        public static Target MapObjectWithILDeepInternal<Source, Target>(Source source, PassOnEngine engine, int recursionIndex)
        {
            var mapper = engine.GetOrCreateInternalMapper<Source, Target>();
            return mapper(source, engine, recursionIndex);
        }


        /// <summary>
        /// Merges or merges all the values of an IEnumerable to another list of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="enumerable">the source</param>
        /// <returns>An array of the target type</returns>
        public static IEnumerable<Target> MapIEnumerableToIEnumerable<Source, Target>(IEnumerable<Source> enumerable, PassOnEngine engine, int recursionIndex)
        {
            if (enumerable == null)
            { yield break; }

            var mapper = engine.GetOrCreateInternalMapper<Source, Target>();

            foreach (var item in enumerable)
            {
                yield return mapper(item, engine, recursionIndex);
            }

            yield break;
        }

        /// <summary>
        /// Maps or merges all the values of an array to another list of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="array">left array</param>
        /// <returns>A list of the target type</returns>
        public static IEnumerable<Target> MapArrayToIEnumerable<Source, Target>(Source[] array, PassOnEngine engine, int recursionIndex)
            where Target : class
        {
            return MapIEnumerableToIEnumerable<Source, Target>(
                array, engine, recursionIndex
            );
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
            where Target : class
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
            where Target : class
        {
            return MapIEnumerableToIEnumerable<Source, Target>(
                array, engine, recursionIndex
            ).ToArray();
        }
    }
}
