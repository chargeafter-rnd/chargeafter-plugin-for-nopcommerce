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
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.ChargeAfter.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class PaymentChargeAfterController : BasePaymentController
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ServiceManager _serviceManager;

        #endregion

        #region Ctor

        public PaymentChargeAfterController(
            ILanguageService languageService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            ServiceManager serviceManager
        ) {
            _languageService = languageService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _serviceManager = serviceManager;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> ConfigureAsync()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var chargeAfterPaymentSettings = await _settingService.LoadSettingAsync<ChargeAfterPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseProduction = chargeAfterPaymentSettings.UseProduction,

                ProductionPublicKey = chargeAfterPaymentSettings.ProductionPublicKey,
                ProductionPrivateKey = chargeAfterPaymentSettings.ProductionPrivateKey,

                SandboxPublicKey = chargeAfterPaymentSettings.SandboxPublicKey,
                SandboxPrivateKey = chargeAfterPaymentSettings.SandboxPrivateKey,

                AdditionalFee = chargeAfterPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = chargeAfterPaymentSettings.AdditionalFeePercentage,

                EnableLineOfCreditPromo = chargeAfterPaymentSettings.EnableLineOfCreditPromo,
                FinancingPageUrlLineOfCreditPromo = chargeAfterPaymentSettings.FinancingPageUrlLineOfCreditPromo,

                EnableSimplePromoBeforeContent = chargeAfterPaymentSettings.EnableSimplePromoBeforeContent,
                WidgetTypeSimplePromoBeforeContentId = Convert.ToInt32(chargeAfterPaymentSettings.WidgetTypeSimplePromoBeforeContent),
                WidgetTypeSimplePromoBeforeContentValues = await chargeAfterPaymentSettings.WidgetTypeSimplePromoBeforeContent.ToSelectListAsync(),

                EnableSimplePromoAfterContent = chargeAfterPaymentSettings.EnableSimplePromoAfterContent,
                WidgetTypeSimplePromoAfterContentId = Convert.ToInt32(chargeAfterPaymentSettings.WidgetTypeSimplePromoAfterContent),
                WidgetTypeSimplePromoAfterContentValues = await chargeAfterPaymentSettings.WidgetTypeSimplePromoAfterContent.ToSelectListAsync(),

                EnableSimplePromoProductBeforeContent = chargeAfterPaymentSettings.EnableSimplePromoProductBeforeContent,
                WidgetTypeSimplePromoProductBeforeContentId = Convert.ToInt32(chargeAfterPaymentSettings.WidgetTypeSimplePromoProductBeforeContent),
                WidgetTypeSimplePromoProductBeforeContentValues = await chargeAfterPaymentSettings.WidgetTypeSimplePromoProductBeforeContent.ToSelectListAsync(),

                EnableSimplePromoProductAfterTitle = chargeAfterPaymentSettings.EnableSimplePromoProductAfterTitle,
                WidgetTypeSimplePromoProductAfterTitleId = Convert.ToInt32(chargeAfterPaymentSettings.WidgetTypeSimplePromoProductAfterTitle),
                WidgetTypeSimplePromoProductAfterTitleValues = await chargeAfterPaymentSettings.WidgetTypeSimplePromoProductAfterTitle.ToSelectListAsync(),

                EnableSimplePromoProductAfterDesc = chargeAfterPaymentSettings.EnableSimplePromoProductAfterDesc,
                WidgetTypeSimplePromoProductAfterDescId = Convert.ToInt32(chargeAfterPaymentSettings.WidgetTypeSimplePromoProductAfterDesc),
                WidgetTypeSimplePromoProductAfterDescValues = await chargeAfterPaymentSettings.WidgetTypeSimplePromoProductAfterDesc.ToSelectListAsync(),

                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.UseProduction_OverrideForStore = await _settingService.SettingExistsAsync(chargeAfterPaymentSettings, x => x.UseProduction, storeScope);

                model.ProductionPublicKey_OverrideForStore = await _settingService.SettingExistsAsync(chargeAfterPaymentSettings, x => x.ProductionPublicKey, storeScope);
                model.ProductionPrivateKey_OverrideForStore = await _settingService.SettingExistsAsync(chargeAfterPaymentSettings, x => x.ProductionPrivateKey, storeScope);                
                
                model.SandboxPublicKey_OverrideForStore = await _settingService.SettingExistsAsync(chargeAfterPaymentSettings, x => x.SandboxPublicKey, storeScope);
                model.SandboxPrivateKey_OverrideForStore = await _settingService.SettingExistsAsync(chargeAfterPaymentSettings, x => x.SandboxPrivateKey, storeScope);
                
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(chargeAfterPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(chargeAfterPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                
                model.EnableLineOfCreditPromo_OverrideForStore = await _settingService.SettingExistsAsync(chargeAfterPaymentSettings, x => x.EnableLineOfCreditPromo, storeScope);
                model.FinancingPageUrlLineOfCreditPromo_OverrideForStore = await _settingService.SettingExistsAsync(chargeAfterPaymentSettings, x => x.FinancingPageUrlLineOfCreditPromo, storeScope);

                model.EnableSimplePromoBeforeContent_OverrideForStore = await _settingService.SettingExistsAsync(chargeAfterPaymentSettings, x => x.EnableSimplePromoBeforeContent, storeScope);
                model.WidgetTypeSimplePromoBeforeContentId_OverrideForStore = await _settingService.SettingExistsAsync(chargeAfterPaymentSettings, x => x.WidgetTypeSimplePromoBeforeContent, storeScope);

                model.EnableSimplePromoAfterContent_OverrideForStore = await _settingService.SettingExistsAsync(chargeAfterPaymentSettings, x => x.EnableSimplePromoAfterContent, storeScope);
                model.WidgetTypeSimplePromoAfterContentId_OverrideForStore = await _settingService.SettingExistsAsync(chargeAfterPaymentSettings, x => x.WidgetTypeSimplePromoAfterContent, storeScope);

                model.EnableSimplePromoProductBeforeContent_OverrideForStore = await _settingService.SettingExistsAsync(chargeAfterPaymentSettings, x => x.EnableSimplePromoProductBeforeContent, storeScope);
                model.WidgetTypeSimplePromoProductBeforeContentId_OverrideForStore = await _settingService.SettingExistsAsync(chargeAfterPaymentSettings, x => x.WidgetTypeSimplePromoProductBeforeContent, storeScope);

                model.EnableSimplePromoProductAfterTitle_OverrideForStore = await _settingService.SettingExistsAsync(chargeAfterPaymentSettings, x => x.EnableSimplePromoProductAfterTitle, storeScope);
                model.WidgetTypeSimplePromoProductAfterTitleId_OverrideForStore = await _settingService.SettingExistsAsync(chargeAfterPaymentSettings, x => x.WidgetTypeSimplePromoProductAfterTitle, storeScope);

                model.EnableSimplePromoProductAfterDesc_OverrideForStore = await _settingService.SettingExistsAsync(chargeAfterPaymentSettings, x => x.EnableSimplePromoProductAfterDesc, storeScope);
                model.WidgetTypeSimplePromoProductAfterDescId_OverrideForStore = await _settingService.SettingExistsAsync(chargeAfterPaymentSettings, x => x.WidgetTypeSimplePromoProductAfterDesc, storeScope);
            }

            // ReSharper disable once Mvc.ViewNotResolved
            return View("~/Plugins/Payments.ChargeAfter/Views/Configuration/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfigureAsync(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await ConfigureAsync();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var chargeAfterPaymentSettings = await _settingService.LoadSettingAsync<ChargeAfterPaymentSettings>(storeScope);

            //save settings
            chargeAfterPaymentSettings.UseProduction = model.UseProduction;

            chargeAfterPaymentSettings.ProductionPublicKey = model.ProductionPublicKey;
            chargeAfterPaymentSettings.ProductionPrivateKey = model.ProductionPrivateKey;

            chargeAfterPaymentSettings.SandboxPublicKey = model.SandboxPublicKey;
            chargeAfterPaymentSettings.SandboxPrivateKey = model.SandboxPrivateKey;

            chargeAfterPaymentSettings.AdditionalFee = 0;
            chargeAfterPaymentSettings.AdditionalFeePercentage = false;

            chargeAfterPaymentSettings.EnableLineOfCreditPromo = model.EnableLineOfCreditPromo;
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

            chargeAfterPaymentSettings.BrandId = GetBrandId(chargeAfterPaymentSettings);

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(chargeAfterPaymentSettings, x => x.UseProduction, model.UseProduction_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(chargeAfterPaymentSettings, x => x.ProductionPublicKey, model.ProductionPublicKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chargeAfterPaymentSettings, x => x.ProductionPrivateKey, model.ProductionPrivateKey_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(chargeAfterPaymentSettings, x => x.SandboxPublicKey, model.SandboxPublicKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chargeAfterPaymentSettings, x => x.SandboxPrivateKey, model.SandboxPrivateKey_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(chargeAfterPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chargeAfterPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(chargeAfterPaymentSettings, x => x.BrandId, model.BrandId_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(chargeAfterPaymentSettings, x => x.EnableLineOfCreditPromo, model.EnableLineOfCreditPromo_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chargeAfterPaymentSettings, x => x.FinancingPageUrlLineOfCreditPromo, model.FinancingPageUrlLineOfCreditPromo_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(chargeAfterPaymentSettings, x => x.EnableSimplePromoBeforeContent, model.EnableSimplePromoBeforeContent_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chargeAfterPaymentSettings, x => x.WidgetTypeSimplePromoBeforeContent, model.WidgetTypeSimplePromoBeforeContentId_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(chargeAfterPaymentSettings, x => x.EnableSimplePromoAfterContent, model.EnableSimplePromoAfterContent_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chargeAfterPaymentSettings, x => x.WidgetTypeSimplePromoAfterContent, model.WidgetTypeSimplePromoAfterContentId_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(chargeAfterPaymentSettings, x => x.EnableSimplePromoProductBeforeContent, model.EnableSimplePromoProductBeforeContent_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chargeAfterPaymentSettings, x => x.WidgetTypeSimplePromoProductBeforeContent, model.WidgetTypeSimplePromoProductBeforeContentId_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(chargeAfterPaymentSettings, x => x.EnableSimplePromoProductAfterTitle, model.EnableSimplePromoProductAfterTitle_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chargeAfterPaymentSettings, x => x.WidgetTypeSimplePromoProductAfterTitle, model.WidgetTypeSimplePromoProductAfterTitleId_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(chargeAfterPaymentSettings, x => x.EnableSimplePromoProductAfterDesc, model.EnableSimplePromoProductAfterDesc_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(chargeAfterPaymentSettings, x => x.WidgetTypeSimplePromoProductAfterDesc, model.WidgetTypeSimplePromoProductAfterDescId_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //notification
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await ConfigureAsync();
        }


        public string GetBrandId(ChargeAfterPaymentSettings chargeAfterPaymentSettings, string defaultBrandId = "")
        {
            var sessionKey = ChargeAfterHelper.GetPublicKeyFromSettings(chargeAfterPaymentSettings);
            if (string.IsNullOrEmpty(sessionKey))
                return defaultBrandId;

            var merchantId = HttpContext.Session.Get<string>(sessionKey);

            if(string.IsNullOrEmpty(merchantId)) { 
                var (session, error_session) = _serviceManager.CreateSession(chargeAfterPaymentSettings);
                if (!string.IsNullOrEmpty(error_session) || string.IsNullOrEmpty(session.SessionId))
                    return defaultBrandId;

                var sessionId = session.SessionId;
            
                var (merchant, error_merchant) = _serviceManager.GetMerchantInfoBySessionId(chargeAfterPaymentSettings, sessionId);
                if (!string.IsNullOrEmpty(error_merchant) || merchant == null || merchant.Data.MerchantId == null)
                    return defaultBrandId;

                merchantId = merchant.Data.MerchantId;

                HttpContext.Session.Set<string>(sessionKey, merchantId);
            }

            var (merchantSettings, error_settings) = _serviceManager.GetMerchantSettingsById(chargeAfterPaymentSettings, merchantId);
            if (!string.IsNullOrEmpty(error_settings) || string.IsNullOrEmpty(merchantSettings.BrandId))
                return defaultBrandId;

            return merchantSettings.BrandId.ToLower();
        }

        #endregion

    }
}
