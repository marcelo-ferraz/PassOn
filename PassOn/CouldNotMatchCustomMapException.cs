using System;


namespace PassOn
{
    public class CustomMapNoMatchException<T> : Exception
    {
        public CustomMapNoMatchException(string mapName, string srcName)
            : base($"Could not find the mapper \"{mapName}\" for the property \"{srcName}\" in {typeof(T).Name}!")
        { }
    }
}
