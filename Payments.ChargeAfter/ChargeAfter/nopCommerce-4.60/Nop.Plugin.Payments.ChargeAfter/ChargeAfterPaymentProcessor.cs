using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.ChargeAfter.Payments;
using Nop.Plugin.Payments.ChargeAfter.Services;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using System;
using System.Collections.Generic;
using Nop.Plugin.Payments.ChargeAfter.Infrastructure;
using System.Threading.Tasks;
using Nop.Plugin.Payments.ChargeAfter.Components;

namespace Nop.Plugin.Payments.ChargeAfter 
{
    public class ChargeAfterPaymentProcessor : BasePlugin, IPaymentMethod, IWidgetPlugin
    {
        #region Fields

        private readonly ChargeAfterPaymentSettings _chargeAfterPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IOrderTaxService _orderTaxService;
        private readonly IOrderSaleService _orderSaleService;
        private readonly IWorkContext _workContext;
        private readonly WidgetSettings _widgetSettings;
        private readonly ServiceManager _serviceManager;

        #endregion

        #region Ctor

        public ChargeAfterPaymentProcessor(
            ChargeAfterPaymentSettings chargeAfterPaymentSettings,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            ISettingService settingService,
            IAddressService addressService,
            ICountryService countryService,
            IOrderTaxService orderTaxService,
            IOrderSaleService orderSaleService,
            WidgetSettings widgetSettings,
            ServiceManager serviceManager,
            IWorkContext workContext
        ) {
            _chargeAfterPaymentSettings = chargeAfterPaymentSettings;
            _localizationService = localizationService;
            _addressService = addressService;
            _webHelper = webHelper;
            _settingService = settingService;
            _countryService = countryService;
            _orderTaxService = orderTaxService;
            _orderSaleService = orderSaleService;
            _widgetSettings = widgetSettings;
            _serviceManager = serviceManager;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();

            try
            {
                if(!processPaymentRequest.CustomValues.TryGetValue(Constants.CA_TOKEN_KEY, out var confirmationToken))
                {
                    throw new NopException("Failed to get the ChargeAfter Confirmation Token");
                }

                var (response, error) = _serviceManager.Authorization(_chargeAfterPaymentSettings, confirmationToken.ToString());
                if (!string.IsNullOrEmpty(error))
                {
                    throw new NopException(error);
                }

                result.AuthorizationTransactionId = response.ChargeId;
                result.AuthorizationTransactionResult = response.State;
                result.AuthorizationTransactionCode = confirmationToken.ToString();
                result.NewPaymentStatus = PaymentStatus.Authorized;
            }
            catch (Exception ex)
            {
                result.AddError(ex.Message);
            }

            return Task.FromResult(result);
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            if (!string.IsNullOrEmpty(postProcessPaymentRequest.Order.AuthorizationTransactionId))
            {
                var chargeId = postProcessPaymentRequest.Order.AuthorizationTransactionId;
                var order = postProcessPaymentRequest.Order as Order;

                if (order == null)
                {
                    throw new NopException("Invalid order data");
                }

                // Update order id for charge
                var (response, error) = _serviceManager.SetMerchantOrderId(_chargeAfterPaymentSettings, chargeId, order.Id.ToString());
                if (!string.IsNullOrEmpty(error))
                {
                    throw new NopException(error);
                }

                // Update tax if LTO
                var chargeTotal = response.TotalAmount;
                if (chargeTotal > 0 && order.OrderTotal > 0 && (order.OrderTotal > chargeTotal))
                {
                    var orderTax = order.OrderTax;
                    if (orderTax > 0 && (chargeTotal + order.OrderTax == order.OrderTotal))
                    {
                        await _orderTaxService.UpdateTaxFreeAsync(order.Id);
                    }
                }

                // Auto capture
                if (_chargeAfterPaymentSettings.UseAutoCapture)
                {
                    await _orderSaleService.CaptureAsync(order.Id);
                }
            }
        }

        public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            
            bool allowCountry = false; 
            bool allowSettings = false;
          
