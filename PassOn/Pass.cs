using PassOn.Utilities;
using System;
using System.Collections.Generic;

namespace PassOn
{
    public static class Pass
    {
        private static PassOnEngine engine = new PassOnEngine();

        public static class ACollectionOf<Source>
        {
            /// <summary>
            /// Maps or merges all the values of an IEnumerable to another list of different types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="enumerable">the source</param>
            /// <returns>An array of the target type</returns>
            public static List<Target> ToAListOf<Target>(IEnumerable<Source> enumerable)
            {
                if (enumerable == null)
                { return null; }

                var result = new List<Target>();

                foreach (var item in enumerable)
                {
                    result.Add(item.Map<Source, Target>());
                }

                return result;
            }

            /// <summary>
            /// Maps or merges all the values of an array to another list of different types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="array">left array</param>
            /// <returns>A list of the target type</returns>
            public static List<Target> ToAListOf<Target>(Source[] array)
                where Target : class
            {
                if (array == null) { return null; }

                var result = new List<Target>(array.Length);

                for (int i = 0; i < array.Length; i++)
                {
                    result.Add(array[i].Map<Source, Target>());
                }

                return result;
            }

            /// <summary>
            /// Maps or merges all the values of an left to another array of different types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="source">left</param>
            /// <returns>An array of the target type</returns>
            public static Target[] ToAnArrayOf<Target>(IEnumerable<Source> source)
            {
                if (source == null) { return null; }

                var result = new List<Target>();

                foreach (var item in source)
                {
                    result.Add(Pass.On<Source, Target>(item));
                }

                return result.ToArray();
            }

            /// <summary>
            /// Maps or merges all the values of an array to another array of different types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="array">left array</param>
            /// <returns>An array of the target type</returns>
            public static Target[] ToAnArrayOf<Target>(Source[] array)
                where Target : class
            {
                if (array == null) { return null; }

                var result = (Target[])Array.CreateInstance(
                    typeof(Target), array.Length);

                for (int i = 0; i < array.Length; i++)
                {
                    result[i] = array[i].Map<Source,Target>();
                }

                return result;
            }

            /// <summary>
            /// Maps or merges all the values of an left to another array of different, or similar types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="source">left</param>
            /// <returns>A list of the target type</returns>
            public static IList<Target> OntoAListOf<Target>(IEnumerable<Source> source, IEnumerable<Target> target)
            {
                var result = new List<Target>();

                IterateThrough.Both(
                    source,
                    target,
                    both: (src, dest) => result.Add(Pass.Onto<Source, Target>(src, dest)),
                    onlyLeft: (src) => result.Add(Pass.On<Source, Target>(src)),
                    onlyRight: (dest) => result.Add(dest));

                return result;
            }

            /// <summary>
            /// Maps or merges all the values of an left to another list of different, or similar types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="source">left</param>
            /// <returns>A list of the target type</returns>
            public static IList<Target> OntoAListOf<Target>(IEnumerable<Source> source, Target[] target)
            {
                var result = new List<Target>();

                IterateThrough.Both(
                    source,
                    target,
                    both: (src, i) => result.Add(Pass.Onto<Source, Target>(src, target[i])),
                    onlyLeft: (src) => result.Add(Pass.On<Source, Target>(src)),
                    onlyRight: (i) => result.Add(target[i]));

                return result;
            }

            /// <summary>
            /// Maps or merges all the values of an left to another array of different types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="source">left</param>
            /// <returns>An array of the target type</returns>
            public static Target[] OntoAnArrayOf<Target>(IEnumerable<Source> source, IEnumerable<Target> target)
            {
                if (source == null)
                { return System.Linq.Enumerable.ToArray(target); }

                var result = new List<Target>();

                IterateThrough.Both(
                    source,
                    target,
                    both: (src, dest) => result.Add(Pass.Onto<Source, Target>(src, dest)),
                    onlyLeft: (src) => result.Add(Pass.On<Source, Target>(src)),
                    onlyRight: (dest) => result.Add(dest));

                return result.ToArray();
            }

            /// <summary>
            /// Maps or merges all the values of an left to another array of different types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="source">left</param>
            /// <returns>An array of the target type</returns>
            public static Target[] OntoAnArrayOf<Target>(IEnumerable<Source> source, Target[] target)
            {
                var result = new List<Target>();

                IterateThrough.Both(
                    source,
                    target,
                    both: (src, i) => result.Add(Pass.Onto<Source, Target>(src, target[i])),
                    onlyLeft: (src) => result.Add(Pass.On<Source, Target>(src)),
                    onlyRight: (i) => result.Add(target[i]));

                return result.ToArray();
            }

            /// <summary>
            /// Maps or merges all the values of an left to another list of different, or similar types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="source">left</param>
            /// <returns>A list of the target type</returns>
            public static IList<Target> OntoAListOf<Target>(Source[] source, Target[] target)
            {
                if (source == null) { return new List<Target>(target); }

                var length = (target != null && target.Length > source.Length) ?
                    source.Length :
                    target.Length;

                var result = new List<Target>(length);

                IterateThrough.Both(
                    source,
                    target,
                    both: i => result.Add(Pass.Onto<Source, Target>(source[i], target[i])),
                    onlyLeft: i => result.Add(Pass.On<Source, Target>(source[i])),
                    onlyRight: i => result.Add(target[i]));

                return result;
            }

