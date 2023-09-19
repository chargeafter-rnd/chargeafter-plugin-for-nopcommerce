using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.ChargeAfter.Services;
using Nop.Services.Payments;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.ChargeAfter.Components
{
    [ViewComponent(Name = Defaults.CHECKOUT_SCRIPT_VIEW_COMPONENT_NAME)]
    public class ChargeAfterCheckoutScriptViewComponent : NopViewComponent
    {
        #region Fields
        
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ICheckoutDataService _checkoutDataService;
        private readonly ChargeAfterPaymentSettings _settings;

        #endregion

        #region Ctor

        public ChargeAfterCheckoutScriptViewComponent(
            IPaymentPluginManager paymentPluginManager,
            IStoreContext storeContext,
            IWorkContext workContext,
            ICheckoutDataService checkoutDataService,
            ChargeAfterPaymentSettings settings
        )
        {
            _paymentPluginManager = paymentPluginManager;
            _storeContext = storeContext;
            _workContext = workContext;
            _checkoutDataService = checkoutDataService;
            _settings = settings;
        }

        #endregion

        #region Methods 

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_paymentPluginManager.IsPluginActive(Defaults.SystemName, _workContext.CurrentCustomer, _storeContext.CurrentStore.Id))
                return Content(string.Empty);

            var caPublicKey = ChargeAfterHelper.GetPublicKeyFromSettings(_settings);
            if (string.IsNullOrEmpty(caPublicKey))
                return Content(string.Empty);

            var model = _checkoutDataService.GetCheckoutData();
            return View("~/Plugins/Payments.ChargeAfter/Views/Checkout/CheckoutScript.cshtml", model);
        }

        #endregion
    }
}
