using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Utilities
{    
    internal static class IEnumerableExtensions
    {
        public static IEnumerable<(Left, Right)> Zip<Left, Right> (
            this (IEnumerable<Left>, IEnumerable<Right>) both)
            where Left : class
            where Right : class
        {
            var (left, right) = both;

            if (left == null)
                left = Enumerable.Empty<Left>();
            if (right == null)
                right = Enumerable.Empty<Right>();

            using (IEnumerator<Left> leftEnum = left.GetEnumerator())
            using (IEnumerator<Right> rightEnum = right.GetEnumerator())
            {
                bool leftIterated = leftEnum.MoveNext();
                bool rightIterated = rightEnum.MoveNext();

                while (rightIterated || leftIterated)
                {
                    yield return (
                        leftIterated? leftEnum.Current: null,
                        rightIterated ? rightEnum.Current: null
                    );

                    leftIterated = leftEnum.MoveNext();
                    rightIterated = rightEnum.MoveNext();
                }
            }            
        }
    }
}
