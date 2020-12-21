using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pinja.Examples.Extensibility
{
    /// <summary>
    /// Providers access to a type's properties.
    /// </summary>
    public class TypeAccessor
    {
        /// <summary>
        /// The type this accessor governs.
        /// </summary>
        public Type Type { get; }

        private delegate object ConstructorDelegate();

        private readonly ConstructorDelegate _ctor;
        private readonly Dictionary<string, PropertyAccessor> _propertyAccessors;

        /// <summary>
        /// Initializes a new instance of <see cref="TypeAccessor"/>.
        /// </summary>
        /// <param name="type">The type to access.</param>
        /// <param name="propertyFilter">A filter to filter out properties to access.</param>
        public TypeAccessor(Type type, Func<PropertyInfo, bool> propertyFilter = null)
        {
            Type = type;
            _ctor = CreateConstructor(Type);
            _propertyAccessors = CreatePropertyAccessors(Type, propertyFilter);
        }

        /// <summary>
        /// Creates an instance of the type.
        /// </summary>
        /// <returns></returns>
        public object CreateInstance()
        {
            return _ctor();
        }

        /// <summary>
        /// Tries to set a value to the specified member of the specified instance.
        /// </summary>
        /// <param name="instance">The instance that has the member.</param>
        /// <param name="memberName">The member name.</param>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="options">The JSON serializer options.</param>
        /// <returns><b>True</b> member was assigned; otherwise <b>false</b>.</returns>
        public bool TrySetMember(object instance, string memberName, ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (!_propertyAccessors.TryGetValue(memberName, out var setter))
            {
                return false;
            }

            object value = JsonSerializer.Deserialize(ref reader, setter.PropertyType, options);

            setter.SetValue(instance, value);
            reader.Read();
            return true;
        }

        /// <summary>
        /// Gets actions that writers property values from the specified instance to the writer.
        /// </summary>
        /// <param name="instance">The instance to get property values from.</param>
        /// <param name="writer">The JSON writer.</param>
        /// <param name="options">The JSON serializer options.</param>
        /// <returns>An enumerable of actions that perform the write operations.</returns>
        public IEnumerable<PropertyWriter> GetPropertyWriters(object instance, Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            foreach (var (memberName, accessor) in _propertyAccessors)
            {
                yield return new PropertyWriter(memberName, accessor, instance, writer, options);
            }
        }

        private static Dictionary<string, PropertyAccessor> CreatePropertyAccessors(Type type, Func<PropertyInfo, bool> propertyFilter = null)
        {
            var setters = new Dictionary<string, PropertyAccessor>(StringComparer.InvariantCultureIgnoreCase);
            IEnumerable<PropertyInfo> properties = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty)
                .Where(p => !IsIgnored(p));
            if (propertyFilter != null)
            {
                properties = properties.Where(propertyFilter);
            }

            foreach (var property in properties)
            {
                setters[property.Name] = new PropertyAccessor(property, type);
            }

            return setters;
        }

        private static bool IsIgnored(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes().Any(a => a is NonSerializedAttribute || a is JsonIgnoreAttribute);
        }

        private static ConstructorDelegate CreateConstructor(Type eventType)
        {
            var ctor = eventType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, binder: null, Type.EmptyTypes, modifiers: null);

            var dynamicMethod = new DynamicMethod(
                ConstructorInfo.ConstructorName,
                typeof(object),
                Type.EmptyTypes,
                typeof(IExtensible).Module,
                skipVisibility: true);

            ILGenerator generator = dynamicMethod.GetILGenerator();

            generator.Emit(OpCodes.Newobj, ctor);

            generator.Emit(OpCodes.Ret);

            return (ConstructorDelegate)dynamicMethod.CreateDelegate(typeof(ConstructorDelegate));
        }
    }
}
