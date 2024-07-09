using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.ChargeAfter.Components
{
    [ViewComponent(Name = "ChargeAfterPromoSimpleProduct")]
    public class ChargeAfterPromoSimpleProductViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly ChargeAfterPaymentSettings _settings;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public ChargeAfterPromoSimpleProductViewComponent(
            IWidgetPluginManager widgetPluginManager,
            ChargeAfterPaymentSettings settings,
            IStoreContext storeContext,
            IWorkContext workContext
        )
        {
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

            if (string.IsNullOrEmpty(ChargeAfterHelper.GetPublicKeyFromSettings(_settings)))
                return Content(string.Empty);

            //ensure that it's a proper widget zone
            if (!widgetZone.Equals(PublicWidgetZones.ProductDetailsEssentialTop) && 
                !widgetZone.Equals(PublicWidgetZones.ProductDetailsOverviewTop) && 
                !widgetZone.Equals(PublicWidgetZones.ProductDetailsEssentialBottom))
            {
                return Content(string.Empty);
            }
                

            if((widgetZone.Equals(PublicWidgetZones.ProductDetailsEssentialTop) && !_settings.EnableSimplePromoProductBeforeContent) ||
               (widgetZone.Equals(PublicWidgetZones.ProductDetailsOverviewTop) && !_settings.EnableSimplePromoProductAfterTitle) ||
               (widgetZone.Equals(PublicWidgetZones.ProductDetailsEssentialBottom) && !_settings.EnableSimplePromoProductAfterDesc))
            {
                return Content(string.Empty);
            }
                
            var widgetType = _settings.WidgetTypeSimplePromoProductBeforeContent;
            if(widgetZone.Equals(PublicWidgetZones.ProductDetailsOverviewTop))
            {
                widgetType = _settings.WidgetTypeSimplePromoProductAfterTitle;
            } 
            else if(widgetZone.Equals(PublicWidgetZones.ProductDetailsEssentialBottom))
            {
                widgetType = _settings.WidgetTypeSimplePromoProductAfterDesc;
            }

            var bannerType = ChargeAfterHelper.GetPromoWidgetType(widgetType);

            return View("~/Plugins/Payments.ChargeAfter/Views/Promo/PromoSimple.cshtml", (widgetZone, bannerType));
        }

        #endregion
    }
}
