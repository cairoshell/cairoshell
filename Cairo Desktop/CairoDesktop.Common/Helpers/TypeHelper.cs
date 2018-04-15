using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CairoDesktop.Common.Helpers
{
    public static class TypeHelper
    {
        /// <summary>
        /// Determines whether the current Type derives from the specified Type. 
        /// </summary>
        /// <exception cref="TypeDoesNotDeriveFromRequiredBaseTypeException">TypeDoesNotDeriveFromRequiredBaseTypeException</exception>
        /// <param name="type">The Type to check.</param>
        /// <param name="baseClassType">The base Type the specified Type must derive</param>
        public static void AssertTypeIsSubclassOfBaseType(Type type, Type requiredBaseType)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (requiredBaseType == null)
                throw new ArgumentNullException("requiredBaseType");

            // it better inherit from the base type or actually be the base type
            if (!type.IsSubclassOf(requiredBaseType) || type == requiredBaseType)
                throw new TypeDoesNotDeriveFromRequiredBaseTypeException(type, requiredBaseType);
        }

        /// <summary>
        /// Creates an instance of the specified Type
        /// </summary>
        /// <param name="type">The Type to create</param>
        /// <param name="constructorParamTypes">An array of Types that define the constructor to use to create the type</param>
        /// <returns></returns>
        public static object CreateInstanceOfType(Type type, Type[] constructorParamTypes, object[] args)
        {
            //return Activator.CreateInstance(type, args);

            // look for the required constructor
            ConstructorInfo ci = type.GetConstructor(constructorParamTypes);
            if (ci == null)
                // if the required constructor cannot be found, we cannot continue
                // check the class definition and add the appropriate constructor
                throw new RequiredConstructorNotFoundException(type, constructorParamTypes);

            // create an instance of the specified type
            return ci.Invoke(args);
        }


        /// <summary>
        /// Defines an exception that is generated when a Type does not derive from a required base Type.
        /// </summary>
        public sealed class TypeDoesNotDeriveFromRequiredBaseTypeException : Exception
        {
            private readonly Type _type;
            private readonly Type _requiredBaseType;

            /// <summary>
            /// Initializes a new instance of the TypeDoesNotDeriveFromRequiredBaseTypeException class.
            /// </summary>
            /// <param name="type">The Type that should derive from the specified base Type</param>
            /// <param name="requiredBaseType">The base Type the specified Type needs to derive from</param>
            internal TypeDoesNotDeriveFromRequiredBaseTypeException(Type type, Type requiredBaseType) :
                base(string.Format("The Type '{0}' does not derive from the required base Type '{1}'.", type.FullName, requiredBaseType.FullName))
            {
                _type = type;
                _requiredBaseType = requiredBaseType;
            }

            /// <summary>
            /// Returns the Type that should derive from the specified base Type
            /// </summary>
            public Type Type
            {
                get
                {
                    return _type;
                }
            }

            /// <summary>
            /// Returns the base Type the specified Type needs to derive from
            /// </summary>
            public Type RequiredBaseType
            {
                get
                {
                    return _requiredBaseType;
                }
            }
        }

        /// <summary>
        /// Defines an exception that is generated when a Type to be created does not provide a constructor
        /// that supports a required set of parameters or order.
        /// </summary>
        public sealed class RequiredConstructorNotFoundException : ApplicationException
        {
            private readonly Type _typeToCreate;
            private readonly Type[] _constructorParamTypes;

            /// <summary>
            /// Initializes a new instance of the RequiredConstructorNotFoundException class
            /// </summary>
            /// <param name="typeToCreate">The Type that was to be created</param>
            /// <param name="constructorParamTypes">The array of Types that define the required constructor's prototype</param>
            internal RequiredConstructorNotFoundException(Type typeToCreate, Type[] constructorParamTypes) :
                base(string.Format("The Type '{0}' does not provide a constructor with the required parameter types.", typeToCreate.FullName))
            {
                _typeToCreate = typeToCreate;
                _constructorParamTypes = constructorParamTypes;
            }

            /// <summary>
            /// Returns the Type that was to be created
            /// </summary>
            public Type TypeToCreate
            {
                get
                {
                    return _typeToCreate;
                }
            }

            /// <summary>
            /// Returns an array of Types that define the required constructor's prototype
            /// </summary>
            public Type[] ConstructorParamTypes
            {
                get
                {
                    return _constructorParamTypes;
                }
            }
        }
    }
}