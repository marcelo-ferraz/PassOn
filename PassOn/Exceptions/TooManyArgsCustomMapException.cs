using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Exceptions
{
    internal class WrongArgCountCustomMapException : Exception
    {
        public WrongArgCountCustomMapException(MethodInfo destMap, byte expected)
            : base($"The mapper {destMap.Name} has {destMap.GetParameters().Length} parameters, but it should have only {expected}")
        {}
    }
}
