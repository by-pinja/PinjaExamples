using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Pinja.Examples.Extensibility
{
    /// <summary>
    /// Provides get and set method access to a property.
    /// </summary>
    public class PropertyAccessor
    {
        private delegate void SetterDelegate(object instance, object value);

        private delegate object GetterDelegate(object instance);

        private readonly PropertyInfo _propertyInfo;
        private readonly SetterDelegate _setter;
        private readonly GetterDelegate _getter;

        /// <summary>
        /// The type of the property.
        /// </summary>
        public Type PropertyType => _propertyInfo.PropertyType;

        public string PropertyName => _propertyInfo.Name;

        /// <summary>
        /// Initializes a new instance of <see cref="PropertyAccessor"/>.
        /// </summary>
        /// <param name="propertyInfo">The property info of the property</param>
        /// <param name="containingType">The containing type of the property.</param>
        public PropertyAccessor(PropertyInfo propertyInfo, Type containingType)
        {
            _propertyInfo = propertyInfo;
            _setter = CreateSetter(propertyInfo, containingType);
            _getter = CreateGetter(propertyInfo, containingType);
        }

        /// <summary>
        /// Sets the value for the specified instances property.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="value">The value to set.</param>
        public void SetValue(object instance, object value)
        {
            _setter(instance, value);
        }

        /// <summary>
        /// Gets the property value from the specified instance.
        /// </summary>
        /// <param name="instance">The instance that has the property.</param>
        /// <returns>The property value.</returns>
        public object GetValue(object instance)
        {
            return _getter(instance);
        }

        private static GetterDelegate CreateGetter(PropertyInfo propertyInfo, Type containingType)
        {
            MethodInfo realMethod = propertyInfo.GetGetMethod();
            Type objectType = typeof(object);

            var dynamicMethod = new DynamicMethod(
                realMethod.Name,
                typeof(object),
                new[] { objectType },
                typeof(PropertyAccessor).Module,
                skipVisibility: true);

            ILGenerator generator = dynamicMethod.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);

            if (containingType.IsValueType)
            {
                generator.Emit(OpCodes.Unbox, containingType);
                generator.Emit(OpCodes.Call, realMethod);
            }
            else
            {
                generator.Emit(OpCodes.Castclass, containingType);
                generator.Emit(OpCodes.Callvirt, realMethod);
            }

            if (propertyInfo.PropertyType.IsValueType)
            {
                generator.Emit(OpCodes.Box, propertyInfo.PropertyType);
            }

            generator.Emit(OpCodes.Ret);

            return (GetterDelegate) dynamicMethod.CreateDelegate(typeof(GetterDelegate));
        }

        private static SetterDelegate CreateSetter(PropertyInfo propertyInfo, Type containingType)
        {
            MethodInfo realMethod = propertyInfo.GetSetMethod();
            Type objectType = typeof(object);

            var dynamicMethod = new DynamicMethod(
                realMethod.Name,
                typeof(void),
                new[] { objectType, objectType },
                typeof(PropertyAccessor).Module,
                skipVisibility: true);

            ILGenerator generator = dynamicMethod.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);

            if (containingType.IsValueType)
            {
                generator.Emit(OpCodes.Unbox, containingType);
            }
            else
            {
                generator.Emit(OpCodes.Castclass, containingType);
            }

            generator.Emit(OpCodes.Ldarg_1);

            if (propertyInfo.PropertyType.IsValueType)
            {
                generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
            }
            else
            {
                generator.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
            }

            generator.Emit(OpCodes.Callvirt, realMethod);

            generator.Emit(OpCodes.Ret);

            return (SetterDelegate) dynamicMethod.CreateDelegate(typeof(SetterDelegate));
        }
    }
}
