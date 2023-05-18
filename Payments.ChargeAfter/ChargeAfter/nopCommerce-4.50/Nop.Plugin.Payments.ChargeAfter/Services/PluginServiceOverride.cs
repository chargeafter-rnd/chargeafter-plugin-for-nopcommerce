using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data.Migrations;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.ChargeAfter.Services
{
    public partial class PluginServiceOverride : PluginService
    {
        #region Fields

        private readonly ChargeAfterPaymentSettings _chargeAfterPaymentSettings;

        #endregion

        #region Ctor

        public PluginServiceOverride(
            CatalogSettings catalogSettings, 
            ICustomerService customerService,
            IHttpContextAccessor httpContextAccessor,
            IMigrationManager migrationManager, 
            ILogger logger, 
            INopFileProvider fileProvider, 
            IWebHelper webHelper,
            MediaSettings mediaSettings,
            ChargeAfterPaymentSettings chargeAfterPaymentSettings
        ) : base(catalogSettings, customerService, httpContextAccessor, migrationManager, logger, fileProvider, webHelper, mediaSettings) 
        {
            _chargeAfterPaymentSettings = chargeAfterPaymentSettings;
        }

        #endregion

        #region Methods

        public override async Task<string> GetPluginLogoUrlAsync(PluginDescriptor pluginDescriptor)
        {
            if(pluginDescriptor.SystemName == Defaults.SystemName) {
                var brandUrl = ChargeAfterHelper.GetBrandUrlFromSettings(_chargeAfterPaymentSettings);
                
                if(!string.IsNullOrEmpty(brandUrl)) {
                    return brandUrl;
                }
            }

            return await base.GetPluginLogoUrlAsync(pluginDescriptor);
        }

        #endregion

    }
}