            /// <summary>
            /// Maps or merges all the values of an left to another array of different, or similar types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="source">left</param>
            /// <returns>A list of the target type</returns>
            public static IList<Target> OntoAListOf<Target>(Source[] source, IEnumerable<Target> target)
            {
                if (source == null) { return new List<Target>(Pass.On(target)); }

                var result = new List<Target>();

                IterateThrough.Both(
                    source,
                    target,
                    both: (i, dest) => result.Add(Pass.Onto<Source, Target>(source[i], dest)),
                    onlyLeft: i => result.Add(Pass.On<Source, Target>(source[i])),
                    onlyRight: dest => result.Add(Pass.On(dest)));

                return result;
            }

            /// <summary>
            /// Maps or merges all the values of an left to another array of different types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="source">left</param>
            /// <returns>An array of the target type</returns>
            public static Target[] OntoAnArrayOf<Target>(Source[] source, Target[] target)
            {
                if (source == null) { return target; }

                var result = (Target[])Array.CreateInstance(typeof(Target),
                    (target != null && target.Length > source.Length) ?
                    source.Length :
                    target.Length);

                IterateThrough.Both(
                    source,
                    target,
                    both: i => result[i] = Pass.Onto<Source, Target>(source[i], target[i]),
                    onlyLeft: i => result[i] = Pass.On<Source, Target>(source[i]),
                    onlyRight: i => result[i] = Pass.On(target[i]));

                return result;
            }

            /// <summary>
            /// Maps all the values of from the source to another array of target types
            /// </summary>
            /// <typeparam name="Target">The result type</typeparam>
            /// <param name="source">left</param>
            /// <returns>An array of the target type</returns>
            public static Target[] OntoAnArrayOf<Target>(Source[] source, IEnumerable<Target> target)
            {
                if (source == null)
                { return System.Linq.Enumerable.ToArray(Pass.On(target)); }

                var result = new List<Target>();

                IterateThrough.Both(
                    source,
                    target,
                    both: (i, dest) => result.Add(Pass.Onto<Source, Target>(source[i], dest)),
                    onlyLeft: i => result.Add(Pass.On<Source, Target>(source[i])),
                    onlyRight: dest => result.Add(Pass.On(dest)));

                return result.ToArray();
            }
        }

        /// <summary>
        /// Maps an object with Deep Cloning or with a custom strategy 
        /// such as Shallow and/or Deep combined (use the MapStrategyAttribute)
        /// </summary>
        /// <param name="input">Object to perform cloning on.</param>
        /// <returns>A new instance of the mapped object.</returns>
        public static Target On<Source, Target>(Source input)
        {
            return engine.MapObjectWithILDeep<Source, Target>(input);
        }

        /// <summary>
        /// Maps an object with one strategy (Deep or Shallow)
        /// </summary>
        /// <param name="input">Object to perform cloning on.</param>
        /// <param name="strategy">strategy for the mapping</param>
        /// <returns>A new instance of the mapped object, in this case a clone.</returns>
        /// <exception cref="InvalidOperationException">When a wrong enum for the strategy is passed.</exception>
        public static Target On<Source, Target>(Source input, Strategy strategy = Strategy.Deep)
        {
            return (strategy == Strategy.Shallow) ?
                engine.CloneObjectWithILShallow<Source, Target>(input) :
                engine.MapObjectWithILDeep<Source, Target>(input);
        }

        /// <summary>
        /// Maps an object with Deep Cloning or with a custom strategy such as Shallow and/or Deep combined (use the MapStrategyAttribute)
        /// </summary>
        /// <typeparam name="Target"></typeparam>
        /// <param name="input">Object to perform cloning on.</param>
        /// <returns>A new instance of the mapped object, in this case a clone.</returns>
        public static Target On<Target>(Target input)
        {
            return engine.MapObjectWithILDeep<Target, Target>(input);
        }

        /// <summary>
        /// Clone an object with one strategy (Deep or Shallow)
        /// </summary>
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
        /// Maps an object with Deep Cloning or with a custom strategy 
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
        /// Register, if not cached, and returns the mapping function 
        /// </summary>
        /// <typeparam name="Source"></typeparam>
        /// <typeparam name="Target"></typeparam>
        /// <returns>The mapping function</returns>
        public static Func<Source, Target> Mapper<Source, Target>()
        {
            return (Func<Source, Target>)engine.GetOrCreateMapper<Source, Target>();
        }

        /// <summary>
        /// Register, if not cached, and returns the mapping function  
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The mapping function</returns>
        public static Func<T, T> Mapper<T>()
        {
            return engine.GetOrCreateMapper<T, T>();
        }

        /// <summary>
        /// Register, if not cached, and returns the mapping function 
        /// </summary>
        /// <remarks>
        /// A shallow mapper moves the references (copy the pointer) and copies the primitives
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <returns>The mapping function</returns>
        public static Func<T, T> ShallowMapper<T>()
        {
            return (Func<T, T>)engine.GetOrCreateShallowMapper<T,T>();
        }

        /// <summary>
        /// Register, if not cached, and returns the mapping function.
        /// </summary>
        /// <remarks>
        /// A shallow mapper moves the references (copy the pointer) and copies the primitives
        /// </remarks>
        /// <typeparam name="Source"></typeparam>
        /// <typeparam name="Target"></typeparam>
        /// <returns>The mapping function</returns>
        public static Func<Source, Target> ShallowMapper<Source, Target>()
        {
            return (Func<Source, Target>)engine.GetOrCreateShallowMapper<Source, Target>();
        }

        public static void ClearCache() { engine.ClearCache(); }
    }
}