            if(customer != null && customer.BillingAddressId.HasValue)
            {
                var billingAddress = await _addressService.GetAddressByIdAsync((int)customer.BillingAddressId);
                
                if (billingAddress != null && billingAddress.CountryId.HasValue)
                {
                    var country = await _countryService.GetCountryByIdAsync((int)billingAddress.CountryId);

                    if(country != null && country.ThreeLetterIsoCode == "USA")
                    {
                        allowCountry = true;
                    }
                }
            }

            if (!string.IsNullOrEmpty(ChargeAfterHelper.GetPublicKeyFromSettings(_chargeAfterPaymentSettings)) && 
                !string.IsNullOrEmpty(ChargeAfterHelper.GetPrivateKeyFromSettings(_chargeAfterPaymentSettings)))
            {
                allowSettings = true;
            }

            return !(allowCountry && allowSettings);
        }

        public Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(decimal.Zero);
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            if(string.IsNullOrEmpty(capturePaymentRequest.Order.AuthorizationTransactionId))
                return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Failed to get the ChargeAfter Charge" } });

            var chargeId = capturePaymentRequest.Order.AuthorizationTransactionId;
            
            var (response, error) = _serviceManager.GetChargeById(_chargeAfterPaymentSettings, chargeId);
            if (!string.IsNullOrEmpty(error))
                return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Failed to get the ChargeAfter Charge" } });

            if(response.State != ChargeState.AUTHORIZED)
                return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Failed. Charge not authorized" } });

            var (capture_response, capture_error) = _serviceManager.Capture(_chargeAfterPaymentSettings, chargeId, response.TotalAmount);
            if (!string.IsNullOrEmpty(capture_error))
                return Task.FromResult(new CapturePaymentResult { Errors = new[] { capture_error } });

