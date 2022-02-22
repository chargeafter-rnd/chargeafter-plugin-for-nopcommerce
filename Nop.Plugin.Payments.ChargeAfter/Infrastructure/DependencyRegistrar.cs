using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Payments.ChargeAfter.Services;
using Nop.Services.Plugins;

namespace Nop.Plugin.Payments.ChargeAfter.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<ServiceManager>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<OrderPaymentInfoService>().As<IOrderPaymentInfoService>().InstancePerLifetimeScope();
            builder.RegisterType<PluginServiceOverride>().As<IPluginService>().InstancePerLifetimeScope();
            builder.RegisterType<CheckoutDataService>().As<ICheckoutDataService>().InstancePerLifetimeScope();
        }

        public int Order => 2;
    }
}
