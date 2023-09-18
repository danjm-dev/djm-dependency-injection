using System;
using System.Runtime.Serialization;
using DJM.DependencyInjection.Binding;

namespace DJM.DependencyInjection
{
    // Base exception for all DI-related issues
    internal class DependencyInjectionException : Exception
    {
        internal DependencyInjectionException(string message) : base(message) { }
        internal DependencyInjectionException(string message, Exception innerException) : base(message, innerException) { }
        protected DependencyInjectionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    // Exception thrown when an invalid binding operation order occurs
    internal class InvalidBindingOrderException : DependencyInjectionException
    {
        internal InvalidBindingOrderException(AvailableOperations attemptedOperation, AvailableOperations lastOperation) 
            : base($"Invalid binding operation order. {attemptedOperation.ToString()} was attempted, but the last operation was {lastOperation.ToString()}.") { }
    }

    // Exception thrown when a type is already registered in the DI container
    internal class TypeAlreadyRegisteredException : DependencyInjectionException
    {
        internal TypeAlreadyRegisteredException(Type type)
            : base($"Type {type.FullName} is already registered.") { }
    }

    // Exception thrown when trying to resolve a type that has not been registered
    internal class TypeNotRegisteredException : DependencyInjectionException
    {
        internal TypeNotRegisteredException(Type type)
            : base($"Type {type.FullName} is not registered.") { }
    }
    
    // Exception thrown when validating all registered types
    internal class InstallationValidationFailedException : DependencyInjectionException
    {
        public InstallationValidationFailedException(Type type, Exception innerException)
            : base($"Initialization validation failed for type {type.FullName}.", innerException) { }

        protected InstallationValidationFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}