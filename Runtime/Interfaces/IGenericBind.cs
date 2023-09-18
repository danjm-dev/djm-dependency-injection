namespace DJM.DependencyInjection
{
    public interface IBindTo<T> : IBindFrom<T>
    {
        public IBindFrom<T> To<TImplementation>() where TImplementation : T;
    }

    public interface IBindFrom<T> : IBindScope<T>
    {
        public IBindScope<T> FromNew();
        public IBindScope<T> FromNewComponentOnNewGameObject();
    }

    public interface IBindScope<T> : IBindNonLazy<T>
    {
        public IBindNonLazy<T> AsSingle();
        public void AsTransient();
    }

    public interface IBindNonLazy<T>
    {
        public void NonLazy();
    }
}