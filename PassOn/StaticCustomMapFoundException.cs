using System;

namespace PassOn
{
    public class StaticCustomMapFoundException<T> : Exception
    {
        public StaticCustomMapFoundException(string mapName, string srcName)
            : base($"The mapper \"{mapName}\" for the property \"{srcName}\" in {typeof(T).Name} cannot be static!")
        { }
    }
}
