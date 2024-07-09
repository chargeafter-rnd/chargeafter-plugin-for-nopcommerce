using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.ChargeAfter.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.ChargeAfter.Areas.Admin.Controllers
{
    public class PaymentChargeAfterProductController : ProductController
    {
        #region Fields

        private IProductService _productService;
        private ICustomProductAttributeService _customProductAttributeService;

        #endregion

        #region Ctor

        public PaymentChargeAfterProductController(
            IAclService aclService,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            ICategoryService categoryService,
            ICopyProductService copyProductService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDiscountService discountService,
            IDownloadService downloadService,
            IExportManager exportManager,
            IGenericAttributeService genericAttributeService,
            IHttpClientFactory httpClientFactory,
            IImportManager importManager,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            IManufacturerService manufacturerService,
            INopFileProvider fileProvider,
            INotificationService notificationService,
            IPdfService pdfService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IProductTagService productTagService,
            ISettingService settingService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            ISpecificationAttributeService specificationAttributeService,
            IStoreContext storeContext,
            IUrlRecordService urlRecordService,
            IVideoService videoService,
            IWebHelper webHelper,
            IWorkContext workContext,
            VendorSettings vendorSettings,
            ICustomProductAttributeService customProductAttributeService) : base(
                aclService,
                backInStockSubscriptionService,
                categoryService,
                copyProductService,
                customerActivityService,
                customerService,
                discountService,
                downloadService,
                exportManager,
                genericAttributeService,
                httpClientFactory,
                importManager,
                languageService,
                localizationService,
                localizedEntityService,
                manufacturerService,
                fileProvider,
                notificationService,
                pdfService,
                permissionService,
                pictureService,
                productAttributeFormatter,
                productAttributeParser,
                productAttributeService,
                productModelFactory,
                productService,
                productTagService,
                settingService,
                shippingService,
                shoppingCartService,
                specificationAttributeService,
                storeContext,
                urlRecordService,
                videoService,
                webHelper,
                workContext,
                vendorSettings)
        {
            _productService = productService;
            _customProductAttributeService = customProductAttributeService;
        }

        #endregion

        #region Method

        public override async Task<IActionResult> Edit(ProductModel model, bool continueEditing)
        {
            var action = await base.Edit(model, continueEditing);
            if (ModelState.IsValid)
            {
                var product = await _productService.GetProductByIdAsync(model.Id);

                if(HttpContext.Request.Form.TryGetValue("CaNonLeasable", out var nonLeasableValues))
                {
                    await _customProductAttributeService.SetNonLeasableAttributeValueAsync(product, Convert.ToBoolean(nonLeasableValues[0]));
                }

                if(HttpContext.Request.Form.TryGetValue("CaWarranty", out var warrantyValues))
                {
                    await _customProductAttributeService.SetWarrantyAttributeValueAsync(product, Convert.ToBoolean(warrantyValues[0]));
                }
            }

            return action;
        }

        #endregion
    }
}