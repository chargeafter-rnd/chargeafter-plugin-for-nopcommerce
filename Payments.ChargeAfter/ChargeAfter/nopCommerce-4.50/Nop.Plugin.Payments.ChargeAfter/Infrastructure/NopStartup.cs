using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.ChargeAfter.Areas.Admin.Controllers;
using Nop.Plugin.Payments.ChargeAfter.Services;
using Nop.Services.Plugins;

namespace Nop.Plugin.Payments.ChargeAfter.Infrastructure
{
    public class NopStartup : INopStartup
    {
        public int Order => 3000;

        public void Configure(IApplicationBuilder application)
        {
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ServiceManager>();
            services.AddScoped<IPluginService, PluginServiceOverride>();
            services.AddScoped<ICheckoutDataService, CheckoutDataService>();
            services.AddScoped<INonLeasableService, NonLeasableService>();
            services.AddScoped<IOrderTaxService, OrderTaxService>();

            //override product controller in admin area
            services.AddScoped<Web.Areas.Admin.Controllers.ProductController, PaymentChargeAfterProductController>();
        }
    }
}
