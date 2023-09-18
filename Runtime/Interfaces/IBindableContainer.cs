namespace DJM.DependencyInjection
{
    public interface IBindableContainer
    {
        public IBindTo<TBinding> Bind<TBinding>();
    }
}