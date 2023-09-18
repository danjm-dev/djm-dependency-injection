namespace DJM.DependencyInjection.Binding
{
    internal class GenericBinder<TBinding> : IBindTo<TBinding>
    {
        private readonly BindingData _bindingData;
        private AvailableOperations _minAllowedOperation;

        internal GenericBinder(BindingData bindingData)
        {
            _bindingData = bindingData;
            _minAllowedOperation = AvailableOperations.All;
        }

        // IBindTo
        public IBindFrom<TBinding> To<TImplementation>() where TImplementation : TBinding
        {
            ValidateOperation(AvailableOperations.BindTo);
            _bindingData.SetConcreteType(typeof(TImplementation));
            _minAllowedOperation = AvailableOperations.BindFrom;
            return this;
        }
        
        // IBindFrom
        public IBindScope<TBinding> FromNew()
        {
            ValidateOperation(AvailableOperations.BindFrom);
            _bindingData.SetInitializationOption(InitializationOption.New);
            _minAllowedOperation = AvailableOperations.BindScope;
            return this;
        }
        
        public IBindScope<TBinding> FromNewComponentOnNewGameObject()
        {
            ValidateOperation(AvailableOperations.BindFrom);
            _bindingData.SetInitializationOption(InitializationOption.NewComponentOnNewGameObject);
            _minAllowedOperation = AvailableOperations.BindScope;
            return this;
        }
        
        // IBindScope
        public IBindNonLazy<TBinding> AsSingle()
        {
            ValidateOperation(AvailableOperations.BindScope);
            _bindingData.SetSingle();
            _minAllowedOperation = AvailableOperations.BindNonLazy;
            return this;
        }
        
        public void AsTransient()
        {
            ValidateOperation(AvailableOperations.BindScope);
            _bindingData.SetTransient();
            _minAllowedOperation = AvailableOperations.None;
        }
        
        // IBindLazy
        public void NonLazy()
        {
            ValidateOperation(AvailableOperations.BindNonLazy);
            _bindingData.SetNonLazy();
            _minAllowedOperation = AvailableOperations.None;
        }
        
        
        private void ValidateOperation(AvailableOperations attemptedOperation)
        {
            if ((byte)attemptedOperation < (byte)_minAllowedOperation)
            {
                throw new InvalidBindingOrderException
                (
                    attemptedOperation, 
                    _minAllowedOperation
                );
            }
        }
    }
    
    internal enum AvailableOperations : byte
    {
        All,
        BindTo,
        BindFrom,
        BindScope,
        BindNonLazy,
        None
    }
}