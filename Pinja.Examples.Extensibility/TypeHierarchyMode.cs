using System;

namespace Pinja.Examples.Extensibility
{
    /// <summary>
    /// Specifies type hierarchy serialization modes.
    /// </summary>
    [Serializable]
    public enum TypeHierarchyMode
    {
        /// <summary>
        /// When serializing type descriptor start from the derived type.
        /// </summary>
        DerivedFirst = 1,

        /// <summary>
        /// When serializing type descriptor start from the root type.
        /// </summary>
        AbstractFirst = 2
    }
}
