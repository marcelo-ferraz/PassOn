using PassOn.Utilities;
using System;
using System.Collections.Generic;

namespace PassOn
{
    public static class Pass
    {
        public static class ACollectionOf<T>
        {
            /// <summary>
            /// Passes all the values of an IEnumerable to another list of different types
            /// </summary>
            /// <typeparam name="R">The result type</typeparam>
            /// <param name="array">left array</param>
            /// <returns>returns an array of that type</returns>
            public static List<R> ToAListOf<R>(IEnumerable<T> enumerable)
            {
                if (enumerable == null)
                { return null; }

                var result = new List<R>();

                foreach (var item in enumerable)
                {
                    result.Add(item.To<R>());
                }

                return result;
            }

            /// <summary>
            /// Passes all the values of an array to another list of different types
            /// </summary>
            /// <typeparam name="R">The result type</typeparam>
            /// <param name="array">left array</param>
            /// <returns>returns an array of that type</returns>
            public static List<R> ToAListOf<R>(T[] array)
                where R : class
            {
                if (array == null) { return null; }

                var result = new List<R>(array.Length);

                for (int i = 0; i < array.Length; i++)
                {
                    result.Add(array[i].To<R>());
                }

                return result;
            }

            /// <summary>
            /// Passes all the values of an left to another array of different types
            /// </summary>
            /// <typeparam name="R">The result type</typeparam>
            /// <param name="left">left</param>
            /// <returns>returns an array of that type</returns>
            public static R[] ToAnArrayOf<R>(IEnumerable<T> source)
            {
                if (source == null) { return null; }

                var result = new List<R>();

                foreach (var item in source)
                {
                    result.Add(Pass.On<R>(item));
                }

                return result.ToArray();
            }

            /// <summary>
            /// Passes all the values of an array to another array of different types
            /// </summary>
            /// <typeparam name="R">The result type</typeparam>
            /// <param name="array">left array</param>
            /// <returns>returns an array of that type</returns>
            public static R[] ToAnArrayOf<R>(T[] array)
                where R : class
            {
                if (array == null) { return null; }

                var result = (R[])Array.CreateInstance(
                    typeof(R), array.Length);

                for (int i = 0; i < array.Length; i++)
                {
                    result[i] = array[i].To<R>();
                }

                return result;
            }

            /// <summary>
            /// Passes all the values of an left to another array of different, or similar types
            /// </summary>
            /// <typeparam name="R">The result type</typeparam>
            /// <param name="left">left</param>
            /// <returns>returns an array of that type</returns>
            public static IList<R> OntoAListOf<R>(IEnumerable<T> source, IEnumerable<R> destination)
            {
                var result = new List<R>();

                IterateThrough.Both(
                    source,
                    destination,
                    both: (src, dest) => result.Add(Pass.Onto<R>(src, dest)),
                    onlyLeft: (src) => result.Add(Pass.On<R>(src)),
                    onlyRight: (dest) => result.Add(dest));

                return result;
            }

            /// <summary>
            /// Passes all the values of an left to another list of different, or similar types
            /// </summary>
            /// <typeparam name="R">The result type</typeparam>
            /// <param name="left">left</param>
            /// <returns>returns an array of that type</returns>
            public static IList<R> OntoAListOf<R>(IEnumerable<T> source, R[] destination)
            {
                var result = new List<R>();

                IterateThrough.Both(
                    source,
                    destination,
                    both: (src, i) => result.Add(Pass.Onto<R>(src, destination[i])),
                    onlyLeft: (src) => result.Add(Pass.On<R>(src)),
                    onlyRight: (i) => result.Add(destination[i]));

                return result;
            }

            /// <summary>
            /// Passes all the values of an left to another array of different types
            /// </summary>
            /// <typeparam name="R">The result type</typeparam>
            /// <param name="left">left</param>
            /// <returns>returns an array of that type</returns>
            public static R[] OntoAnArrayOf<R>(IEnumerable<T> source, IEnumerable<R> destination)
            {
                if (source == null)
                { return System.Linq.Enumerable.ToArray(destination); }

                var result = new List<R>();

                IterateThrough.Both(
                    source,
                    destination,
                    both: (src, dest) => result.Add(Pass.Onto<R>(src, dest)),
                    onlyLeft: (src) => result.Add(Pass.On<R>(src)),
                    onlyRight: (dest) => result.Add(dest));

                return result.ToArray();
            }

            /// <summary>
            /// Passes all the values of an left to another array of different types
            /// </summary>
            /// <typeparam name="R">The result type</typeparam>
            /// <param name="left">left</param>
            /// <returns>returns an array of that type</returns>
            public static R[] OntoAnArrayOf<R>(IEnumerable<T> source, R[] destination)
            {
                var result = new List<R>();

                IterateThrough.Both(
                    source,
                    destination,
                    both: (src, i) => result.Add(Pass.Onto<R>(src, destination[i])),
                    onlyLeft: (src) => result.Add(Pass.On<R>(src)),
                    onlyRight: (i) => result.Add(destination[i]));

                return result.ToArray();
            }

            /// <summary>
            /// Passes all the values of an left to another list of different, or similar types
            /// </summary>
            /// <typeparam name="R">The result type</typeparam>
            /// <param name="left">left</param>
            /// <returns>returns an array of that type</returns>
            public static IList<R> OntoAListOf<R>(T[] source, R[] destination)
            {
                if (source == null) { return new List<R>(destination); }

                var length = (destination != null && destination.Length > source.Length) ?
                    source.Length :
                    destination.Length;

                var result = new List<R>(length);

                IterateThrough.Both(
                    source,
                    destination,
                    both: i => result.Add(Pass.Onto<R>(source[i], destination[i])),
                    onlyLeft: i => result.Add(Pass.On<R>(source[i])),
                    onlyRight: i => result.Add(destination[i]));

                return result;
            }

