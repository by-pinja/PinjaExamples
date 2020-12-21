using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Pinja.Examples.Extensibility
{
    /// <summary>
    /// Helpers for converter types.
    /// </summary>
    public static class SerializationHelpers
    {
        /// <summary>
        /// The default type separator in type descriptors.
        /// </summary>
        public const string DefaultTypeSeparator = ":";

        /// <summary>
        /// The constructor signature of the <see cref="DefaultTypeMappings{TRootType}"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies that contain types that are assignable to the root type.</param>
        /// <param name="typeDescriptorPolicy">The type descriptor policy to use.</param>
        public delegate ITypeMappings DefaultTypeMappingsConstructorDelegate(IEnumerable<Assembly> assemblies, TypeDescriptorPolicy typeDescriptorPolicy);

        /// <summary>
        /// Creates a constructor method for the specified type using the constructor overload with the specified parameters.
        /// </summary>
        /// <param name="instanceType">The type to instantiate.</param>
        /// <typeparam name="TDelegate">The delegate method signature.</typeparam>
        /// <returns>The compiled delegate method wrapper.</returns>
        public static TDelegate CreateConstructor<TDelegate>(Type instanceType) where TDelegate : Delegate
        {
            var delegateType = typeof(TDelegate);
            var invokeMethodInfo = delegateType.GetMethod("Invoke");
            // ReSharper disable once PossibleNullReferenceException Delegates always have the Invoke method.
            var constructorParameterTypes = invokeMethodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
            var ctor = instanceType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, binder: null, constructorParameterTypes, modifiers: null);

            var dynamicMethod = new DynamicMethod(
                ConstructorInfo.ConstructorName,
                invokeMethodInfo.ReturnType,
                constructorParameterTypes,
                typeof(ExtensibleJsonConverterFactory).Module,
                skipVisibility: true);

            ILGenerator generator = dynamicMethod.GetILGenerator();

            // Load the constructor arguments
            for (int i = 0; i < constructorParameterTypes.Length; ++i)
            {
                generator.Emit(OpCodes.Ldarg_S, i);
            }

            generator.Emit(OpCodes.Newobj, ctor);

            generator.Emit(OpCodes.Ret);


            return (TDelegate)dynamicMethod.CreateDelegate(delegateType);
        }
    }
}
