using System;
using System.Reflection;

namespace PassOn
{
    public class ValueTypeArgLifeCycleFunction : Exception
    {
        public ValueTypeArgLifeCycleFunction(ParameterInfo arg, MethodInfo func)
            : base($"The argument \"{arg.Name}\" from the \"{func.DeclaringType}.{func}\" function is a value type! ")
        { }
    }
}
