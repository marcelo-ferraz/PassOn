using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PassOn.Collections
{
    public class Cache
    {
        private List<CacheItem> _items = new List<CacheItem>();

        internal static byte[] CreateKey(Type[] pseudoKey)
        {
            var tmpKey = new List<byte>();
            foreach (var item in pseudoKey)
            {
                tmpKey.AddRange(
                    BitConverter.GetBytes(item.GetHashCode()));
            }
            return tmpKey.ToArray();
        }

        public CacheItem Add(Delegate parser, Delegate[] customParsers, params Type[] key)
        {
            var realKey = Cache.CreateKey(key);

            var item = new CacheItem
            {
                Key = realKey,
                Parser = parser,
                CustomParsers = customParsers
            };

            var i = _items.BinarySearch(item);

            i = i < 0 ? ~i : i;

            bool success = false;
            
            try { }
            finally
            {
                try
                {
                    if (success = Monitor.TryEnter(_items, 500))
                    {                        
                        _items.Insert(i, item);
                    }
                }
                finally
                {
                    if (success) { Monitor.Exit(_items); }
                }
            }
            return item;
        }

        public CacheItem Get(params Type[] key)
        {
            bool success = false;
            CacheItem item = null;
           
            try { } finally
            {
                try
                {

                    if (success = Monitor.TryEnter(_items, 500))
                    {
                        var i = 
                            _items.BinarySearch(new CacheItem 
                            { 
                                Key = Cache.CreateKey(key) 
                            });

                        if (i > -1)
                        { item = _items[i]; }
                    }

                }
                finally
                {
                    if (success) { Monitor.Exit(_items); }
                }
            }
            return item;
        }
    }
}
