namespace Pinja.Examples.Extensibility
{
    /// <summary>
    /// Represents a single member of an extensible data structure that is not recognized to be a part the the deserialized type.
    /// </summary>
    public class ExtensionMember
    {
        /// <summary>
        /// The index of the member in the serialized form.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The name of the member.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value of the member.
        /// </summary>
        public object Value { get; set; }
    }
}
