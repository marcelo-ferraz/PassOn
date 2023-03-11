using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PassOn
{
    public class BothArgumentsAreTargetsException: Exception
    {
        public BothArgumentsAreTargetsException(MethodInfo func)
            : base($"The \"{func.DeclaringType}.{func}\" function's arguments can't be both Target! ")
        { }
    }
}
