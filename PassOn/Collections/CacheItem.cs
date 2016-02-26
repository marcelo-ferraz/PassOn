using PassOn.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PassOn.Collections
{
    public class CacheItem : IComparable
    {
        public byte[] Key { get; set; }
        public Delegate Parser { get; set; }
        public Delegate[] CustomParsers { get; set; }

        public CacheItem(params Type[] val)
        {
            Key = Cache.CreateKey(val);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }

            CacheItem other;

            if ((other = obj as CacheItem) == null || other.Key == null)
            { return false; }

            return Range.Equals(this.Key, other.Key);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            { throw new ArgumentNullException("obj"); }

            CacheItem other;

            if ((other = obj as CacheItem) == null || other.Key == null)
            { throw new ArgumentException("obj"); }

            return Range.Compare(this.Key, other.Key);
        }
    }
}