            return Task.FromResult(new CapturePaymentResult 
            {
                CaptureTransactionId = response.ChargeId,
                CaptureTransactionResult = ChargeState.SETTLED,
                NewPaymentStatus = PaymentStatus.Paid
            });
        }

        public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            if (string.IsNullOrEmpty(refundPaymentRequest.Order.CaptureTransactionId))
                return Task.FromResult(new RefundPaymentResult { Errors = new[] { "Charge not settled for refund" } });

            var chargeId = refundPaymentRequest.Order.CaptureTransactionId;
            var refundAmount = refundPaymentRequest.AmountToRefund;

            var (response, error) = _serviceManager.GetChargeById(_chargeAfterPaymentSettings, chargeId);
            if (!string.IsNullOrEmpty(error))
                return Task.FromResult(new RefundPaymentResult { Errors = new[] { "Failed to get the ChargeAfter Charge" } });

            if (response.State == ChargeState.REFUNDED)
                return Task.FromResult(new RefundPaymentResult { Errors = new[] { "Refund error occured. Charge fully refunded" } });

            var availableToRefund = response.SettledAmount - response.RefundedAmount;
            if (refundAmount > availableToRefund)
            {
                refundAmount = availableToRefund;
            }

            if (refundAmount > 0) {
                var (_, refund_error) = _serviceManager.Refund(_chargeAfterPaymentSettings, chargeId, refundAmount);
                if (!string.IsNullOrEmpty(refund_error)) { 
                    return Task.FromResult(new RefundPaymentResult { Errors = new[] { refund_error } });
                }
            }

            return Task.FromResult(new RefundPaymentResult
            {
                NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded
            });
        }

        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            if (string.IsNullOrEmpty(voidPaymentRequest.Order.AuthorizationTransactionId))
                return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Failed to get the ChargeAfter Charge" } });

            var chargeId = voidPaymentRequest.Order.AuthorizationTransactionId;
            var result = new VoidPaymentResult 
            { 
                NewPaymentStatus = PaymentStatus.Voided 
            };

            var (response, error) = _serviceManager.GetChargeById(_chargeAfterPaymentSettings, chargeId);
            if (!string.IsNullOrEmpty(error))
                return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Failed to get the ChargeAfter Charge" } });

            if(response.State == ChargeState.AUTHORIZED)
            {
                var (void_response, void_error) = _serviceManager.Void(_chargeAfterPaymentSettings, chargeId);
                if (!string.IsNullOrEmpty(void_error))
                    return Task.FromResult(new VoidPaymentResult { Errors = new[] { void_error } });

                return Task.FromResult(result);
            } 
            else if(response.State == ChargeState.SETTLED || response.State == ChargeState.PARTIALLY_REFUNDED)
            {
                var amount = response.TotalAmount - response.RefundedAmount;
                var (refund_response, refund_error) = _serviceManager.Refund(_chargeAfterPaymentSettings, chargeId, amount);
                if (!string.IsNullOrEmpty(refund_error))
                    return Task.FromResult(new VoidPaymentResult { Errors = new[] { refund_error } });

                return Task.FromResult(result);
            }

            return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void error occured" } });
        }

        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return Task.FromResult(new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //it's not a redirection payment method. So we always return false
            return Task.FromResult(false);
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            return Task.FromResult(new ProcessPaymentRequest());
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}{Defaults.ConfigurationRouteName}";
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("Plugins.Payment.ChargeAfter.PaymentMethodDescription");
        }

        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new ChargeAfterPaymentSettings {
                UseProduction = false
            });

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(Defaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(Defaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Payment.ChargeAfter.PaymentMethodDescription"] = ".",

                ["Plugins.Payment.ChargeAfter.PaymentMethod.Tip"] = "You will be redirected to complete the order.",

                ["Plugins.Payments.ChargeAfter.Header.Payment"] = "Payment",
                ["Plugins.Payments.ChargeAfter.Header.Checkout"] = "Checkout",
                ["Plugins.Payments.ChargeAfter.Header.PromoLineOfCredit"] = "Promo: Line of Credit",
                ["Plugins.Payments.ChargeAfter.Header.PromoSimple"] = "Promo: Simple Widgets",
                ["Plugins.Payments.ChargeAfter.Header.ProductAttr"] = "Consumer Financing attributes",

                ["Plugins.Payments.ChargeAfter.Fields.ProductionPublicKey"] = "Production Public Key",
                ["Plugins.Payments.ChargeAfter.Fields.ProductionPublicKey.Hint"] = "The REST API public key is used to authenticate this plugin with the ChargeAfter API.",
                ["Plugins.Payments.ChargeAfter.Fields.ProductionPublicKey.Required"] = "Production Public Key is required.",
                ["Plugins.Payments.ChargeAfter.Fields.ProductionPrivateKey"] = "Production Private Key",
                ["Plugins.Payments.ChargeAfter.Fields.ProductionPrivateKey.Hint"] = "The REST API private key is used to authenticate this plugin with the ChargeAfter API.",
                ["Plugins.Payments.ChargeAfter.Fields.ProductionPrivateKey.Required"] = "Production Private Key is required.",

                ["Plugins.Payments.ChargeAfter.Fields.UseProduction"] = "Use Production",
                ["Plugins.Payments.ChargeAfter.Fields.UseProduction.Hint"] = "The production mode uses the production ChargeAfter keys and switches the script to the production environment. Don`t activate for testing!",

                ["Plugins.Payments.ChargeAfter.Fields.SandboxPublicKey"] = "Sandbox Public Key",
                ["Plugins.Payments.ChargeAfter.Fields.SandboxPublicKey.Hint"] = "The REST API public key is used while testing to authenticate this plugin with the ChargeAfter API.",
                ["Plugins.Payments.ChargeAfter.Fields.SandboxPublicKey.Required"] = "Sandbox Public Key is required.",
                ["Plugins.Payments.ChargeAfter.Fields.SandboxPrivateKey"] = "Sandbox Private Key",
                ["Plugins.Payments.ChargeAfter.Fields.SandboxPrivateKey.Hint"] = "The REST API private key is used while testing to authenticate this plugin with the ChargeAfter API.",
                ["Plugins.Payments.ChargeAfter.Fields.SandboxPrivateKey.Required"] = "Sandbox Private Key is required.",

                ["Plugins.Payments.ChargeAfter.Fields.AdditionalFee"] = "Additional fee",
                ["Plugins.Payments.ChargeAfter.Fields.AdditionalFee.Hint"] = "Enter additional fee to charge your customers.",
                ["Plugins.Payments.ChargeAfter.Fields.AdditionalFeePercentage"] = "Additional fee. Use percentage",
                ["Plugins.Payments.ChargeAfter.Fields.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",

                ["Plugins.Payments.ChargeAfter.Fields.TypeTransaction"] = "Transaction Type",
                ["Plugins.Payments.ChargeAfter.Fields.TypeTransaction.Hint"] = "Set to capture enabling auto capture at the end of checkout experience",

                ["Plugins.Payments.ChargeAfter.Fields.TypeCheckoutBrand"] = "Checkout Brand Type",
                ["Plugins.Payments.ChargeAfter.Fields.TypeCheckoutBrand.Hint"] = "Type attribute for checkout branding according the space available to you",

                ["Plugins.Payments.ChargeAfter.Fields.EnableLineOfCreditPromo"] = "Enable promotional widget",
                ["Plugins.Payments.ChargeAfter.Fields.EnableLineOfCreditPromo.Hint"] = "This widget will display the offer with the lowest interest rate that a consumer can get.",
                ["Plugins.Payments.ChargeAfter.Fields.TypeLineOfCreditPromo"] = "Promotional widget Type",
                ["Plugins.Payments.ChargeAfter.Fields.TypeLineOfCreditPromo.Hint"] = "You can choose one of widget types to display the financial offer available.",
                ["Plugins.Payments.ChargeAfter.Fields.FinancingPageUrlLineOfCreditPromo"] = "Financing page Url",
                ["Plugins.Payments.ChargeAfter.Fields.FinancingPageUrlLineOfCreditPromo.Hint"] = "The financing page URL is used to notify the user of more detailed funding information. Link must be absolute.",
                ["Plugins.Payments.ChargeAfter.Fields.FinancingPageUrlLineOfCreditPromo.Required"] = "The financing page URL is required.",

                ["Plugins.Payments.ChargeAfter.Fields.EnableSimplePromoBeforeContent"] = "Enable promotional widget before content",
                ["Plugins.Payments.ChargeAfter.Fields.EnableSimplePromoBeforeContent.Hint"] = "The widget will be displayed on all pages before the main content.",
                ["Plugins.Payments.ChargeAfter.Fields.WidgetTypeSimplePromoBeforeContent"] = "Widget type before content",
                ["Plugins.Payments.ChargeAfter.Fields.WidgetTypeSimplePromoBeforeContent.Hint"] = "The type of widget to be displayed on all pages before the main content.",

                ["Plugins.Payments.ChargeAfter.Fields.EnableSimplePromoAfterContent"] = "Enable promotional widget after content",
                ["Plugins.Payments.ChargeAfter.Fields.EnableSimplePromoAfterContent.Hint"] = "The widget will be displayed on all pages after the main content.",
                ["Plugins.Payments.ChargeAfter.Fields.WidgetTypeSimplePromoAfterContent"] = "Widget type after content",
                ["Plugins.Payments.ChargeAfter.Fields.WidgetTypeSimplePromoAfterContent.Hint"] = "The type of widget to be displayed on all pages after the main content.",
                
                ["Plugins.Payments.ChargeAfter.Fields.EnableSimplePromoProductBeforeContent"] = "Enable promotional widget before product",
                ["Plugins.Payments.ChargeAfter.Fields.EnableSimplePromoProductBeforeContent.Hint"] = "The widget will be displayed on product page before the main content.",
                ["Plugins.Payments.ChargeAfter.Fields.WidgetTypeSimplePromoProductBeforeContent"] = "Widget type before product",
                ["Plugins.Payments.ChargeAfter.Fields.WidgetTypeSimplePromoProductBeforeContent.Hint"] = "The type of widget to be displayed on product page before the main content.",
                
                ["Plugins.Payments.ChargeAfter.Fields.EnableSimplePromoProductAfterTitle"] = "Enable promotional widget after product title",
                ["Plugins.Payments.ChargeAfter.Fields.EnableSimplePromoProductAfterTitle.Hint"] = "The widget will be displayed on product page after the product title and short description.",
                ["Plugins.Payments.ChargeAfter.Fields.WidgetTypeSimplePromoProductAfterTitle"] = "Widget type after product title",
                ["Plugins.Payments.ChargeAfter.Fields.WidgetTypeSimplePromoProductAfterTitle.Hint"] = "The type of widget to be displayed on product page after the product title and short description.",
                
                ["Plugins.Payments.ChargeAfter.Fields.EnableSimplePromoProductAfterDesc"] = "Enable promotional widget after product description",
                ["Plugins.Payments.ChargeAfter.Fields.EnableSimplePromoProductAfterDesc.Hint"] = "The widget will be displayed on product page after product description.",
                ["Plugins.Payments.ChargeAfter.Fields.WidgetTypeSimplePromoProductAfterDesc"] = "Widget type after content",
                ["Plugins.Payments.ChargeAfter.Fields.WidgetTypeSimplePromoProductAfterDesc.Hint"] = "The type of widget to be displayed on product page after product description.",

                ["Plugins.Payments.ChargeAfter.Fields.NonLeasable"] = "Non Leasable",
                ["Plugins.Payments.ChargeAfter.Fields.NonLeasable.Hint"] = "Specifying whether a product is non-leasable is required when offering a lease-to-own financing option.",
                ["Plugins.Payments.ChargeAfter.Fields.Warranty"] = "Warranty",
                ["Plugins.Payments.ChargeAfter.Fields.Warranty.Hint"] = "Specifying whether a product has a warranty.",

                ["Plugins.Payments.ChargeAfter.Fields.ProductAttr.SaveBeforeEdit"] = "You need to save the product before you can edit consumer financing attributes for this product.",
                
                ["Plugins.Payments.ChargeAfter.Customer.Checkout.Token"] = "ChargeAfter Confirmation Token",
            });

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(Defaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(Defaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }
            await _settingService.DeleteSettingAsync<ChargeAfterPaymentSettings>();

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.ChargeAfter");

            await base.UninstallAsync();
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                /** Public zones **/
                PublicWidgetZones.Footer,
                PublicWidgetZones.ContentBefore,
                PublicWidgetZones.ContentAfter,
                PublicWidgetZones.ProductDetailsAddInfo,
                PublicWidgetZones.ProductDetailsOverviewTop,
                PublicWidgetZones.ProductDetailsEssentialTop,
                PublicWidgetZones.ProductDetailsEssentialBottom,

                /** Admin zones **/
                AdminWidgetZones.OrderDetailsButtons,
                AdminWidgetZones.ProductDetailsBlock
            });
        }

        public Type GetPublicViewComponent()
        {
            return typeof(ChargeAfterPaymentInfoViewComponent);
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == null)
                throw new ArgumentNullException(nameof(widgetZone));

            if (widgetZone.Equals(AdminWidgetZones.OrderDetailsButtons))
            {
                return typeof(ChargeAfterAdminOrderViewComponent);
            }

            if (widgetZone.Equals(AdminWidgetZones.ProductDetailsBlock))
            {
                return typeof(ChargeAfterAdminProductViewComponent);
            }

            if (widgetZone.Equals(PublicWidgetZones.ProductDetailsAddInfo))
                return typeof(ChargeAfterPromoLineOfCreditViewComponent);

            if (widgetZone.Equals(PublicWidgetZones.ContentBefore) ||
                widgetZone.Equals(PublicWidgetZones.ContentAfter))
            {
                return typeof(ChargeAfterPromoSimpleGlobalViewComponent);
            }

            if (widgetZone.Equals(PublicWidgetZones.ProductDetailsEssentialTop) ||
                widgetZone.Equals(PublicWidgetZones.ProductDetailsOverviewTop) ||
                widgetZone.Equals(PublicWidgetZones.ProductDetailsEssentialBottom))
            {
                return typeof(ChargeAfterPromoSimpleProductViewComponent);
            }

            return typeof(ChargeAfterPromoScriptViewComponent);
        }

        #endregion

        #region Properties

        public bool SupportCapture => true;

        public bool SupportRefund => true;

        public bool SupportPartiallyRefund => true;

        public bool SupportVoid => true;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        public bool SkipPaymentInfo => false;

        public bool HideInWidgetList => false;

        #endregion
    }
}