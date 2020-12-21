using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static System.FormattableString;

namespace Pinja.Examples.ConventionBasedFactories
{
    /// <summary>
    /// Helper methods for creating handlers based on attributes.
    /// </summary>
    public static class HandlerMappingHelper
    {
        /// <summary>
        /// Creates mappings from type to the specified discriminator type.
        /// </summary>
        /// <param name="discriminatorSelector">Selector to get the discriminator from the specified attribute type.</param>
        /// <typeparam name="TInterface">The interface for the handlers.</typeparam>
        /// <typeparam name="TDiscriminator">The type of the discriminator.</typeparam>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <returns></returns>
        public static IReadOnlyDictionary<Type, TDiscriminator> CreateMappings<TInterface, TDiscriminator, TAttribute>(Func<TAttribute, TDiscriminator> discriminatorSelector)
            where TInterface : class
            where TAttribute : Attribute
        {
            return CreateMappings<TInterface, TDiscriminator, TAttribute>(discriminatorSelector, typeof(TInterface).Assembly);
        }

        /// <summary>
        /// Creates mappings from type to the specified discriminator type by scanning types from the specified assembly.
        /// </summary>
        /// <param name="discriminatorSelector">Selector to get the discriminator from the specified attribute type.</param>
        /// <param name="assemblyToScan">The assembly to scan.</param>
        /// <typeparam name="TInterface">The interface for the handlers.</typeparam>
        /// <typeparam name="TDiscriminator">The type of the discriminator.</typeparam>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        public static IReadOnlyDictionary<Type, TDiscriminator> CreateMappings<TInterface, TDiscriminator, TAttribute>(Func<TAttribute, TDiscriminator> discriminatorSelector, Assembly assemblyToScan)
            where TInterface : class
            where TAttribute : Attribute
        {
            var mappings = new Dictionary<Type, TDiscriminator>();
            var interfaceType = typeof(TInterface);
            // Get implementations of the desired interface from the assembly
            var implementationCandidates = assemblyToScan.GetTypes().Where(interfaceType.IsAssignableFrom);
            var attributeType = typeof(TAttribute);
            foreach (var implementationCandidate in implementationCandidates)
            {
                // Check if the implementation is decorated with the attribute that contains the discriminator
                var attribute = implementationCandidate.GetCustomAttributes(attributeType, false).Cast<TAttribute>().FirstOrDefault();
                if (attribute == null)
                {
                    continue;
                }

                // Store the connection between the implementing type and the discriminator
                if (!mappings.TryAdd(implementationCandidate, discriminatorSelector(attribute)))
                {
                    throw new InvalidOperationException(Invariant($"Type '{implementationCandidate.FullName}' is mapped to more than one discriminator."));
                }
            }

            return mappings;
        }

        /// <summary>
        /// Creates mappings from type to the specified discriminator type.
        /// </summary>
        /// <param name="discriminatorSelector">Selector to get the discriminator from the specified attribute type.</param>
        /// <typeparam name="TInterface">The interface for the handlers.</typeparam>
        /// <typeparam name="TDiscriminator">The type of the discriminator.</typeparam>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <returns></returns>
        public static IReadOnlyDictionary<Type, ICollection<TDiscriminator>> CreateMultiAttributeMappings<TInterface, TDiscriminator, TAttribute>(Func<TAttribute, TDiscriminator> discriminatorSelector)
            where TInterface : class
            where TAttribute : Attribute
        {
            return CreateMultiAttributeMappings<TInterface, TDiscriminator, TAttribute>(discriminatorSelector, typeof(TInterface).Assembly);
        }

