using System.Collections.Generic;

namespace Pinja.Examples.Extensibility
{
    /// <summary>
    /// Represents object data that is not recognized to be a part of the deserialized type.
    /// </summary>
    public class ExtensionData
    {
        /// <summary>
        /// Contains the type descriptor of the deserialized object.
        /// </summary>
        public string TypeDescriptor { get; set; }

        /// <summary>
        /// Members of the extension data.
        /// </summary>
        public Dictionary<int, ExtensionMember> ExtensionMembers { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="ExtensionData"/>.
        /// </summary>
        public ExtensionData()
        {
            ExtensionMembers = new Dictionary<int, ExtensionMember>();
        }
    }
}
