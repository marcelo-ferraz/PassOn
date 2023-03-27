using System;

namespace PassOn
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class TargetAttribute : Attribute
    {
    }
}
