namespace CairoDesktop.Application.Interfaces
{
    public interface IDependencyRegistrant
    {
        void Register(IDependencyRegistrar registrar);
    }
}