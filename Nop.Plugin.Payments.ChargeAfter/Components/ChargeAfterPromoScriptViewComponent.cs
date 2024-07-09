using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.ChargeAfter.Domain;
using Nop.Services.Cms;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

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

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_widgetPluginManager.IsPluginActive(Defaults.SystemName, _workContext.CurrentCustomer, _storeContext.CurrentStore.Id))
                return Content(string.Empty);

            var caPublicKey = ChargeAfterHelper.GetPublicKeyFromSettings(_settings);
            if (string.IsNullOrEmpty(caPublicKey))
                return Content(string.Empty);

            //ensure that it's a proper widget zone
            if (!widgetZone.Equals(PublicWidgetZones.Footer))
                return Content(string.Empty);

            var caHost = ChargeAfterHelper.GetCaHostByUseProduction(_settings.UseProduction);
            var checkoutPromoType = _settings.TypeCheckoutBrand.ToKebabCaseString();

            return View("~/Plugins/Payments.ChargeAfter/Views/Promo/PromoScript.cshtml", (caPublicKey, caHost, checkoutPromoType));
        }

        #endregion
    }
}
