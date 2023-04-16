using PassOn.Engine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PassOn
{
    public static class Pass
    {
        private static MapperEngine engine = new MapperEngine();

        public static class ACollectionOf<Source>
        {
            /// <summary>
            /// Maps or merges all the values of an IEnumerable to another list of different types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="enumerable">the source</param>
            /// <returns>An array of the target type</returns>
            public static IList<Target> ToAListOf<Target>(IEnumerable<Source> source)
            {
                return engine
                   .MapObjectWithILDeep<IEnumerable<Source>, IList<Target>>(source);
            }

            /// <summary>
            /// Maps or merges all the values of an array to another list of different types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="array">left array</param>
            /// <returns>A list of the target type</returns>
            public static IList<Target> ToAListOf<Target>(Source[] source)
                where Target : class
            {
                return engine
                   .MapObjectWithILDeep<Source[], Target[]>(source)
                   .ToList();
            }

            /// <summary>
            /// Maps or merges all the values of an left to another array of different types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="source">left</param>
            /// <returns>An array of the target type</returns>
            public static Target[] ToAnArrayOf<Target>(IEnumerable<Source> source)
            {
                return engine
                   .MapObjectWithILDeep<IEnumerable<Source>, Target[]>(source)
                   .ToArray();
            }

            /// <summary>
            /// Maps or merges all the values of an array to another array of different types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="array">left array</param>
            /// <returns>An array of the target type</returns>
            public static Target[] ToAnArrayOf<Target>(Source[] source)
                where Target : class
            {
                return engine
                   .MapObjectWithILDeep<Source[], Target[]>(source)
                   .ToArray();
            }

            /// <summary>
            /// Maps or merges all the values of an left to another array of different, or similar types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="source">left</param>
            /// <returns>A list of the target type</returns>
            public static IList<Target> OntoAListOf<Target>(IEnumerable<Source> source, IEnumerable<Target> target)
            {
                return engine
                   .MergeWithILDeep(source, target)
                   .ToList();
            }

            /// <summary>
            /// Maps or merges all the values of an left to another list of different, or similar types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="source">left</param>
            /// <returns>A list of the target type</returns>
            public static IList<Target> OntoAListOf<Target>(IEnumerable<Source> source, Target[] target)
            {
                return engine
                   .MergeWithILDeep(source, target)
                   .ToList();
            }

            /// <summary>
            /// Maps or merges all the values of an left to another array of different types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="source">left</param>
            /// <returns>An array of the target type</returns>
            public static Target[] OntoAnArrayOf<Target>(IEnumerable<Source> source, IEnumerable<Target> target)
            {
                return engine
                   .MergeWithILDeep(source, target)
                   .ToArray();
            }

            /// <summary>
            /// Maps or merges all the values of an left to another array of different types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="source">left</param>
            /// <returns>An array of the target type</returns>
            public static Target[] OntoAnArrayOf<Target>(IEnumerable<Source> source, Target[] target)
            {
                return engine
                   .MergeWithILDeep(source, target)
                   .ToArray();
            }

            /// <summary>
            /// Maps or merges all the values of an left to another list of different, or similar types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="source">left</param>
            /// <returns>A list of the target type</returns>
            public static IList<Target> OntoAListOf<Target>(Source[] source, Target[] target)
            {
                return engine
                   .MergeWithILDeep(source, target)
                   .ToList();
            }

            /// <summary>
            /// Maps or merges all the values of an left to another array of different, or similar types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="source">left</param>
            /// <returns>A list of the target type</returns>
            public static IList<Target> OntoAListOf<Target>(Source[] source, IEnumerable<Target> target)
            {
                return engine
                   .MergeWithILDeep(source, target)
                   .ToList();
            }

            /// <summary>
            /// Maps or merges all the values of an left to another array of different types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="source">left</param>
            /// <returns>An array of the target type</returns>
            public static Target[] OntoAnArrayOf<Target>(Source[] source, Target[] target)
            {
                return engine.MergeWithILDeep(source, target);
            }

            /// <summary>
            /// Maps all the values of from the source to another array of target types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="source">left</param>
            /// <returns>An array of the target type</returns>
            public static Target[] OntoAnArrayOf<Target>(Source[] source, IEnumerable<Target> target)
            {
                return engine
                    .MergeWithILDeep(source, target)
                    .ToArray();
            }
        }

        /// <summary>
        /// Maps an object deeply or with mixed custom strategy.         
        /// </summary>
        /// <remarks>
        /// To mix "Shallow", "Deep", "Custom" and "Ignore" strategies on the model, use the MapStrategyAttribute
        ///</remarks>
        /// <param name="input">The source object</param>
        /// <returns>A new mapped instance of the object.</returns>
        public static Target On<Source, Target>(Source input)
        {
            return engine.MapObjectWithILDeep<Source, Target>(input);
        }

        /// <summary>
        /// Maps an object deeply or with mixed custom strategy.         
        /// </summary>
        /// <remarks>
        /// To mix "Shallow", "Deep", "Custom" and "Ignore" strategies on the model, use the MapStrategyAttribute
        /// </remarks>
        /// <param name="input">The source object</param>
        /// <param name="strategy">An mapping strategy that overrides the first level</param>
        /// <returns>A new mapped instance of the object.</returns>
        /// <exception cref="InvalidOperationException">When a wrong enum for the strategy is passed.</exception>
        public static Target On<Source, Target>(Source input, Strategy strategy = Strategy.Deep)
        {
            if (strategy == Strategy.Shallow) {
                return engine.CloneObjectWithILShallow<Source, Target>(input);
            }

            if (strategy == Strategy.Deep)
            {
                return engine.MapObjectWithILDeep<Source, Target>(input);
            }

            throw new InvalidOperationException($"The \"{strategy}\" strategy is invalid for this scope!");
        }

        /// <summary>
        /// Maps an object deeply or with mixed custom strategy.         
        /// </summary>
        /// <remarks>
        /// To mix "Shallow", "Deep", "Custom" and "Ignore" strategies on the model, use the MapStrategyAttribute
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="input">Object to perform cloning on.</param>
        /// <returns>A new instance of the mapped object, in this case a clone.</returns>
        public static T On<T>(T input)
        {
            return engine.MapObjectWithILDeep<T, T>(input);
        }

        /// <summary>
        /// Maps an object deeply or with mixed custom strategy.         
        /// </summary>
        /// <remarks>
        /// To mix "Shallow", "Deep", "Custom" and "Ignore" strategies on the model, use the MapStrategyAttribute
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="input">Object to perform cloning on.</param>
        /// <param name="strategy">strategy for the mapping</param>
        /// <returns>A new instance of the mapped object.</returns>
        /// <exception cref="InvalidOperationException">When a wrong enum for the strategy is passed.</exception>
        public static T On<T>(T input, Strategy strategy = Strategy.Deep)
        {
            return (strategy == Strategy.Shallow) ?
                engine.CloneObjectWithILShallow<T, T>(input) :
                engine.MapObjectWithILDeep<T, T>(input);
        }

        /// <summary>
        /// Merges an object with Deep mapping or with a custom strategy 
        /// such as Shallow and/or Deep combined (use the MapStrategyAttribute)
        /// </summary>
        /// <typeparam name="Overlap"></typeparam>
        /// <typeparam name="Target"></typeparam>
        /// <param name="overlap"></param>
        /// <param name="target"></param>
        /// <returns>A new target instance with the values of the target overriden by the overlap</returns>
        public static Target Onto<Overlap, Target>(Overlap source, Target target)
        {
            return engine.MergeWithILDeep(source, target);
        }

        /// <summary>
        /// Maps the values from a source to a target of a same type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Overlap"></param>
        /// <param name="target""></param>
        /// <returns>A new instance with the values of the target overriden by the overlap</returns>
        public static T Onto<T>(T overlap, T target)
        {
            return engine.MergeWithILDeep(overlap, target);
        }

        /// <summary>
        /// Registers (if not already) a mapping function and returns it.
        /// </summary>
        /// <typeparam name="Source"></typeparam>
        /// <typeparam name="Target"></typeparam>
        /// <returns>The mapping function</returns>
        public static Func<Source, Target> Mapper<Source, Target>()
        {
            return engine.GetOrCreateMapper<Source, Target>();
        }

        /// <summary>
        /// Registers (if not already) a mapping function and returns it. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The mapping function</returns>
        public static Func<T, T> Mapper<T>()
        {
            return engine.GetOrCreateMapper<T, T>();
        }

        /// <summary>
        /// Registers (if not already) a mapping function and returns it.
        /// </summary>
        /// <remarks>
        /// A shallow mapper moves the references (copy the pointer) and copies the primitives
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <returns>The mapping function</returns>
        public static Func<T, T> ShallowMapper<T>()
        {
            return engine.GetOrCreateShallowMapper<T,T>();
        }

        /// <summary>
        /// Registers (if not already) a mapping function and returns it.
        /// </summary>
        /// <remarks>
        /// A shallow mapper moves the references (copy the pointer) and copies the primitives
        /// </remarks>
        /// <typeparam name="Source"></typeparam>
        /// <typeparam name="Target"></typeparam>
        /// <returns>The mapping function</returns>
        public static Func<Source, Target> ShallowMapper<Source, Target>()
        {
            return engine.GetOrCreateShallowMapper<Source, Target>();
        }

        public static void ClearCache() { engine.ClearCache(); }
    }
}