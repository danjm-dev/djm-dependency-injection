namespace DJM.DependencyInjection
{
    public interface IResolvableContainer
    {
        public TBinding Resolve<TBinding>();
    }
}