            /// <summary>
            /// Passes all the values of an left to another array of different, or similar types
            /// </summary>
            /// <typeparam name="R">The result type</typeparam>
            /// <param name="left">left</param>
            /// <returns>returns an array of that type</returns>
            public static IList<R> OntoAListOf<R>(T[] source, IEnumerable<R> destination)
            {
                if (source == null) { return new List<R>(destination); }

                var result = new List<R>();

                IterateThrough.Both(
                    source,
                    destination,
                    both: (i, dest) => result.Add(Pass.Onto<R>(source[i], dest)),
                    onlyLeft: i => result.Add(Pass.On<R>(source[i])),
                    onlyRight: dest => result.Add(dest));

                return result;
            }

            /// <summary>
            /// Passes all the values of an left to another array of different types
            /// </summary>
            /// <typeparam name="R">The result type</typeparam>
            /// <param name="left">left</param>
            /// <returns>returns an array of that type</returns>
            public static R[] OntoAnArrayOf<R>(T[] source, R[] destination)
            {
                if (source == null) { return destination; }

                var result = (R[])Array.CreateInstance(typeof(R),
                    (destination != null && destination.Length > source.Length) ?
                    source.Length :
                    destination.Length);

                IterateThrough.Both(
                    source,
                    destination,
                    both: i => result[i] = Pass.Onto<R>(source[i], destination[i]),
                    onlyLeft: i => result[i] = Pass.On<R>(source[i]),
                    onlyRight: i => result[i] = destination[i]);

                return result;
            }

            /// <summary>
            /// Passes all the values of an left to another array of different types
            /// </summary>
            /// <typeparam name="R">The result type</typeparam>
            /// <param name="left">left</param>
            /// <returns>returns an array of that type</returns>
            public static R[] OntoAnArrayOf<R>(T[] source, IEnumerable<R> destination)
            {
                if (source == null)
                { return System.Linq.Enumerable.ToArray(destination); }

                var result = new List<R>();

                IterateThrough.Both(
                    source,
                    destination,
                    both: (i, dest) => result.Add(Pass.Onto<R>(source[i], dest)),
                    onlyLeft: i => result.Add(Pass.On<R>(source[i])),
                    onlyRight: dest => result.Add(dest));

                return result.ToArray();
            }
        }

        /// <summary>
        /// Clone an object with Deep Cloning or with a custom strategy 
        /// such as Shallow and/or Deep combined (use the CloneAttribute)
        /// </summary>
        /// <param name="obj">Object to perform cloning on.</param>
        /// <returns>Cloned object.</returns>
        public static object On(Type returnType, object obj)
        {
            return PassOnEngine.CloneObjectWithILDeep(returnType, obj);
        }

        /// <summary>
        /// Clone an object with one strategy (DeepClone or ShallowClone)
        /// </summary>
        /// <param name="obj">Object to perform cloning on.</param>
        /// <param name="inspectionType">Type of cloning</param>
        /// <returns>Cloned object.</returns>
        /// <exception cref="InvalidOperationException">When a wrong enum for cloningtype is passed.</exception>
        public static object On(Type returnType, object obj, Inspection cloneType = Inspection.Deep)
        {
            return (cloneType == Inspection.Shallow) ?
                PassOnEngine.CloneObjectWithILShallow(returnType, obj) :
                PassOnEngine.CloneObjectWithILDeep(returnType, obj);
        }

        /// <summary>
        /// Passes on, clone, an object with Deep Cloning or with a custom strategy 
        /// such as Shallow and/or Deep combined (use the CloneAttribute)
        /// </summary>
        /// <param name="obj">Object to perform cloning on.</param>
        /// <returns>Cloned object.</returns>
        public static R On<R>(object obj)
        {
            PassOnEngine.SaveMethod(typeof(R), obj);
            
            return obj.GetType().IsClass ?
                (R)PassOnEngine.CloneObjectWithILDeep(typeof(R), obj) :
                (R)PassOnEngine.CloneObjectWithILShallow(typeof(R), obj);
        }

        /// <summary>
        /// Passes on, clone, an object with Deep Cloning or with a custom strategy 
        /// such as Shallow and/or Deep combined (use the CloneAttribute)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public static object On(object source, object destination)
        {
            return PassOnEngine.MergeWithILDeep(source, destination);
        }

        /// <summary>
        /// Clone an object with one strategy (DeepClone or ShallowClone)
        /// </summary>
        /// <param name="obj">Object to perform cloning on.</param>
        /// <param name="inspectionType">Type of cloning</param>
        /// <returns>Cloned object.</returns>
        /// <exception cref="InvalidOperationException">When a wrong enum for cloningtype is passed.</exception>
        public static R On<R>(object obj, Inspection cloneType = Inspection.Deep)
        {
            return (cloneType == Inspection.Shallow) ?
                (R)PassOnEngine.CloneObjectWithILShallow(typeof(R), obj) :
                (R)PassOnEngine.CloneObjectWithILDeep(typeof(R), obj);
        }

        /// <summary>
        /// Passes the values from a left to a right of a different type
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static R Onto<R>(object source, R destination)
        {
            return (R)PassOnEngine.MergeWithILDeep(source, destination, typeof(R));
        }
    }
}