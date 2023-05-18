using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Payments.ChargeAfter.Components
{
    [ViewComponent(Name = Defaults.PROMO_LINE_OF_CREDIT_VIEW_COMPONENT_NAME)]
    public class ChargeAfterPromoLineOfCreditViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly ChargeAfterPaymentSettings _settings;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IProductService _productService;

        #endregion

        #region Ctor

        public ChargeAfterPromoLineOfCreditViewComponent(
            IWidgetPluginManager widgetPluginManager,
            ChargeAfterPaymentSettings settings,
            IStoreContext storeContext,
            IWorkContext workContext,
            IProductService productService
        )
        {
            _widgetPluginManager = widgetPluginManager;
            _settings = settings;
            _storeContext = storeContext;
            _workContext = workContext;
            _productService = productService;
        }

        #endregion

        #region Methods

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_widgetPluginManager.IsPluginActive(Defaults.SystemName, _workContext.CurrentCustomer, _storeContext.CurrentStore.Id))
                return Content(string.Empty);

            if (string.IsNullOrEmpty(ChargeAfterHelper.GetPublicKeyFromSettings(_settings)) || !_settings.EnableLineOfCreditPromo)
                return Content(string.Empty);

            //ensure that it's a proper widget zone
            if (!widgetZone.Equals(PublicWidgetZones.ProductDetailsAddInfo))
                return Content(string.Empty);

            var productId = additionalData is ProductDetailsModel.AddToCartModel model ? model.ProductId : 0;
            if (productId == 0)
                return Content(string.Empty);

            var product = _productService.GetProductById(productId);
            if(product == null || string.IsNullOrEmpty(product.Sku) || product.Price == 0 || product.IsRental)
                return Content(string.Empty);

            var financingPageUrl = _settings.FinancingPageUrlLineOfCreditPromo;
            if (string.IsNullOrEmpty(financingPageUrl))
                financingPageUrl = "/";

            return View("~/Plugins/Payments.ChargeAfter/Views/Promo/PromoLineOfCredit.cshtml", (product, financingPageUrl));
        }

        #endregion
    }
}
