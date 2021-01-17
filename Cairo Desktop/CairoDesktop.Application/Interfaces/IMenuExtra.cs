namespace CairoDesktop.Application.Interfaces
{
    public interface IMenuExtra<out T>
    {
        T StartControl(IMenuExtraHost host);
    }
}
