using System;
using System.Collections.Generic;
using DJM.DependencyInjection.Binding;
using DJM.DependencyInjection.ComponentContext;
using UnityEngine;

namespace DJM.DependencyInjection
{
    internal sealed class DependencyContainer : IResolvableContainer, IBindableContainer
    {
        private readonly Dictionary<Type, BindingData> _bindings;
        private readonly HashSet<Type> _nonLazyBindings;
        
        private readonly Dictionary<Type, object> _singleInstances;
        private readonly List<IInitializable> _initializables;
        private readonly List<IDisposable> _disposables;
        
        private readonly GameObjectContext _gameObjectContext;

        internal DependencyContainer(GameObjectContext gameObjectContext)
        {
            _bindings = new Dictionary<Type, BindingData>();
            _nonLazyBindings = new HashSet<Type>();
            
            _singleInstances = new Dictionary<Type, object>();
            _initializables = new List<IInitializable>();
            _disposables = new List<IDisposable>();
            
            _gameObjectContext = gameObjectContext;
            _gameObjectContext.OnContextStart += RunInitializables;
            _gameObjectContext.OnContextDestroy += RunDisposables;
        }
        
        public IBindTo<TBinding> Bind<TBinding>()
        {
            var bindingType = typeof(TBinding);
            if (_bindings.ContainsKey(bindingType)) throw new TypeAlreadyRegisteredException(bindingType);
            
            // register binding with default data
            var bindingData = new BindingData(bindingType);
            _bindings[bindingType] = bindingData;
            
            return new GenericBinder<TBinding>(bindingData);
        }
        
        public void Install(params IInstaller[] installers)
        {
            foreach (var installer in installers) installer.InstallBindings(this);
            ValidateBindings();
            
            
            foreach (var type in _bindings.Keys)
            {
                var bindingData = _bindings[type];
                if (bindingData.IsNonLazy)
                {
                    _nonLazyBindings.Add(type);
                    Resolve(type);
                }
            }
        }

        public TBinding Resolve<TBinding>()
        {
            var type = typeof(TBinding);
            return (TBinding)Resolve(type);
        }
        
        private object Resolve(Type bindingType)
        {
            if (!_bindings.ContainsKey(bindingType)) throw new TypeNotRegisteredException(bindingType);
            
            var bindingData = _bindings[bindingType];

            // create transient instance
            if (!bindingData.IsSingle) return CreateInstance(bindingData);
            
            // return existing single instance
            if (_singleInstances.TryGetValue(bindingType, out var singleInstance)) return singleInstance;

            // create single instance
            var newInstance = CreateInstance(bindingData);
            _singleInstances[bindingType] = newInstance;
            return newInstance;
        }
        
        private object CreateInstance(BindingData bindingData)
        {
            return bindingData.InitializationOption switch
            {
                InitializationOption.New => CreateNewInstance(bindingData),
                InitializationOption.NewComponentOnNewGameObject => CreateNewComponentOnNewGameObjectInstance(bindingData),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private object CreateNewInstance(BindingData bindingData)
        {
            var constructorParameters = bindingData.Constructor.GetParameters();
            var parameters = new object[constructorParameters.Length];

            for (var i = 0; i < constructorParameters.Length; i++)
            {
                var parameterType = constructorParameters[i].ParameterType;
                parameters[i] = Resolve(parameterType);
            }

            var instance = Activator.CreateInstance(bindingData.ConcreteType, parameters);

            // these are not called on components
            if(bindingData.IsInitializable) _initializables.Add((IInitializable)instance);
            if(bindingData.IsDisposable) _disposables.Add((IDisposable)instance);

            return instance;
        }
        
        private object CreateNewComponentOnNewGameObjectInstance(BindingData bindingData)
        {
            return _gameObjectContext.AddComponentToNewChildGameObject(bindingData.ConcreteType);
        }
        
        private void ValidateBindings()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD

            var components = new List<Component>();
            
            foreach (var type in _bindings.Keys)
            {
                try
                {
                    var instance = Resolve(type);
                    if(instance is Component component)
                        components.Add(component);
                }
                catch (Exception exception)
                {
                    throw new InstallationValidationFailedException(type, exception);
                }
            }

            RunInitializables();
            RunDisposables();
            _singleInstances.Clear();
            foreach (var instance in components) UnityEngine.Object.Destroy(instance.gameObject);
#endif
        }

        private void RunInitializables()
        {
            foreach (var initializable in _initializables) initializable.Initialize();
            _initializables.Clear();
        }
        
        private void RunDisposables()
        {
            foreach (var disposable in _disposables) disposable.Dispose();
            _disposables.Clear();
        }
    }
}