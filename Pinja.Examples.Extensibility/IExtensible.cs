namespace Pinja.Examples.Extensibility
{
    /// <summary>
    /// Defines an interface for extensible data structures.
    /// </summary>
    public interface IExtensible
    {
        /// <summary>
        /// The extension data containing all members which are not recognized to be a part of the type being deserialized.
        /// </summary>
        ExtensionData ExtensionData { get; set; }
    }
}
