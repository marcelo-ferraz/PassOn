using System;
using System.Reflection;

namespace PassOn.Exceptions
{
    public class ValueTypeArgLifecycleFunctionException : Exception
    {
        public ValueTypeArgLifecycleFunctionException(ParameterInfo arg, MethodInfo func)
            : base($"The argument \"{arg.Name}\" from the \"{func.DeclaringType}.{func}\" function is a value type! ")
        { }
    }
}
