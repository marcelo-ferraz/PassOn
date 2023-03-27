using System;
using System.Collections.Generic;

namespace PassOn
{
    public static class PassOnMixins
    {
        /// <summary>
        /// Maps or merges all the values of an left to another array of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="source">left</param>
        /// <returns>An array of the target type</returns>
        public static Target[] MapToAnArrayOf<Source, Target>(this Source[] array)
            where Target : class
            where Source : class
        {
            return Pass.ACollectionOf<Source>.ToAnArrayOf<Target>(array);
        }

        /// <summary>
        /// Maps or merges all the values of an left to another array of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="source">left</param>
        /// <returns>An array of the target type</returns>
        public static Target[] MapToAnArrayOf<Source, Target>(this IEnumerable<Source> enumerable)
            where Target : class
            where Source : class
        {
            return Pass.ACollectionOf<Source>.ToAnArrayOf<Target>(enumerable);
        }

        /// <summary>
        /// Maps or merges all the values of an array to another list of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="array">left array</param>
        /// <returns>A list of the target type</returns>
        public static IList<Target> MapToAListOf<Source, Target>(this Source[] array)
            where Target : class
            where Source : class
        {
            return Pass.ACollectionOf<Source>.ToAListOf<Target>(array);
        }

        /// <summary>
        /// Maps or merges all the values of an IEnumerable to another list of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="enumerable">the source</param>
        /// <returns>An array of the target type</returns>
        public static IList<Target> MapToAListOf<Source, Target>(this IEnumerable<Source> enumerable)
        {
            return Pass.ACollectionOf<Source>.ToAListOf<Target>(enumerable);
        }

        /// <summary>
        /// Maps an object with Deep Cloning or with a custom strategy such as Shallow and/or Deep combined (use the MapStrategyAttribute)
        /// </summary>
        /// <typeparam name="Target"></typeparam>
        /// <param name="input">Object to perform cloning on.</param>
        /// <returns>A new instance of the mapped object, in this case a clone.</returns>
        public static Target Map<Target>(this Target obj)            
        {
            return Pass.On<Target>(obj);
        }

        /// <summary>
        /// Maps an object with one strategy (Deep or Shallow)
        /// </summary>
        /// <param name="input">Object to perform cloning on.</param>
        /// <param name="strategy">strategy for the mapping</param>
        /// <returns>A new instance of the mapped object, in this case a clone.</returns>
        /// <exception cref="InvalidOperationException">When a wrong enum for the strategy is passed.</exception>
        public static Target Map<Target>(this Target obj, Strategy cloneType = Strategy.Deep)            
        {
            return Pass.On<Target, Target>(obj, cloneType); 
        }

        /// <summary>
        /// Maps an object with Deep Cloning or with a custom strategy such as Shallow and/or Deep combined (use the MapStrategyAttribute)
        /// </summary>
        /// <typeparam name="Target"></typeparam>
        /// <param name="input">Object to perform cloning on.</param>
        /// <returns>A new instance of the mapped object, in this case a clone.</returns>
        public static Target Map<Source, Target>(this Source obj, Strategy cloneType = Strategy.Deep)
        {
            return Pass.On<Source, Target>(obj, cloneType); 
        }

        /// <summary>
        /// Maps the values from a source to a target of a same type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Overlap"></param>
        /// <param name="target""></param>
        /// <returns>A new instance with the values of the target overriden by the overlap</returns>
        public static Target Merge<Target>(this Target source, Target target)
        {
            return Pass.Onto<Target>(source, target);
        }

        /// <summary>
        /// Maps an object with Deep Cloning or with a custom strategy 
        /// such as Shallow and/or Deep combined (use the MapStrategyAttribute)
        /// </summary>
        /// <typeparam name="Overlap"></typeparam>
        /// <typeparam name="Target"></typeparam>
        /// <param name="overlap"></param>
        /// <param name="target"></param>
        /// <returns>A new target instance with the values of the target overriden by the overlap</returns>
        public static Target Merge<Source, Target>(this Source overlap, Target target)
        {
            return Pass.Onto<Source, Target>(overlap, target);
        }

        /// <summary>
        /// Maps or merges all the values of an left to another array of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="source">left</param>
        /// <returns>An array of the target type</returns>
        public static Target[] MapToArray<Source, Target>(this IEnumerable<Source> source)
        {
            return Pass.ACollectionOf<Source>.ToAnArrayOf<Target>(source);
        }

        /// <summary>
        /// Maps or merges all the values of an left to another array of different types
        /// </summary>
        /// <typeparam name="Target">The result type</typeparam>
        /// <param name="source">left</param>
        /// <returns>An array of the target type</returns>
        public static Target[] MergeArrays<Source, Target>(this IEnumerable<Source> source, IEnumerable<Target> target)
        {
            return Pass.ACollectionOf<Source>.OntoAnArrayOf<Target>(source, target);
        }
    }
}
