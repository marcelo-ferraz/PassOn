using System;

namespace PassOn
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class BeforeMappingAttribute : Attribute
    {
    }
}
