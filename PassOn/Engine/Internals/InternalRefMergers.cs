using PassOn.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Engine.Internals
{
    public class InternalRefMergers
    {
        /// <summary>
        /// Merges the values onto another object
        /// </summary>
        /// <typeparam name="Source"></typeparam>        
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="engine"></param>
        /// <param name="recursionIndex"></param>
        /// <returns></returns>
        public static Target MergeObjectWithILDeepInternal<Source, Target>(Source source, Target target, MapperEngine engine, int recursionIndex)
        {
            var merger = engine.GetOrCreateInternalMerger<Source, Target>();
            return merger(source, target, engine, recursionIndex);
        }


        /// <summary>
        /// Merges all the values of an IEnumerable to another list of different types
        /// </summary>
        /// <typeparam name="Source"></typeparam>        
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="enumerable">the source</param>
        /// <returns>An array of the target type</returns>
        public static IEnumerable<Target> MergeIEnumerableToIEnumerable<Source, Target>(IEnumerable<Source> source, IEnumerable<Target> target, MapperEngine engine, int recursionIndex)
            where Source : class
            where Target : class
        {
            if (source == null)
            { yield break; }

            var merge = engine.GetOrCreateInternalMerger<Source, Target>();
            
            Func<Source, MapperEngine, int, Target> mapSrc = null;
            Func<Target, MapperEngine, int, Target> mapTgt = null;

            foreach (var (srcItem, tgtItem) in (source, target).Zip())
            {
                if (srcItem != null && tgtItem != null)
                {
                    yield return merge(srcItem, tgtItem, engine, recursionIndex);
                    continue;
                }

                if (srcItem != null)
                {
                    mapSrc = mapSrc ?? engine.GetOrCreateInternalMapper<Source, Target>();
                    yield return mapSrc(srcItem, engine, recursionIndex);
                    continue;
                }

                if (srcItem != null)
                {
                    mapTgt = mapTgt ?? engine.GetOrCreateInternalMapper<Target, Target>();
                    yield return mapTgt(tgtItem, engine, recursionIndex);
                    continue;
                }
            }

            yield break;
        }

        /// <summary>
        /// Merges all the values of an array to another list of different types
        /// </summary>
        /// <typeparam name="Source"></typeparam>        
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="array">left array</param>
        /// <returns>A list of the target type</returns>
        public static IEnumerable<Target> MergeArrayToIEnumerable<Source, Target>(Source[] source, IEnumerable<Target> target, MapperEngine engine, int recursionIndex)
            where Source : class
            where Target : class
        {
            return MergeIEnumerableToIEnumerable<Source, Target>(
                source, target, engine, recursionIndex
            );
        }

        /// <summary>
        /// Merges all the values of an IEnumerable to another list of different types
        /// </summary>
        /// <typeparam name="Source"></typeparam>        
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="enumerable">the source</param>
        /// <returns>An array of the target type</returns>
        public static IList<Target> MergeIEnumerableToIList<Source, Target>(IEnumerable<Source> enumerable, IList<Target> target, MapperEngine engine, int recursionIndex)
            where Source : class
            where Target : class
        {
            return MergeIEnumerableToIEnumerable<Source, Target>(
                enumerable, target, engine, recursionIndex
            ).ToList();
        }

        /// <summary>
        /// Merges all the values of an array to another list of different types
        /// </summary>
        /// <typeparam name="Source"></typeparam>        
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="array">left array</param>
        /// <returns>A list of the target type</returns>
        public static IList<Target> MergeArrayToIList<Source, Target>(Source[] array, IList<Target> target, MapperEngine engine, int recursionIndex)
            where Source : class
            where Target : class
        {
            return MergeIEnumerableToIEnumerable<Source, Target>(
                array, target, engine, recursionIndex
            ).ToList();
        }

        /// <summary>
        /// Merges all the values of an left to another array of different types
        /// </summary>
        /// <typeparam name="Source"></typeparam>        
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="source">left</param>
        /// <returns>An array of the target type</returns>
        public static Target[] MergeIEnumerableToArray<Source, Target>(IEnumerable<Source> source, Target[] target, MapperEngine engine, int recursionIndex)
            where Source : class
            where Target : class
        {
            return MergeIEnumerableToIEnumerable<Source, Target>(
                source, target, engine, recursionIndex
            ).ToArray();
        }

        /// <summary>
        /// Merges all the values of an array to another array of different types
        /// </summary>
        /// <typeparam name="Source"></typeparam>        
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="array">left array</param>
        /// <returns>An array of the target type</returns>
        public static Target[] MergeArrayToArray<Source, Target>(Source[] array, Target[] target, MapperEngine engine, int recursionIndex)
            where Source : class
            where Target : class
        {
            return MergeIEnumerableToIEnumerable<Source, Target>(
                array, target, engine, recursionIndex
            ).ToArray();
        }
    }
}
