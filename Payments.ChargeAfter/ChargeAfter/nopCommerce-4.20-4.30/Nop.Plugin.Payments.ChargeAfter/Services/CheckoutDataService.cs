using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Payments.ChargeAfter.Domain;
using Nop.Plugin.Payments.ChargeAfter.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Tax;
using System.Linq;

namespace Nop.Plugin.Payments.ChargeAfter.Services
{
    public interface ICheckoutDataService
    {
        public CheckoutModel GetCheckoutData();
    }
}

namespace Nop.Plugin.Payments.ChargeAfter.Services
{
    public class CheckoutDataService : ICheckoutDataService
    {
        #region Fields

        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly ChargeAfterPaymentSettings _settings;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IAddressService _addressService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        private readonly ICurrencyService _currencyService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly TaxSettings _taxSettings;
        private readonly IDiscountService _discountService;
        private readonly ITaxService _taxService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly INonLeasableService _nonLeasableService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICheckoutAttributeService _checkoutAttributeService;

        #endregion

        #region Ctor

        public CheckoutDataService(
            IPaymentPluginManager paymentPluginManager,
            IStoreContext storeContext,
            IWorkContext workContext,
            IAddressService addressService,
            IStateProvinceService stateProvinceService,
            IShoppingCartService shoppingCartService,
            IProductService productService,
            ILocalizationService localizationService,
            ICurrencyService currencyService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IDiscountService discountService,
            ITaxService taxService,
            ChargeAfterPaymentSettings settings,
            TaxSettings taxSettings,
            IGenericAttributeService genericAttributeService,
            INonLeasableService nonLeasableService,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICheckoutAttributeService checkoutAttributeService
        )
        {
            _paymentPluginManager = paymentPluginManager;
            _storeContext = storeContext;
            _workContext = workContext;
            _addressService = addressService;
            _stateProvinceService = stateProvinceService;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
            _localizationService = localizationService;
            _currencyService = currencyService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _discountService = discountService;
            _taxService = taxService;
            _settings = settings;
            _taxSettings = taxSettings;
            _genericAttributeService = genericAttributeService;
            _nonLeasableService = nonLeasableService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _checkoutAttributeService = checkoutAttributeService;
        }

        #endregion

        #region Methods

        public CheckoutModel GetCheckoutData()
        {
            var customer = _workContext.CurrentCustomer;

            if (!_paymentPluginManager.IsPluginActive(Defaults.SystemName, customer, _storeContext.CurrentStore.Id))
                throw new NopException("Unauthorized action");

            var paymentMethodSystemName = _genericAttributeService.GetAttribute<string>(
                customer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute, 
                _storeContext.CurrentStore.Id
            );

            if (!paymentMethodSystemName.Equals(Defaults.SystemName))
                throw new NopException("Unauthorized action");
            
            var caPublicKey = ChargeAfterHelper.GetPublicKeyFromSettings(_settings);
            if (string.IsNullOrEmpty(caPublicKey))
                throw new NopException("Incorrect credentials");

            if (customer.BillingAddressId == null)
                throw new NopException("Invalid customer billing information");

            var caHost = ChargeAfterHelper.GetCaHostByUseProduction(_settings.UseProduction);
            var billingAddress = _addressService.GetAddressById((int)customer.BillingAddressId);

            var shippingAddress = billingAddress;
            if (customer.ShippingAddressId != null)
                shippingAddress = _addressService.GetAddressById((int)customer.ShippingAddressId);

            if (billingAddress.StateProvinceId == null || shippingAddress.StateProvinceId == null)
                throw new NopException("Invalid customer billing or shipping addresses");

            var billingAddressState = _stateProvinceService.GetStateProvinceById((int)billingAddress.StateProvinceId);
            var shippingAddressState = _stateProvinceService.GetStateProvinceById((int)shippingAddress.StateProvinceId);

            // Checkout UI Data
            var checkoutUiData = new ChargeAfterCheckoutUI
            {
                FirstName = billingAddress.FirstName,
                LastName = billingAddress.LastName,
                Email = billingAddress.Email,
                Phone = billingAddress.PhoneNumber,

                BillingAddressLine1 = billingAddress.Address1,
                BillingAddressLine2 = billingAddress.Address2,
                BillingAddressCity = billingAddress.City,
                BillingAddressZipCode = billingAddress.ZipPostalCode,
                BillingAddressState = billingAddressState.Abbreviation,

                ShippingAddressLine1 = shippingAddress.Address1,
                ShippingAddressLine2 = shippingAddress.Address2,
                ShippingAddressCity = shippingAddress.City,
                ShippingAddressZipCode = shippingAddress.ZipPostalCode,
                ShippingAddressState = shippingAddressState.Abbreviation,
            };

            var model = new CheckoutModel { ChargeAfterCheckoutUI = checkoutUiData };
            
            // items
            var shoppingCartItems = _shoppingCartService.GetShoppingCart(customer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);
            if (!shoppingCartItems.Any())
            {
                throw new NopException("Cart is empty. Please try again");
            }

            foreach (var sci in shoppingCartItems)
            {
                var product = _productService.GetProductById(sci.ProductId);

                // sub total
                var cartItemSubTotalWithDiscountBase = _taxService.GetProductPrice(
                    product, 
                    _shoppingCartService.GetSubTotal(sci, true, out var shoppingCartItemDiscountBase, out _, out var maximumDiscountQty),
                    includingTax: false,
                    customer,
                    out _
                );

                var cartItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(cartItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);

                // cart item
                var itemModel = new CheckoutModel.CheckoutItemModel
                {
                    Sku = _productService.FormatSku(product, sci.AttributesXml),
                    Id = sci.ProductId,
                    ProductId = sci.ProductId,
                    Name = _localizationService.GetLocalized(product, x => x.Name),
                    Quantity = sci.Quantity,
                    UnitPrice = (float)cartItemSubTotalWithDiscount/sci.Quantity,
                    Leasable = _nonLeasableService.GetAttributeValue(product) == false
                };

                model.Items.Add(itemModel);
            }

            // checkout items
            var checkoutAttributesXml = _genericAttributeService.GetAttribute<string>(
                customer,
                NopCustomerDefaults.CheckoutAttributes,
                _storeContext.CurrentStore.Id
            );

            checkoutAttributesXml = _checkoutAttributeParser.EnsureOnlyActiveAttributes(checkoutAttributesXml, shoppingCartItems);

            var attributes = _checkoutAttributeParser.ParseCheckoutAttributes(checkoutAttributesXml);
            for (var i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];
                var valuesStr = _checkoutAttributeParser.ParseValues(checkoutAttributesXml, attribute.Id);

                for (var j = 0; j < valuesStr.Count; j++)
                {
                    var valueStr = valuesStr[j];

                    if (int.TryParse(valueStr, out var attributeValueId))
                    {
                        var attributeValue = _checkoutAttributeService.GetCheckoutAttributeValueById(attributeValueId);

                        if (attributeValue != null)
                        {
                            var priceAdjustmentBase = _taxService.GetCheckoutAttributePrice(attribute, attributeValue, customer);
                            var priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);

                            if (priceAdjustmentBase > 0)
                            {
                                // checkout item
                                var itemModel = new CheckoutModel.CheckoutItemModel
                                {
                                    Sku = string.Format("checkout_attr_{0}", attribute.Id),
                                    Name = _localizationService.GetLocalized(attribute, a => a.Name),
                                    Quantity = 1,
                                    UnitPrice = (float)priceAdjustment,
                                    Leasable = true
                                };

                                model.Items.Add(itemModel);
                            }
                        }
                    }
                }
            }

