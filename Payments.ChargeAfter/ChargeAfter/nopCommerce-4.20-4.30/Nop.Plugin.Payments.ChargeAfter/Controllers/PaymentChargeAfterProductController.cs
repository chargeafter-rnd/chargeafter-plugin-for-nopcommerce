﻿using Microsoft.AspNetCore.Mvc;
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
            IWorkContext workContext, 
            VendorSettings vendorSettings,
            ICustomProductAttributeService customProductAttributeService
        ) : base(aclService,
                 backInStockSubscriptionService,
                 categoryService,
                 copyProductService,
                 customerActivityService,
                 customerService,
                 discountService,
                 downloadService,
                 exportManager,
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
                 workContext,
                 vendorSettings)
        {
            _productService = productService;
            _customProductAttributeService = customProductAttributeService;
        }

        #endregion

        #region Method

        public override IActionResult Edit(ProductModel model, bool continueEditing)
        {
            var action = base.Edit(model, continueEditing);

            if(ModelState.IsValid)
            {
                var product = _productService.GetProductById(model.Id);

                if (HttpContext.Request.Form.TryGetValue("CaNonLeasable", out var nonLeasableValues))
                {
                    _customProductAttributeService.SetNonLeasableAttributeValue(product, Convert.ToBoolean(nonLeasableValues[0]));
                }

                if (HttpContext.Request.Form.TryGetValue("CaWarranty", out var warrantyValues))
                {
                     _customProductAttributeService.SetWarrantyAttributeValue(product, Convert.ToBoolean(warrantyValues[0]));
                }
            }

            return action;
        }

        #endregion
    }
}
