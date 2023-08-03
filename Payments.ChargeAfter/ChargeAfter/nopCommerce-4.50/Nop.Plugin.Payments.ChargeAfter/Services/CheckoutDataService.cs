using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
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
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.ChargeAfter.Services
{
    public interface ICheckoutDataService
    {
        public Task<CheckoutModel> GetCheckoutDataAsync();
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
            ITaxService taxService,
            ChargeAfterPaymentSettings settings,
            IGenericAttributeService genericAttributeService,
            IDiscountService discountService,
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
            _taxService = taxService;
            _settings = settings;
            _genericAttributeService = genericAttributeService;
            _discountService = discountService;
            _nonLeasableService = nonLeasableService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _checkoutAttributeService = checkoutAttributeService;
        }

        #endregion

        #region Methods

        public async Task<CheckoutModel> GetCheckoutDataAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var currentStore = await _storeContext.GetCurrentStoreAsync();

            if (!await _paymentPluginManager.IsPluginActiveAsync(Defaults.SystemName, customer, currentStore.Id))
                throw new NopException("Unauthorized action");
            
            var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(
                customer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute, 
                currentStore.Id
            );

            if (!paymentMethodSystemName.Equals(Defaults.SystemName))
                throw new NopException("Unauthorized action");
            
            var caPublicKey = ChargeAfterHelper.GetPublicKeyFromSettings(_settings);
            if (string.IsNullOrEmpty(caPublicKey))
                throw new NopException("Incorrect credentials");

            var caHost = ChargeAfterHelper.GetCaHostByUseProduction(_settings.UseProduction);
            var checkoutUiData = await GetCheckoutUiDataAsync(customer);

            var model = new CheckoutModel
            {
                ChargeAfterCheckoutUI = checkoutUiData,
                CaPublicKey = caPublicKey,
                CaHost = caHost
            };
            
            // items
            var shoppingCartItems = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, currentStore.Id);
            if (!shoppingCartItems.Any())
                throw new NopException("Cart is empty. Please try again");
            
            var workingCurrency = await _workContext.GetWorkingCurrencyAsync();

            foreach (var sci in shoppingCartItems)
            {
                var product = await _productService.GetProductByIdAsync(sci.ProductId);

                // sub total
                var (subTotal, shoppingCartItemDiscountBase, _, _) = await _shoppingCartService.GetSubTotalAsync(sci, true);

                var (cartItemSubTotalWithDiscountBase, _) = await _taxService.GetProductPriceAsync(
                    product,
                    subTotal,
                    includingTax: false,
                    customer
                );

                var cartItemSubTotalWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                    cartItemSubTotalWithDiscountBase,
                    workingCurrency
                );

                // cart item
                var itemModel = new CheckoutModel.CheckoutItemModel
                {
                    Sku = await _productService.FormatSkuAsync(product, sci.AttributesXml),
                    Id = sci.ProductId,
                    ProductId = sci.ProductId,
                    Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                    Quantity = sci.Quantity,
                    UnitPrice = (float)cartItemSubTotalWithDiscount/sci.Quantity,
                    Leasable = await _nonLeasableService.GetAttributeValueAsync(product) == false
                };

                model.Items.Add(itemModel);
            }

            // checkout items
            var checkoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(
                customer,
                NopCustomerDefaults.CheckoutAttributes,
                currentStore.Id
            );
            checkoutAttributesXml = await _checkoutAttributeParser.EnsureOnlyActiveAttributesAsync(checkoutAttributesXml, shoppingCartItems);

            var attributes = await _checkoutAttributeParser.ParseCheckoutAttributesAsync(checkoutAttributesXml);
            for (var i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];
                var valuesStr = _checkoutAttributeParser.ParseValues(checkoutAttributesXml, attribute.Id);
                for (var j = 0; j < valuesStr.Count; j++)
                {
                    var valueStr = valuesStr[j];
                    if (int.TryParse(valueStr, out var attributeValueId))
                    {
                        var attributeValue = await _checkoutAttributeService.GetCheckoutAttributeValueByIdAsync(attributeValueId);
                        if (attributeValue != null)
                        {
                            var priceAdjustmentBase = (await _taxService.GetCheckoutAttributePriceAsync(attribute, attributeValue, customer)).price;
                            var priceAdjustment = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(priceAdjustmentBase, await _workContext.GetWorkingCurrencyAsync());

                            if (priceAdjustmentBase > 0)
                            {
                                // checkout item
                                var itemModel = new CheckoutModel.CheckoutItemModel
                                {
                                    Sku = string.Format("checkout_attr_{0}", attribute.Id),
                                    Name = await _localizationService.GetLocalizedAsync(attribute, a => a.Name),
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
            var (_, _, subTotalWithoutDiscountBase, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(
                shoppingCartItems, 
                includingTax: false
            );

            // total
            var (shoppingCartTotalBase, 
                orderTotalDiscountAmountBase, 
                appliedDiscounts, 
                appliedGiftCards, 
                redeemedRewardPoints, 
                redeemedRewardPointsAmount) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(shoppingCartItems);
            
            if (shoppingCartTotalBase == null)
                throw new NopException("Failed to get total amount");
            
            model.TotalAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                (decimal)shoppingCartTotalBase, 
                workingCurrency
            );
            
            // shipping total
            var (shippingExclTax, _, _) = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(shoppingCartItems, includingTax: false);
            var (shippingInclTax, _, _) = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(shoppingCartItems, includingTax: true);

            decimal shippingTax = 0;
            if (shippingInclTax.HasValue && shippingExclTax.HasValue)
            {
                shippingTax = shippingInclTax.Value - shippingExclTax.Value;
            }

            model.TotalShippingAmount = 0;
            if (shippingExclTax.HasValue)
            {
                model.TotalShippingAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shippingExclTax.Value, workingCurrency);
            }

            var (shoppingCartTaxBase, taxRates) = await _orderTotalCalculationService.GetTaxTotalAsync(shoppingCartItems);
            model.TotalTaxAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTaxBase, workingCurrency);

            // discount amount
            if (appliedDiscounts != null) 
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

        public async Task<ChargeAfterCheckoutUI> GetCheckoutUiDataAsync(Customer customer)
        {
            if (customer.BillingAddressId == null)
                throw new NopException("Invalid customer billing information");

            var caHost = ChargeAfterHelper.GetCaHostByUseProduction(_settings.UseProduction);
            var billingAddress = await _addressService.GetAddressByIdAsync((int)customer.BillingAddressId);

            var shippingAddress = billingAddress;
            if (customer.ShippingAddressId != null)
                shippingAddress = await _addressService.GetAddressByIdAsync((int)customer.ShippingAddressId);

            if (billingAddress.StateProvinceId == null || shippingAddress.StateProvinceId == null)
                throw new NopException("Invalid customer billing or shipping addresses");

            var billingAddressState = await _stateProvinceService.GetStateProvinceByIdAsync((int)billingAddress.StateProvinceId);
            var shippingAddressState = await _stateProvinceService.GetStateProvinceByIdAsync((int)shippingAddress.StateProvinceId);

            return new ChargeAfterCheckoutUI
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
        }

        #endregion
    }
}
