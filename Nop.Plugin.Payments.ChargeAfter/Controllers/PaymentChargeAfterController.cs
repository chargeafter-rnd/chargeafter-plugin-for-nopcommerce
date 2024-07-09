using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Payments.ChargeAfter.Domain;
using Nop.Plugin.Payments.ChargeAfter.Models;
using Nop.Plugin.Payments.ChargeAfter.Services;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;

namespace Nop.Plugin.Payments.ChargeAfter.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class PaymentChargeAfterController : BasePaymentController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        
        #endregion

        #region Ctor

        public PaymentChargeAfterController(
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext
        ) {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var chargeAfterPaymentSettings = _settingService.LoadSetting<ChargeAfterPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseProduction = chargeAfterPaymentSettings.UseProduction,

                ProductionPublicKey = chargeAfterPaymentSettings.ProductionPublicKey,
                ProductionPrivateKey = chargeAfterPaymentSettings.ProductionPrivateKey,

                SandboxPublicKey = chargeAfterPaymentSettings.SandboxPublicKey,
                SandboxPrivateKey = chargeAfterPaymentSettings.SandboxPrivateKey,

                AdditionalFee = chargeAfterPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = chargeAfterPaymentSettings.AdditionalFeePercentage,

                TypeTransaction = (int)chargeAfterPaymentSettings.TypeTransaction,
                TypeTransactionValues = chargeAfterPaymentSettings.TypeTransaction.ToSelectList(),

                TypeCheckoutBrand = (int)chargeAfterPaymentSettings.TypeCheckoutBrand,
                TypeCheckoutBrandValues = chargeAfterPaymentSettings.TypeCheckoutBrand.ToSelectList(),

                EnableLineOfCreditPromo = chargeAfterPaymentSettings.EnableLineOfCreditPromo,
                TypeLineOfCreditPromo = (int)chargeAfterPaymentSettings.TypeLineOfCreditPromo,
                TypeLineOfCreditPromoValues = chargeAfterPaymentSettings.TypeLineOfCreditPromo.ToSelectList(),
                FinancingPageUrlLineOfCreditPromo = chargeAfterPaymentSettings.FinancingPageUrlLineOfCreditPromo,

                EnableSimplePromoBeforeContent = chargeAfterPaymentSettings.EnableSimplePromoBeforeContent,
                WidgetTypeSimplePromoBeforeContentId = Convert.ToInt32(chargeAfterPaymentSettings.WidgetTypeSimplePromoBeforeContent),
                WidgetTypeSimplePromoBeforeContentValues = chargeAfterPaymentSettings.WidgetTypeSimplePromoBeforeContent.ToSelectList(),

                EnableSimplePromoAfterContent = chargeAfterPaymentSettings.EnableSimplePromoAfterContent,
                WidgetTypeSimplePromoAfterContentId = Convert.ToInt32(chargeAfterPaymentSettings.WidgetTypeSimplePromoAfterContent),
                WidgetTypeSimplePromoAfterContentValues = chargeAfterPaymentSettings.WidgetTypeSimplePromoAfterContent.ToSelectList(),

                EnableSimplePromoProductBeforeContent = chargeAfterPaymentSettings.EnableSimplePromoProductBeforeContent,
                WidgetTypeSimplePromoProductBeforeContentId = Convert.ToInt32(chargeAfterPaymentSettings.WidgetTypeSimplePromoProductBeforeContent),
                WidgetTypeSimplePromoProductBeforeContentValues = chargeAfterPaymentSettings.WidgetTypeSimplePromoProductBeforeContent.ToSelectList(),

                EnableSimplePromoProductAfterTitle = chargeAfterPaymentSettings.EnableSimplePromoProductAfterTitle,
                WidgetTypeSimplePromoProductAfterTitleId = Convert.ToInt32(chargeAfterPaymentSettings.WidgetTypeSimplePromoProductAfterTitle),
                WidgetTypeSimplePromoProductAfterTitleValues = chargeAfterPaymentSettings.WidgetTypeSimplePromoProductAfterTitle.ToSelectList(),

                EnableSimplePromoProductAfterDesc = chargeAfterPaymentSettings.EnableSimplePromoProductAfterDesc,
                WidgetTypeSimplePromoProductAfterDescId = Convert.ToInt32(chargeAfterPaymentSettings.WidgetTypeSimplePromoProductAfterDesc),
                WidgetTypeSimplePromoProductAfterDescValues = chargeAfterPaymentSettings.WidgetTypeSimplePromoProductAfterDesc.ToSelectList(),

                EnableAdvancedSetting = chargeAfterPaymentSettings.EnableAdvancedSetting,

                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.UseProduction_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.UseProduction, storeScope);

                model.ProductionPublicKey_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.ProductionPublicKey, storeScope);
                model.ProductionPrivateKey_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.ProductionPrivateKey, storeScope);                
                
                model.SandboxPublicKey_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.SandboxPublicKey, storeScope);
                model.SandboxPrivateKey_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.SandboxPrivateKey, storeScope);

                model.TypeCheckoutBrand_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.TypeCheckoutBrand, storeScope);

                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.AdditionalFeePercentage, storeScope);

                model.TypeTransaction_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.TypeTransaction, storeScope);

                model.EnableLineOfCreditPromo_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.EnableLineOfCreditPromo, storeScope);
                model.TypeLineOfCreditPromo_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.TypeLineOfCreditPromo, storeScope);
                model.FinancingPageUrlLineOfCreditPromo_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.FinancingPageUrlLineOfCreditPromo, storeScope);

                model.EnableSimplePromoBeforeContent_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.EnableSimplePromoBeforeContent, storeScope);
                model.WidgetTypeSimplePromoBeforeContentId_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.WidgetTypeSimplePromoBeforeContent, storeScope);

                model.EnableSimplePromoAfterContent_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.EnableSimplePromoAfterContent, storeScope);
                model.WidgetTypeSimplePromoAfterContentId_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.WidgetTypeSimplePromoAfterContent, storeScope);

                model.EnableSimplePromoProductBeforeContent_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.EnableSimplePromoProductBeforeContent, storeScope);
                model.WidgetTypeSimplePromoProductBeforeContentId_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.WidgetTypeSimplePromoProductBeforeContent, storeScope);

                model.EnableSimplePromoProductAfterTitle_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.EnableSimplePromoProductAfterTitle, storeScope);
                model.WidgetTypeSimplePromoProductAfterTitleId_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.WidgetTypeSimplePromoProductAfterTitle, storeScope);

                model.EnableSimplePromoProductAfterDesc_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.EnableSimplePromoProductAfterDesc, storeScope);
                model.WidgetTypeSimplePromoProductAfterDescId_OverrideForStore = _settingService.SettingExists(chargeAfterPaymentSettings, x => x.WidgetTypeSimplePromoProductAfterDesc, storeScope);
            }

            // ReSharper disable once Mvc.ViewNotResolved
            return View("~/Plugins/Payments.ChargeAfter/Areas/Admin/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var chargeAfterPaymentSettings = _settingService.LoadSetting<ChargeAfterPaymentSettings>(storeScope);

            //save settings
            chargeAfterPaymentSettings.UseProduction = model.UseProduction;

            chargeAfterPaymentSettings.ProductionPublicKey = model.ProductionPublicKey;
            chargeAfterPaymentSettings.ProductionPrivateKey = model.ProductionPrivateKey;

            chargeAfterPaymentSettings.SandboxPublicKey = model.SandboxPublicKey;
            chargeAfterPaymentSettings.SandboxPrivateKey = model.SandboxPrivateKey;

            chargeAfterPaymentSettings.AdditionalFee = 0;
            chargeAfterPaymentSettings.AdditionalFeePercentage = false;

            chargeAfterPaymentSettings.TypeTransaction = (Domain.TransactionType)model.TypeTransaction;

            chargeAfterPaymentSettings.TypeCheckoutBrand = (Domain.Promo.CheckoutBrandType)model.TypeCheckoutBrand;

            chargeAfterPaymentSettings.EnableLineOfCreditPromo = model.EnableLineOfCreditPromo;
            chargeAfterPaymentSettings.TypeLineOfCreditPromo = (Domain.Promo.LineOfCreditType)model.TypeLineOfCreditPromo;
            chargeAfterPaymentSettings.FinancingPageUrlLineOfCreditPromo = model.FinancingPageUrlLineOfCreditPromo;

            chargeAfterPaymentSettings.EnableSimplePromoBeforeContent = model.EnableSimplePromoBeforeContent;
            chargeAfterPaymentSettings.WidgetTypeSimplePromoBeforeContent = (PromoWidgetType)model.WidgetTypeSimplePromoBeforeContentId;

            chargeAfterPaymentSettings.EnableSimplePromoAfterContent = model.EnableSimplePromoAfterContent;
            chargeAfterPaymentSettings.WidgetTypeSimplePromoAfterContent = (PromoWidgetType)model.WidgetTypeSimplePromoAfterContentId;

            chargeAfterPaymentSettings.EnableSimplePromoProductBeforeContent = model.EnableSimplePromoProductBeforeContent;
            chargeAfterPaymentSettings.WidgetTypeSimplePromoProductBeforeContent = (PromoWidgetType)model.WidgetTypeSimplePromoProductBeforeContentId;

            chargeAfterPaymentSettings.EnableSimplePromoProductAfterTitle = model.EnableSimplePromoProductAfterTitle;
            chargeAfterPaymentSettings.WidgetTypeSimplePromoProductAfterTitle = (PromoWidgetType)model.WidgetTypeSimplePromoProductAfterTitleId;

            chargeAfterPaymentSettings.EnableSimplePromoProductAfterDesc = model.EnableSimplePromoProductAfterDesc;
            chargeAfterPaymentSettings.WidgetTypeSimplePromoProductAfterDesc = (PromoWidgetType)model.WidgetTypeSimplePromoProductAfterDescId;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.UseProduction, model.UseProduction_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.ProductionPublicKey, model.ProductionPublicKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.ProductionPrivateKey, model.ProductionPrivateKey_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.SandboxPublicKey, model.SandboxPublicKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.SandboxPrivateKey, model.SandboxPrivateKey_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.TypeTransaction, model.TypeTransaction_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.TypeCheckoutBrand, model.TypeCheckoutBrand_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.EnableLineOfCreditPromo, model.EnableLineOfCreditPromo_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.TypeLineOfCreditPromo, model.TypeLineOfCreditPromo_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.FinancingPageUrlLineOfCreditPromo, model.FinancingPageUrlLineOfCreditPromo_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.EnableSimplePromoBeforeContent, model.EnableSimplePromoBeforeContent_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.WidgetTypeSimplePromoBeforeContent, model.WidgetTypeSimplePromoBeforeContentId_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.EnableSimplePromoAfterContent, model.EnableSimplePromoAfterContent_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.WidgetTypeSimplePromoAfterContent, model.WidgetTypeSimplePromoAfterContentId_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.EnableSimplePromoProductBeforeContent, model.EnableSimplePromoProductBeforeContent_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.WidgetTypeSimplePromoProductBeforeContent, model.WidgetTypeSimplePromoProductBeforeContentId_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.EnableSimplePromoProductAfterTitle, model.EnableSimplePromoProductAfterTitle_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.WidgetTypeSimplePromoProductAfterTitle, model.WidgetTypeSimplePromoProductAfterTitleId_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.EnableSimplePromoProductAfterDesc, model.EnableSimplePromoProductAfterDesc_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.WidgetTypeSimplePromoProductAfterDesc, model.WidgetTypeSimplePromoProductAfterDescId_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            //notification
            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [HttpPost]
        public IActionResult SavePreferenceMode(bool value)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var chargeAfterPaymentSettings = _settingService.LoadSetting<ChargeAfterPaymentSettings>(storeScope);

            //save settings
            chargeAfterPaymentSettings.EnableAdvancedSetting = value;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(chargeAfterPaymentSettings, x => x.EnableAdvancedSetting, value, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            return Json(new { Result = true });
        }

        #endregion

    }
}
