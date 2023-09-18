using System;
using System.Reflection;
using UnityEngine;

namespace DJM.DependencyInjection.Binding
{
    internal class BindingData
    {
        private static readonly Type InitializableType = typeof(IInitializable); 
        private static readonly Type DisposableType = typeof(IDisposable); 
        
        public ConstructorInfo Constructor { get; private set; }
        
        private readonly Type _bindingType;
        private readonly bool _isComponent;

        public Type ConcreteType { get; private set; }
        public InitializationOption InitializationOption { get; private set; }
        public bool IsSingle { get; private set; }
        public bool IsNonLazy { get; private set; }
        public bool IsInitializable { get; private set; }
        public bool IsDisposable { get; private set; }
        
        internal BindingData(Type bindingType)
        {
            _bindingType = bindingType;

            if (_bindingType.IsAbstract && !_bindingType.IsInterface) throw new Exception("Cant bind abstract classes");

            if (typeof(Component).IsAssignableFrom(_bindingType))
            {
                _isComponent = true;
                InitializationOption = InitializationOption.NewComponentOnNewGameObject;
            }
            else InitializationOption = InitializationOption.New;
            
            if(!_bindingType.IsInterface) SetConcreteType(_bindingType);
            
            IsSingle = false;
            IsNonLazy = false;
        }

        // these setters should be seperated from the binding value class
        
        internal void SetConcreteType(Type type)
        {
            if(!_bindingType.IsAssignableFrom(type))
            {
                throw new Exception($"binding type {_bindingType} is not assignable from concrete type {type}.");
            }
            
            ConcreteType = type;
            if(_isComponent) return;
            Constructor = ResolveConstructor(ConcreteType);
            if (InitializableType.IsAssignableFrom(type)) IsInitializable = true;
            if (DisposableType.IsAssignableFrom(type)) IsDisposable = true;
        }

        internal void SetInitializationOption(InitializationOption option)
        {
            if (_isComponent && option == InitializationOption.New)
                throw new Exception($"binding type {_bindingType} is a component, so can not be initialized with {option}.");
            

            if (!_isComponent && option == InitializationOption.NewComponentOnNewGameObject)
                throw new Exception($"binding type {_bindingType} is not a component, so can not be initialized with {option}.");
            
            InitializationOption = option;
        }

        internal void SetSingle() => IsSingle = true;
        internal void SetTransient() => IsSingle = false;
        
        internal void SetNonLazy()
        {
            if (!IsSingle) throw new Exception("Transient binding can not be non lazy.");
            IsNonLazy = true;
        }

        private static ConstructorInfo ResolveConstructor(Type type)
        {
            return type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)[0];
        }
    }
        
    internal enum InitializationOption : byte
    {
        New,
        NewComponentOnNewGameObject
    }
}