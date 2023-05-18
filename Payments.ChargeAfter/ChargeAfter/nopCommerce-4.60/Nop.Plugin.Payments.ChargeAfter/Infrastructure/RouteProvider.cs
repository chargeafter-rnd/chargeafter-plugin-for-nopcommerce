using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.ChargeAfter.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            // Checkout Place
            endpointRouteBuilder.MapControllerRoute(Defaults.CheckoutPlaceRouteName, Defaults.CheckoutPlaceRoute,
                new { controller = "PaymentChargeAfterCheckout", action = "Place" });
        }

        public int Priority => 0;
    }
}
