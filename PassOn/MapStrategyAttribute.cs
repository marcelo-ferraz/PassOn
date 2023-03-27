using System;

namespace PassOn
{
    /// <summary>
    /// CloningAttribute for specifying the cloneproperties of a property or class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public class MapStrategyAttribute : Attribute
    {
        public const Strategy DEFAULT_TYPE = Strategy.Deep;

        public MapStrategyAttribute() { }
        
        public MapStrategyAttribute(params string[] aliases)
        {
            Aliases = aliases;
        }

        public MapStrategyAttribute(Strategy type = DEFAULT_TYPE, params string[] aliases)
        {
            Type = type;
            Aliases = aliases;
        }

        public Strategy Type { get; set; }

        public string[] Aliases { get; set; }

        public string Alias {
            get { return this.Aliases[0]; }
            set { this.Aliases = new[] { value }; }
        }

        public string Mapper { get; set; }
    }
}