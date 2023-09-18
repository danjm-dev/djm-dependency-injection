namespace DJM.DependencyInjection
{
    public interface IInstaller
    {
        public void InstallBindings(IBindableContainer container);
    }
}