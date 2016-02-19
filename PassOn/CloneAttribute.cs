using System;

namespace PassOn
{
    /// <summary>
    /// CloningAttribute for specifying the cloneproperties of a property or class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public class CloneAttribute : Attribute
    {
        public CloneAttribute() { }

        public CloneAttribute(Inspection inspectionType, params string[] aliases)
        {
            InspectionType = inspectionType;
            Aliases = aliases;
        }

        public Inspection InspectionType { get; set; }

        public string[] Aliases { get; set; }
    }
}