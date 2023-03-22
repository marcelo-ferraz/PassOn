using System;
using System.Reflection;

namespace PassOn.Exceptions
{
    public class InvalidArgCountLifeCycleFunctionException: Exception
    {
        public InvalidArgCountLifeCycleFunctionException(MethodInfo func)
            : base($"The \"{func.DeclaringType}.{func}\" function has an invalid number of arguments! ")
        { }
    }
}
