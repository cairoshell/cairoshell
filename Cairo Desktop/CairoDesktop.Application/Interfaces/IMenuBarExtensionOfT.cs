namespace CairoDesktop.Application.Interfaces
{
    public interface IMenuBarExtension<out T> : IMenuBarExtension
    {
        T StartControl(IMenuBar host);
    }
}