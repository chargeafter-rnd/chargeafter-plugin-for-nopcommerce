using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.ChargeAfter.Models;
using Nop.Services.Cms;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.ChargeAfter.Components
{
    [ViewComponent(Name = Defaults.PROMO_SCRIPT_VIEW_COMPONENT_NAME)]
    public class ChargeAfterPromoScriptViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly ChargeAfterPaymentSettings _settings;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public ChargeAfterPromoScriptViewComponent(
            IWidgetPluginManager widgetPluginManager, 
            ChargeAfterPaymentSettings settings,
            IStoreContext storeContext,
            IWorkContext workContext
        ) {
            _widgetPluginManager = widgetPluginManager;
            _settings = settings;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!await _widgetPluginManager.IsPluginActiveAsync(Defaults.SystemName, 
                                                                await _workContext.GetCurrentCustomerAsync(), 
                                                                _storeContext.GetCurrentStore().Id))
                return Content(string.Empty);

            var caPublicKey = ChargeAfterHelper.GetPublicKeyFromSettings(_settings);
            if (string.IsNullOrEmpty(caPublicKey))
                return Content(string.Empty);

            //ensure that it's a proper widget zone
            if (!widgetZone.Equals(PublicWidgetZones.Footer))
                return Content(string.Empty);

            var caHost = ChargeAfterHelper.GetCaHostByUseProduction(_settings.UseProduction);
            var model = new CheckoutModel
            {
                CaPublicKey = caPublicKey,
                CaHost = caHost
            };

            return View("~/Plugins/Payments.ChargeAfter/Views/Promo/PromoScript.cshtml", model);
        }

        #endregion
    }
}
