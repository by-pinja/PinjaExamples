using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Pinja.Examples.Extensibility
{
    /// <summary>
    /// Provides a static cache for type accessors.
    /// </summary>
    public static class TypeAccessorCache
    {
        private static readonly ConcurrentDictionary<Type, TypeAccessor> TypeAccessors = new ConcurrentDictionary<Type, TypeAccessor>();

        /// <summary>
        /// Gets a type accessor for the specified type.
        /// </summary>
        /// <param name="type">The type to get accessor for.</param>
        /// <param name="propertyFilter">A filter to filter out properties to access from the type.</param>
        /// <returns>A type accessor.</returns>
        public static TypeAccessor GetTypeAccessor(Type type, Func<PropertyInfo, bool> propertyFilter = null)
        {
            if (!TypeAccessors.TryGetValue(type, out var typeCreator))
            {
                typeCreator = TypeAccessors.GetOrAdd(type, new TypeAccessor(type, propertyFilter));
            }

            return typeCreator;
        }
    }
}
