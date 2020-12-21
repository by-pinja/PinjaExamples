using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Pinja.Examples.Extensibility
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterExtensibleTypes(
            this IServiceCollection serviceCollection,
            TypeDescriptorPolicy defaultTypeDescriptorPolicy = null,
            IDictionary<Type, TypeDescriptorPolicy> typeDescriptorPolicyOverrides = null)
        {
            return serviceCollection.RegisterExtensibleTypes(defaultTypeDescriptorPolicy ?? TypeDescriptorPolicy.Default,
                typeDescriptorPolicyOverrides ?? new Dictionary<Type, TypeDescriptorPolicy>(),
                AppDomain.CurrentDomain.GetAssemblies());
        }

        public static IServiceCollection RegisterExtensibleTypes(
            this IServiceCollection serviceCollection,
            TypeDescriptorPolicy defaultTypeDescriptorPolicy,
            IDictionary<Type, TypeDescriptorPolicy> typeDescriptorPolicyOverrides,
            Assembly[] assemblies)
        {
            serviceCollection.Configure<JsonSerializerOptions>(options =>
            {
                var rootTypes = assemblies.SelectMany(a => a.GetTypes()
                                                            .Where(t =>
                                                            {
                                                                if (!typeof(IExtensible).IsAssignableFrom(t))
                                                                {
                                                                    return false;
                                                                }

                                                                var extensionDataProperty = t.GetProperty(nameof(IExtensible.ExtensionData), typeof(ExtensionData));
                                                                return extensionDataProperty != null && extensionDataProperty.DeclaringType == t;
                                                            }))
                                          .Distinct();

                var typeHierarchyAssemblyMapping = rootTypes.Select(t => new { Type = t, Assemblies = assemblies.Where(a => a.GetTypes().Any(t.IsAssignableFrom)).Distinct() })
                                                            .ToDictionary(o => o.Type, o => o.Assemblies);

                var typeMappings = new List<ITypeMappings>();

                foreach (var (type, relatedAssemblies) in typeHierarchyAssemblyMapping)
                {
                    if (!typeDescriptorPolicyOverrides.TryGetValue(type, out var typeDescriptorPolicy))
                    {
                        typeDescriptorPolicy = defaultTypeDescriptorPolicy;
                    }
                    typeMappings.Add(SerializationHelpers.CreateConstructor<SerializationHelpers.DefaultTypeMappingsConstructorDelegate>(typeof(DefaultTypeMappings<>).MakeGenericType(type))(relatedAssemblies, typeDescriptorPolicy));
                }

                options.Converters.Add(new ExtensibleJsonConverterFactory(typeMappings, defaultTypeDescriptorPolicy));
            });

            return serviceCollection;
        }
    }
}
