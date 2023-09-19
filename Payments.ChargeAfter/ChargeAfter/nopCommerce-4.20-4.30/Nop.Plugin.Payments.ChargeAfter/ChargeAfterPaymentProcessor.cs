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

namespace Nop.Plugin.Payments.ChargeAfter 
{
    public class ChargeAfterPaymentProcessor : BasePlugin, IPaymentMethod, IWidgetPlugin
    {
        #region Fields

        private readonly ChargeAfterPaymentSettings _chargeAfterPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
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
            IPaymentService paymentService,
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
            _paymentService = paymentService;
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

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
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

            return result;
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
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
                        _orderTaxService.UpdateTaxFree(order.Id);
                    }
                }

                // Auto capture
                if (_chargeAfterPaymentSettings.UseAutoCapture)
                {
                    _orderSaleService.Capture(order.Id);
                }
            }
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            var customer = _workContext.CurrentCustomer;
            
            bool allowCountry = false; 
            bool allowSettings = false;
          
            if(customer != null && customer.BillingAddressId.HasValue)
            {
                var billingAddress = _addressService.GetAddressById((int)customer.BillingAddressId);
                
                if (billingAddress != null && billingAddress.CountryId.HasValue)
                {
                    var country = _countryService.GetCountryById((int)billingAddress.CountryId);

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

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _paymentService.CalculateAdditionalFee(cart,
                _chargeAfterPaymentSettings.AdditionalFee, _chargeAfterPaymentSettings.AdditionalFeePercentage);
        }

        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            if(string.IsNullOrEmpty(capturePaymentRequest.Order.AuthorizationTransactionId))
                return new CapturePaymentResult { Errors = new[] { "Failed to get the ChargeAfter Charge" } };

            var chargeId = capturePaymentRequest.Order.AuthorizationTransactionId;
            
            var (response, error) = _serviceManager.GetChargeById(_chargeAfterPaymentSettings, chargeId);
            if (!string.IsNullOrEmpty(error))
                return new CapturePaymentResult { Errors = new[] { "Failed to get the ChargeAfter Charge" } };

            if(response.State != ChargeState.AUTHORIZED)
                return new CapturePaymentResult { Errors = new[] { "Failed. Charge not authorized" } };

            var (capture_response, capture_error) = _serviceManager.Capture(_chargeAfterPaymentSettings, chargeId, response.TotalAmount);
            if (!string.IsNullOrEmpty(capture_error))
                return new CapturePaymentResult { Errors = new[] { "Charge settle error" } };

            return new CapturePaymentResult 
            {
                CaptureTransactionId = response.ChargeId,
                CaptureTransactionResult = ChargeState.SETTLED,
                NewPaymentStatus = PaymentStatus.Paid
            };
        }

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            if (string.IsNullOrEmpty(refundPaymentRequest.Order.CaptureTransactionId))
                return new RefundPaymentResult { Errors = new[] { "Charge not settled for refund" } };

            var chargeId = refundPaymentRequest.Order.CaptureTransactionId;
            var refundAmount = refundPaymentRequest.AmountToRefund;

            var (response, error) = _serviceManager.GetChargeById(_chargeAfterPaymentSettings, chargeId);
            if (!string.IsNullOrEmpty(error))
                return new RefundPaymentResult { Errors = new[] { "Failed to get the ChargeAfter Charge" } };

            if (response.State == ChargeState.REFUNDED)
                return new RefundPaymentResult { Errors = new[] { "Refund error occured. Charge fully refunded" } };

            var availableToRefund = response.SettledAmount - response.RefundedAmount;
            if (refundAmount > availableToRefund)
            {
                refundAmount = availableToRefund;
            }

            if (refundAmount > 0) {
                var (_, refund_error) = _serviceManager.Refund(_chargeAfterPaymentSettings, chargeId, refundAmount);
                if (!string.IsNullOrEmpty(refund_error)) { 
                    return new RefundPaymentResult { Errors = new[] { "Refund error occured" } };
                }
            }

            return new RefundPaymentResult
            {
                NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded
            };
        }

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            if (string.IsNullOrEmpty(voidPaymentRequest.Order.AuthorizationTransactionId))
                return new VoidPaymentResult { Errors = new[] { "Failed to get the ChargeAfter Charge" } };

            var chargeId = voidPaymentRequest.Order.AuthorizationTransactionId;
            var result = new VoidPaymentResult 
            { 
                NewPaymentStatus = PaymentStatus.Voided 
            };

            var (response, error) = _serviceManager.GetChargeById(_chargeAfterPaymentSettings, chargeId);
            if (!string.IsNullOrEmpty(error))
                return new VoidPaymentResult { Errors = new[] { "Failed to get the ChargeAfter Charge" } };

            if(response.State == ChargeState.AUTHORIZED)
            {
                var (void_response, void_error) = _serviceManager.Void(_chargeAfterPaymentSettings, chargeId);
                if (!string.IsNullOrEmpty(void_error))
                    return new VoidPaymentResult { Errors = new[] { "Charge void error" } };

                return result;
            } 
            else if(response.State == ChargeState.SETTLED || response.State == ChargeState.PARTIALLY_REFUNDED)
            {
                var amount = response.TotalAmount - response.RefundedAmount;
                var (refund_response, refund_error) = _serviceManager.Refund(_chargeAfterPaymentSettings, chargeId, amount);
                if (!string.IsNullOrEmpty(refund_error))
                    return new VoidPaymentResult { Errors = new[] { "Charge void error" } };

                return result;
            }

            return new VoidPaymentResult { Errors = new[] { "Void error occured" } };
        }

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //it's not a redirection payment method. So we always return false
            return false;
        }

        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            return new List<string>();
        }

        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            return new ProcessPaymentRequest();
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}{Defaults.ConfigurationRouteName}";
        }

        public string GetPublicViewComponentName()
        {
            return Defaults.PAYMENT_INFO_VIEW_COMPONENT_NAME;
        }

        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new ChargeAfterPaymentSettings {
                UseProduction = false
            });

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(Defaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(Defaults.SystemName);
                _settingService.SaveSetting(_widgetSettings);
            }

            //locales
            _localizationService.AddPluginLocaleResource(new Dictionary<string, string>
            {
                ["Plugins.Payment.ChargeAfter.PaymentMethodDescription"] = ".",

                ["Plugins.Payment.ChargeAfter.PaymentMethod.Tip"] = "You will be redirected to complete the order.",

                ["Plugins.Payments.ChargeAfter.Header.Payment"] = "Payment",
                ["Plugins.Payments.ChargeAfter.Header.Checkout"] = "Checkout",
                ["Plugins.Payments.ChargeAfter.Header.PromoLineOfCredit"] = "Promo: Line of Credit",
                ["Plugins.Payments.ChargeAfter.Header.PromoSimple"] = "Promo: Simple Widgets",
                
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
                ["Plugins.Payments.ChargeAfter.Fields.FinancingPageUrlLineOfCreditPromo.Hint"] = "The financing page URL is used to notify the user of more detailed funding information.  Link must be absolute.",
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
                ["Plugins.Payments.ChargeAfter.Fields.NonLeasable.SaveBeforeEdit"] = "You need to save the product before you can edit non-leasable attribute for this product page.",
                ["Plugins.Payments.ChargeAfter.Fields.NonLeasable.Hint"] = "Specifying whether a product is non-leasable is required when offering a lease-to-own financing option",


                ["Plugins.Payments.ChargeAfter.Customer.Checkout.Token"] = "ChargeAfter Confirmation Token",
            });

            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(Defaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(Defaults.SystemName);
                _settingService.SaveSetting(_widgetSettings);
            }
            _settingService.DeleteSetting<ChargeAfterPaymentSettings>();

            //locales
            _localizationService.DeletePluginLocaleResources("Plugins.Payments.ChargeAfter");

            base.Uninstall();
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string>
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
            };
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (widgetZone == null)
                throw new ArgumentNullException(nameof(widgetZone));

            if(widgetZone.Equals(AdminWidgetZones.OrderDetailsButtons))
            {
                return Defaults.ADMIN_ORDER_VIEW_COMPONENT_NAME;
            }

            if(widgetZone.Equals(AdminWidgetZones.ProductDetailsBlock))
            {
                return Defaults.ADMIN_PRODUCT_VIEW_COMPONENT_NAME;
            }

            if (widgetZone.Equals(PublicWidgetZones.ProductDetailsAddInfo))
                return Defaults.PROMO_LINE_OF_CREDIT_VIEW_COMPONENT_NAME;

            if (widgetZone.Equals(PublicWidgetZones.ContentBefore) || 
                widgetZone.Equals(PublicWidgetZones.ContentAfter))
            {
                return Defaults.PROMO_SIMPLE_GLOBAL_VIEW_COMPONENT_NAME;
            }

            if (widgetZone.Equals(PublicWidgetZones.ProductDetailsEssentialTop) ||
                widgetZone.Equals(PublicWidgetZones.ProductDetailsOverviewTop) ||
                widgetZone.Equals(PublicWidgetZones.ProductDetailsEssentialBottom))
            {
                return Defaults.PROMO_SIMPLE_PRODUCT_VIEW_COMPONENT_NAME;
            }

            return Defaults.PROMO_SCRIPT_VIEW_COMPONENT_NAME;
        }

        #endregion

        #region Properties

        public bool SupportCapture => true;

        public bool SupportPartiallyRefund => true;

        public bool SupportRefund => true;

        public bool SupportVoid => true;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        public bool SkipPaymentInfo => false;

        public string PaymentMethodDescription => _localizationService.GetResource("Plugins.Payment.ChargeAfter.PaymentMethodDescription");

        public bool HideInWidgetList => false;

        #endregion
    }
}