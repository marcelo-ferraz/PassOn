using System;
using System.Reflection;

namespace PassOn
{
    public class InvalidArgCountLifeCycleFunction: Exception
    {
        public InvalidArgCountLifeCycleFunction(MethodInfo func)
            : base($"The \"{func.DeclaringType}.{func}\" function has an invalid number of arguments! ")
        { }
    }
}
