using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PassOn.Utilities
{
    public class IterateThrough
    {
        public static void Both<T, R>(
            T[] left, IEnumerable<R> right, Action<int, R> both, Action<R> onlyRight, Action<int> onlyLeft)
        {
            if (left == null) { return; }

            var dest = right.GetEnumerator();
            bool destMoved = true;

            int i = 0;

            while (destMoved &= dest.MoveNext())
            {
                if (destMoved && i++ < left.Length)
                {
                    both(i, dest.Current);
                }
                else if (destMoved)
                {
                    onlyRight(dest.Current);
                }
                else // index < left.Length
                {
                    onlyLeft(i);
                }
            }
        }

        public static void Both<T, R>(
            IEnumerable<T> left, IEnumerable<R> right, Action<T, R> both, Action<R> onlyRight, Action<T> onlyLeft)
        {
            if (left == null) { return; }

            var result = new List<R>();

            bool srcMoved = true;
            bool destMoved = true;

            var src = left.GetEnumerator();

            var dest = right != null ?
                right.GetEnumerator() :
                Enumerable.Empty<R>().GetEnumerator();

            while ((srcMoved &= src.MoveNext()) |
                (destMoved &= dest.MoveNext()))
            {
                if (srcMoved && destMoved)
                {
                    both(
                        src.Current,
                        dest.Current);
                }
                else if (srcMoved)
                {
                    onlyLeft(src.Current);
                }
                else // destMoved
                {
                    onlyRight(dest.Current);
                }
            }
        }

        public static void Both<T, R>(
            T[] left, R[] right, Action<int> both, Action<int> onlyRight, Action<int> onlyLeft)
        {
            if (left == null) { return; }

            var length = (right != null && right.Length > left.Length) ?
                right.Length :
                left.Length;

            for (int i = 0; i < length; i++)
            {
                //both have items
                if (i < left.Length && i < right.Length)
                {
                    both(i);
                } // left does not have an item
                else if (i >= left.Length && i < right.Length)
                {
                    onlyRight(i);
                } // right does not have an item
                else if (i < left.Length && i >= right.Length)
                {
                    onlyLeft(i);
                }
            }
        }

        public static void Both<T, R>(
            IEnumerable<T> left, R[] right, Action<T, int> both, Action<int> onlyRight, Action<T> onlyLeft) 
        {

            if (left == null) { return; }

            var src = left.GetEnumerator();
            bool srcMoved = true;

            int i = 0;

            while (srcMoved &= src.MoveNext())
            {
                if (srcMoved && i++ < right.Length)
                {
                    both(src.Current, i);
                }
                else if (srcMoved)
                {
                    onlyLeft(src.Current);
                }
                else // index < right.Length
                {
                    onlyRight(i);                    
                }
            }
        }
    }
}