            // sub total
            var subTotalIncludingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
            _orderTotalCalculationService.GetShoppingCartSubTotal(shoppingCartItems, false, out var orderSubTotalDiscountAmountBase, out var _, out var subTotalWithoutDiscountBase, out var _);

            // total
            var shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(shoppingCartItems, out var orderTotalDiscountAmountBase, out var appliedDiscounts, out var appliedGiftCards, out var redeemedRewardPoints, out var redeemedRewardPointsAmount);
            if (!shoppingCartTotalBase.HasValue)
            {
                throw new NopException("Failed to get total amount");
            }

            model.TotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTotalBase.Value, _workContext.WorkingCurrency);
            model.TotalShippingAmount = 0;

            // shipping total
            decimal? shippingExclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(shoppingCartItems, false);
            decimal? shippingInclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(shoppingCartItems, true);
            
            var shippingTax = shippingInclTax.Value - shippingExclTax.Value;

            if (shippingExclTax.HasValue)
            {
                model.TotalShippingAmount = _currencyService.ConvertFromPrimaryStoreCurrency(shippingExclTax.Value, _workContext.WorkingCurrency);
            }

            //var shoppingCartShippingBase = _orderTotalCalculationService.GetShoppingCartShippingTotal(shoppingCartItems, false);
            //if (shoppingCartShippingBase.HasValue)
            //{
            //    model.TotalShippingAmount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartShippingBase.Value, _workContext.WorkingCurrency);
            //}

            var shoppingCartTaxBase = _orderTotalCalculationService.GetTaxTotal(shoppingCartItems, out var taxRates);
            model.TotalTaxAmount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTaxBase, _workContext.WorkingCurrency);

            // discount amount
            if (orderTotalDiscountAmountBase > decimal.Zero)
            {
                var orderTotalDiscountAmount = _currencyService.ConvertFromPrimaryStoreCurrency(orderTotalDiscountAmountBase, _workContext.WorkingCurrency);
            }

            if(appliedDiscounts != null) 
            {
                var orderTotalAmount = subTotalWithoutDiscountBase + model.TotalShippingAmount + shippingTax + model.TotalTaxAmount;
                foreach (var discount in appliedDiscounts)
                {
                    var currentDiscountValue = _discountService.GetDiscountAmount(discount, orderTotalAmount);

                    if (currentDiscountValue > decimal.Zero)
                    {
                        var discountItem = new CheckoutModel.CheckoutDiscountItemModel
                        {
                            Id = discount.Id,
                            DiscountId = discount.Id,
                            Name = discount.Name,
                            Code = discount.CouponCode,
                            Amount = currentDiscountValue
                        };

                        model.DiscountItems.Add(discountItem);
                    }  
                }
            }

            return model;
        }

        #endregion
    }
}
