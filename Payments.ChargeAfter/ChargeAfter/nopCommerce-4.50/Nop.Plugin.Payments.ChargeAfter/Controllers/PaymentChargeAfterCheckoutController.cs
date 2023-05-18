using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Controllers;
using Nop.Plugin.Payments.ChargeAfter.Infrastructure;
using Nop.Core;
using Nop.Core.Http.Extensions;
using Nop.Services.Orders;
using System;
using Nop.Services.Payments;
using Nop.Services.Logging;
using Nop.Core.Domain.Orders;
using System.Linq;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.ChargeAfter.Controllers
{
    public class PaymentChargeAfterCheckoutController : BasePaymentController
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPaymentService _paymentService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public PaymentChargeAfterCheckoutController(
            IWebHelper webHelper,
            IShoppingCartService shoppingCartService,
            IOrderProcessingService orderProcessingService,
            IPaymentService paymentService,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            IWorkContext workContext,
            ILogger logger
        ) {
            _webHelper = webHelper;
            _shoppingCartService = shoppingCartService;
            _orderProcessingService = orderProcessingService;
            _paymentService = paymentService;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _workContext = workContext;
            _logger = logger;
        }

        #endregion

        #region Methods

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PlaceAsync(IFormCollection form)
        {
            try {
                var confirmationToken = GetValue("ca_token", form).ToString();
                if (string.IsNullOrEmpty(confirmationToken))
                {
                    throw new NopException("Incorrect confirmation token");
                }

                var cart = await _shoppingCartService.GetShoppingCartAsync(
                    await _workContext.GetCurrentCustomerAsync(),
                    ShoppingCartType.ShoppingCart,
                    _storeContext.GetCurrentStore().Id
                );

                if (!cart.Any())
                {
                    throw new NopException("Your cart is empty");
                }

                var processPaymentRequest = HttpContext.Session.Get<ProcessPaymentRequest>("OrderPaymentInfo");
                if (processPaymentRequest == null)
                {
                    processPaymentRequest = new ProcessPaymentRequest();
                }

                // Process Payment
                _paymentService.GenerateOrderGuid(processPaymentRequest);

                var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                var currentStoreId = _storeContext.GetCurrentStore().Id;

                processPaymentRequest.StoreId = currentStoreId;
                processPaymentRequest.CustomerId = currentCustomer.Id;
                processPaymentRequest.PaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(
                    currentCustomer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute,
                    currentStoreId
                );
                processPaymentRequest.CustomValues.Add(Constants.CA_TOKEN_KEY, confirmationToken);

                HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", processPaymentRequest);
                var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);

                // Post Process Payment
                if (placeOrderResult.Success)
                {
                    HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);
                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placeOrderResult.PlacedOrder
                    };
                    await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

                    return Json(new { redirect = Url.RouteUrl("CheckoutCompleted", new { orderId = placeOrderResult.PlacedOrder.Id }) });
                }

                // Log errors
                foreach (var error in placeOrderResult.Errors)
                {
                    await _logger.ErrorAsync("ChargeAfter Order Place Error", new NopException(error));
                }

                throw new NopException("Payment Error. Please try again");
            } catch(Exception e) {
                return Json(new { error = 1, message = e.Message });
            }
        }

        #endregion

        #region Utils

        private string GetValue(string key, IFormCollection form)
        {
            return (form.Keys.Contains(key) ? form[key].ToString() : _webHelper.QueryString<string>(key)) ?? string.Empty;
        }

        #endregion
    }
}
