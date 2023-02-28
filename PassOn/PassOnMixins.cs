using System;
using System.Collections.Generic;

namespace PassOn
{
    public static class PassOnMixins
    {
        public static R[] ToAnArrayOf<T, R>(this T[] array)
            where R : class
            where T : class
        {
            return Pass.ACollectionOf<T>.ToAnArrayOf<R>(array);
        }

        public static R[] ToAnArrayOf<T, R>(this IEnumerable<T> enumerable)
            where R : class
            where T : class
        {
            return Pass.ACollectionOf<T>.ToAnArrayOf<R>(enumerable);
        }

        public static IList<R> ToAListOf<T, R>(this T[] array)
            where R : class
            where T : class
        {
            return Pass.ACollectionOf<T>.ToAListOf<R>(array);
        }

        public static IList<R> ToAListOf<T, R>(this IEnumerable<T> enumerable)
        {
            return Pass.ACollectionOf<T>.ToAListOf<R>(enumerable);
        }

        /// <summary>
        /// Clone an object with Deep Cloning or with a custom strategy 
        /// such as Shallow and/or Deep combined (use the CloneAttribute)
        /// </summary>
        /// <param name="obj">Object to perform cloning on.</param>
        /// <returns>Cloned object.</returns>
        public static R To<R>(this R obj)            
        {
            return Pass.On<R>(obj);
        }


        /// <summary>
        /// Clone an object with Deep Cloning or with a custom strategy 
        /// such as Shallow and/or Deep combined (use the CloneAttribute)
        /// </summary>
        /// <param name="obj">Object to perform cloning on.</param>
        /// <returns>Cloned object.</returns>
        public static object To(this object obj, Type returnType)
        {
            return Pass.On(returnType, obj);            
        }

        /// <summary>
        /// Clone an object with one strategy (DeepClone or ShallowClone)
        /// </summary>
        /// <param name="obj">Object to perform cloning on.</param>
        /// <param name="inspectionType">Type of cloning</param>
        /// <returns>Cloned object.</returns>
        /// <exception cref="InvalidOperationException">When a wrong enum for cloningtype is passed.</exception>
        public static R To<R>(this R obj, Inspection cloneType = Inspection.Deep)            
        {
            return Pass.On<R, R>(obj, cloneType); 
        }

        /// <summary>
        /// Clone an object with one strategy (DeepClone or ShallowClone)
        /// </summary>
        /// <param name="obj">Object to perform cloning on.</param>
        /// <param name="inspectionType">Type of cloning</param>
        /// <returns>Cloned object.</returns>
        /// <exception cref="InvalidOperationException">When a wrong enum for cloningtype is passed.</exception>
        public static R To<T, R>(this T obj, Inspection cloneType = Inspection.Deep)
        {
            return Pass.On<T, R>(obj, cloneType); 
        }

        public static T To<T>(this T source, T destination)            
        {
            return Pass.Onto(source, destination);
        }

        public static R To<T, R>(this T source, R destination)
        {
            return Pass.Onto<T, R>(source, destination);
        }

        public static object To(this object source, object destination)
        {
            return Pass.On(source, destination);
        }

        public static R[] ToArray<T, R>(this IEnumerable<T> source)
        {
            return Pass.ACollectionOf<T>.ToAnArrayOf<R>(source);
        }

        public static R[] ToArray<T, R>(this IEnumerable<T> source, IEnumerable<R> destination)
        {
            return Pass.ACollectionOf<T>.OntoAnArrayOf<R>(source, destination);
        }
    }
}
