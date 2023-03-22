using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PassOn.Exceptions
{
    public class BothArgumentsAreSourcesException: Exception
    {
        public BothArgumentsAreSourcesException(MethodInfo func)
            : base($"The \"{func.DeclaringType}.{func}\" function's arguments can't be both Source! ")
        { }
    }
}
