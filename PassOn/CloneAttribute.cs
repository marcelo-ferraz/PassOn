using System;
using System.Linq.Expressions;

namespace PassOn
{
    /// <summary>
    /// CloningAttribute for specifying the cloneproperties of a property or class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public class CloneAttribute : Attribute
    {
        private Delegate _customParsing;
        private Inspection _inspectionType;
        public CloneAttribute() { }

        public CloneAttribute(Inspection inspectionType, params string[] aliases)
        {
            InspectionType = inspectionType;
            Aliases = aliases;
        }

        public CloneAttribute(Inspection inspectionType, Delegate customParsing, params string[] aliases)
            : this(inspectionType, aliases)
        {
            CustomParsing = customParsing;
        }

        public Inspection InspectionType 
        {
            get { return _inspectionType; }
            set
            {
                _inspectionType = CustomParsing != null ?
                    Inspection.Custom :
                    value;
            }
        }

        public string[] Aliases { get; set; }

        public Delegate CustomParsing
        {
            get
            {
                return _customParsing;
            }
            set
            {
                _customParsing = value;
                InspectionType = Inspection.Custom;
            }
        }
    }
}