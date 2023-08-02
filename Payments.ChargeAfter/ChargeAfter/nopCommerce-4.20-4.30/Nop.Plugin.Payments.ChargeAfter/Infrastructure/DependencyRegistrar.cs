using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Payments.ChargeAfter.Areas.Admin.Controllers;
using Nop.Plugin.Payments.ChargeAfter.Services;
using Nop.Services.Plugins;

namespace Nop.Plugin.Payments.ChargeAfter.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<ServiceManager>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<PluginServiceOverride>().As<IPluginService>().InstancePerLifetimeScope();
            builder.RegisterType<CheckoutDataService>().As<ICheckoutDataService>().InstancePerLifetimeScope();
            builder.RegisterType<NonLeasableService>().As<INonLeasableService>().InstancePerLifetimeScope();
            builder.RegisterType<OrderTaxService>().As<IOrderTaxService>().InstancePerLifetimeScope();

            //override product controller in admin area
            builder.RegisterType<PaymentChargeAfterProductController>().As<Web.Areas.Admin.Controllers.ProductController>();
        }

        public int Order => 2;
    }
}
