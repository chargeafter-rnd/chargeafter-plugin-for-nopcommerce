using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Payments.ChargeAfter.Domain;
using Nop.Plugin.Payments.ChargeAfter.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Tax;
using System;
using System.Collections.Generic;
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
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IPaymentService _paymentService;
        private readonly ITaxService _taxService;
        private readonly IGenericAttributeService _genericAttributeService;

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
            ICustomerService customerService,
            IDiscountService discountService,
            IPaymentService paymentService,
            ITaxService taxService,
            ChargeAfterPaymentSettings settings,
            TaxSettings taxSettings,
            IGenericAttributeService genericAttributeService
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
            _customerService = customerService;
            _discountService = discountService;
            _paymentService = paymentService;
            _taxService = taxService;
            _settings = settings;
            _taxSettings = taxSettings;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Methods

        public CheckoutModel GetCheckoutData()
        {
            if (!_paymentPluginManager.IsPluginActive(Defaults.SystemName, _workContext.CurrentCustomer, _storeContext.CurrentStore.Id))
                throw new NopException("Unauthorized action");

            var customer = _workContext.CurrentCustomer;
            var paymentMethodSystemName = _genericAttributeService.GetAttribute<string>(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, _storeContext.CurrentStore.Id);

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

            var model = new CheckoutModel
            {
                ChargeAfterCheckoutUI = checkoutUiData,
                CaPublicKey = caPublicKey,
                CaHost = caHost,
                AddressMismatch = CheckAddressMismatch(checkoutUiData),
            };
            
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
                var cartItemSubTotalWithDiscountBase = _taxService.GetProductPrice(product, _shoppingCartService.GetSubTotal(sci, true, out var shoppingCartItemDiscountBase, out _, out var maximumDiscountQty), out _);
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
                    Leasable = (bool)product.IsRental
                };

                model.Items.Add(itemModel);
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
            var shoppingCartShippingBase = _orderTotalCalculationService.GetShoppingCartShippingTotal(shoppingCartItems, true);
            if (shoppingCartShippingBase.HasValue)
            {
                model.TotalShippingAmount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartShippingBase.Value, _workContext.WorkingCurrency);
            }

            var shoppingCartTaxBase = _orderTotalCalculationService.GetTaxTotal(shoppingCartItems, out var taxRates);
            model.TotalTaxAmount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTaxBase, _workContext.WorkingCurrency);

            // discount amount
            if (orderTotalDiscountAmountBase > decimal.Zero)
            {
                var orderTotalDiscountAmount = _currencyService.ConvertFromPrimaryStoreCurrency(orderTotalDiscountAmountBase, _workContext.WorkingCurrency);
            }

            if(appliedDiscounts != null) 
            {
                var orderTotalAmount = subTotalWithoutDiscountBase + model.TotalShippingAmount + model.TotalTaxAmount;
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
                    // 6725 + 554.81 = 7 279,81 | 1455  
                }
            }

            return model;
        }

        #endregion

        #region Utilities

        private static bool CheckAddressMismatch(ChargeAfterCheckoutUI checkoutUiData)
        {
            if (!checkoutUiData.BillingAddressLine1.Equals(checkoutUiData.ShippingAddressLine1) ||
                (!string.IsNullOrEmpty(checkoutUiData.BillingAddressLine2) &&
                 !string.IsNullOrEmpty(checkoutUiData.ShippingAddressLine2) &&
                 !checkoutUiData.BillingAddressLine2.Equals(checkoutUiData.ShippingAddressLine2)
                ) ||
                !checkoutUiData.BillingAddressCity.Equals(checkoutUiData.ShippingAddressCity) ||
                !checkoutUiData.BillingAddressZipCode.Equals(checkoutUiData.ShippingAddressZipCode) ||
                !checkoutUiData.BillingAddressState.Equals(checkoutUiData.ShippingAddressState)
            ) {
                return true;
            }

            return false;
        }

        #endregion
    }
}