        /// <summary>
        /// Creates mappings from type to the specified discriminator type by scanning types from the specified assembly.
        /// </summary>
        /// <param name="discriminatorSelector">Selector to get the discriminator from the specified attribute type.</param>
        /// <param name="assemblyToScan">The assembly to scan.</param>
        /// <typeparam name="TInterface">The interface for the handlers.</typeparam>
        /// <typeparam name="TDiscriminator">The type of the discriminator.</typeparam>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        public static IReadOnlyDictionary<Type, ICollection<TDiscriminator>> CreateMultiAttributeMappings<TInterface, TDiscriminator, TAttribute>(Func<TAttribute, TDiscriminator> discriminatorSelector, Assembly assemblyToScan)
            where TInterface : class
            where TAttribute : Attribute
        {
            var mappings = new Dictionary<Type, ICollection<TDiscriminator>>();
            var interfaceType = typeof(TInterface);
            var implementationCandidates = assemblyToScan.GetTypes().Where(t => interfaceType.IsAssignableFrom(t));
            var attributeType = typeof(TAttribute);
            foreach (var implementationCandidate in implementationCandidates)
            {
                var attributes = implementationCandidate.GetCustomAttributes(attributeType, false).Cast<TAttribute>();

                var list = attributes.Select(discriminatorSelector).ToList();
                mappings.Add(implementationCandidate, list);
            }

            return mappings;
        }

        /// <summary>
        /// Creates an activator method for the specified implementation via the specified activator delegate.
        /// </summary>
        /// <param name="implementation">The type of the implementation.</param>
        /// <typeparam name="TActivator">The delegate type of the activator.</typeparam>
        /// <returns>The compiled activator method.</returns>
        /// <exception cref="ArgumentException">Thrown when the implementing type does not have constructor with the same parameter signature as the activator delegate.</exception>
        public static TActivator CreateActivator<TActivator>(Type implementation) where TActivator : Delegate
        {
            var activatorType = typeof(TActivator);
            const string invokeMethodName = "Invoke";
            // ReSharper disable once PossibleNullReferenceException Delegates always have an Invoke method.
            var ctorTypes = activatorType.GetMethod(invokeMethodName).GetParameters().Select(p => p.ParameterType).ToArray();

            // Get a constructor matching the convention specified by the delegate
            var ctor = implementation.GetConstructor(ctorTypes);

            if (ctor == null)
            {
                throw new ArgumentException($"Updater implementation '{implementation.FullName}' does not have a public constructor following the convention.");
            }

            // Dynamic method that will build the constructor call
            var method = new DynamicMethod("CreateInstance", implementation, ctorTypes);
            var gen = method.GetILGenerator();

            // Load parameters
            for (int i = 0; i < ctorTypes.Length; ++i)
            {
                // Load constructor parameters from the stack in order
                gen.Emit(OpCodes.Ldarg_S, i);
            }

            gen.Emit(OpCodes.Newobj, ctor); // Create instance
            gen.Emit(OpCodes.Ret); // Return

            // Compile the method and return it as the delegate
            return (TActivator)method.CreateDelegate(activatorType);
        }

        /// <summary>
        /// Creates activator mappings.
        /// </summary>
        /// <param name="discriminatorSelector">A selector for getting the discriminator value from the attribute.</param>
        /// <typeparam name="TActivator">The type of the activator delegate.</typeparam>
        /// <typeparam name="TInterface">The type of the handler interface.</typeparam>
        /// <typeparam name="TDiscriminator">The type of the discriminator.</typeparam>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <returns>The mapping from discriminator to activator and implementing type.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the discriminator is mapped to more than one handler implementation.</exception>
        public static IReadOnlyDictionary<TDiscriminator, (TActivator Activator, Type ImplemetingType)> CreateActivators<TActivator, TInterface, TDiscriminator, TAttribute>(
            Func<TAttribute, TDiscriminator> discriminatorSelector)
            where TInterface : class
            where TAttribute : Attribute
            where TActivator : Delegate
        {
            var activators = new Dictionary<TDiscriminator, (TActivator Activator, Type ImplemetingType)>();
            foreach (var (implementingType, discriminator) in CreateMappings<TInterface, TDiscriminator, TAttribute>(discriminatorSelector))
            {
                if (!activators.TryAdd(discriminator, (CreateActivator<TActivator>(implementingType), implementingType)))
                {
                    throw new InvalidOperationException(Invariant($"The discriminator '{discriminator}' is mapped to more than one {typeof(TInterface).FullName} handler implementation."));
                }
            }

            return activators;
        }
    }
}
