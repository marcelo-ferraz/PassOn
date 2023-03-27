
namespace PassOn
{
    /// <summary>
    /// Enumeration that defines the type of cloning of a srcField.
    /// Used in combination with the CloneAttribute
    /// </summary>
    public enum Strategy
    {     
        Shallow,
        Deep,
        Ignore,
        CustomMap
    }
}