﻿using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.ChargeAfter.Models;
using Nop.Plugin.Payments.ChargeAfter.Services;
using Nop.Services.Catalog;
using Nop.Services.Payments;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.ChargeAfter.Components
{
    [ViewComponent(Name = Defaults.ADMIN_PRODUCT_VIEW_COMPONENT_NAME)]
    public class ChargeAfterAdminProductViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly INonLeasableService _nonLeasableService;
        
        #endregion

        #region Ctor

        public ChargeAfterAdminProductViewComponent(
            IProductService productService,
            IPaymentPluginManager paymentPluginManager,
            IStoreContext storeContext,
            IWorkContext workContext,
            INonLeasableService nonLeasableService
        )
        {
            _productService = productService;
            _paymentPluginManager = paymentPluginManager;
            _storeContext = storeContext;
            _workContext = workContext;
            _nonLeasableService = nonLeasableService;
        }

        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!await _paymentPluginManager.IsPluginActiveAsync(Defaults.SystemName,
                                                                 await _workContext.GetCurrentCustomerAsync(),
                                                                 _storeContext.GetCurrentStore().Id))
                return Content(string.Empty);

            //ensure that it's a proper widget zone
            if (!widgetZone.Equals(AdminWidgetZones.ProductDetailsBlock))
                return Content(string.Empty);

            if (!(additionalData is Web.Areas.Admin.Models.Catalog.ProductModel productModel))
                return Content(string.Empty);

            var model = new NonLeasableProductModel { ProductId = productModel.Id };
            if (model.ProductId > 0)
            {
                var product = await _productService.GetProductByIdAsync(model.ProductId);
                model.CaNonLeasable = await _nonLeasableService.GetAttributeValueAsync(product);
            }

            return View("~/Plugins/Payments.ChargeAfter/Areas/Admin/Views/Product.cshtml", model);
        }

        #endregion
    }
}

