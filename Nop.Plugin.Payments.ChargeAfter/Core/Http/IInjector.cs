namespace Nop.Plugin.Payments.ChargeAfter.Core.Http
{
    public interface IInjector
    {
        void Inject(HttpRequest request);
    }
}
