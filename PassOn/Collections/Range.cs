using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PassOn.Collections
{
    public static class Range
    {
        public static bool Equals(byte[] self, byte[] other)
        {
            if (self.Length != other.Length) { return false; }
            
            for (int i = 0; i < self.Length; i++)
            {
                if (self[i] != other[i])
                { return false; }
            }

            return true;
        }

        //public unsafe static bool UnsafeEquals(byte[] self, byte[] other)
        //{
        //    if (self.Length != other.Length) { return false; }
            
        //    int n = self.Length;

        //    fixed (byte* selfPtr = self, otherPtr = other)
        //    {
        //        byte* ptr1 = selfPtr;
        //        byte* ptr2 = otherPtr;

        //        byte b1;
        //        byte b2;
        //        while (n-- > 0)
        //        {
        //            b1 = (*ptr1++);
        //            b2 = (*ptr2++);

        //            if (b1 != b2) { return false; }
        //        }
        //    }
        //    return true;
        //}

        public static int Compare(byte[] self, byte[] other)
        {
            if (self.Length < other.Length) { return -1; }

            if (self.Length > other.Length) { return +1; }

            for (int i = 0; i < self.Length; i++)
            {
                if (self[i] != other[i]) 
                { return other[i] - self[i]; }
            }

            return 0;
        }

        //public unsafe static int UnsafeCompare(byte[] self, byte[] other)
        //{
        //    if (self.Length < other.Length) { return -1; }

        //    if (self.Length > other.Length) { return +1; }

        //    int n = self.Length;

        //    fixed (byte* selfPtr = self, otherPtr = other)
        //    {
        //        byte* ptr1 = selfPtr;
        //        byte* ptr2 = otherPtr;

        //        byte b1;
        //        byte b2;
        //        while (n-- > 0)
        //        {
        //            b1 = (*ptr1++);
        //            b2 = (*ptr2++);

        //            if (b1 != b2) { return b1 - b2; }
        //        }
        //    }
        //    return 0;
        //}
    }
}