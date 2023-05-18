using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.ChargeAfter.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.ChargeAfter.Components
{
    [ViewComponent(Name = "ChargeAfterPaymentInfo")]
    public class ChargeAfterPaymentInfoViewComponent : NopViewComponent
    {
        private readonly ChargeAfterPaymentSettings _chargeAfterPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public ChargeAfterPaymentInfoViewComponent(
            ChargeAfterPaymentSettings chargeAfterPaymentSettings,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IWorkContext workContext
        ) {
            _chargeAfterPaymentSettings = chargeAfterPaymentSettings;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new PaymentInfoModel
            {
                DescriptionText = await _localizationService.GetResourceAsync("Plugins.Payment.ChargeAfter.PaymentMethod.Tip")
            };

            return View("~/Plugins/Payments.ChargeAfter/Views/Payment/PaymentInfo.cshtml", model);
        }
    }
}
