using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Data.Migrations;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Plugins;

namespace Nop.Plugin.Payments.ChargeAfter.Services
{
    public partial class PluginServiceOverride : PluginService
    {
        private readonly ChargeAfterPaymentSettings _chargeAfterPaymentSettings;

        public PluginServiceOverride(
            CatalogSettings catalogSettings, 
            ICustomerService customerService, 
            IMigrationManager migrationManager, 
            ILogger logger, 
            INopFileProvider fileProvider, 
            IWebHelper webHelper,
            ChargeAfterPaymentSettings chargeAfterPaymentSettings
        ) : base(catalogSettings, customerService, migrationManager, logger, fileProvider, webHelper) 
        {
            _chargeAfterPaymentSettings = chargeAfterPaymentSettings;
        }

        public override string GetPluginLogoUrl(PluginDescriptor pluginDescriptor)
        {
            if(pluginDescriptor.SystemName == Defaults.SystemName) {
                var brandUrl = ChargeAfterHelper.GetBrandUrlFromSettings(_chargeAfterPaymentSettings);
                
                if(!string.IsNullOrEmpty(brandUrl)) {
                    return brandUrl;
                }
            }

            return base.GetPluginLogoUrl(pluginDescriptor);
        }

    }
}
