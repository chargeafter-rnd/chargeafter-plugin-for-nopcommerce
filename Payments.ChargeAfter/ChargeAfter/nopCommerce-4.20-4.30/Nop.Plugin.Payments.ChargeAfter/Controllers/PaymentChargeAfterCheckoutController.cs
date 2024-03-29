﻿using Microsoft.AspNetCore.Http;
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

namespace Nop.Plugin.Payments.ChargeAfter.Controllers
{
    public class PaymentChargeAfterCheckoutController : BasePaymentController
    {
        private const string ProcessPaymentRequestKey = "OrderPaymentInfo";

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
            _genericAttributeService = genericAttributeService;
            _paymentService = paymentService;
            _storeContext = storeContext;
            _workContext = workContext;
            _logger = logger;
        }

        #endregion

        #region Methods

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult Place(IFormCollection form)
        {
            try {
                var confirmationToken = GetValue("ca_token", form).ToString();
                if (string.IsNullOrEmpty(confirmationToken))
                {
                    throw new NopException("Incorrect confirmation token");
                }

                var cart = _shoppingCartService.GetShoppingCart(
                    _workContext.CurrentCustomer,
                    ShoppingCartType.ShoppingCart,
                    _storeContext.CurrentStore.Id
                );

                if (!cart.Any())
                {
                    throw new NopException("Your cart is empty");
                }

                var processPaymentRequest = HttpContext.Session.Get<ProcessPaymentRequest>(ProcessPaymentRequestKey);
                if (processPaymentRequest == null)
                {
                    processPaymentRequest = new ProcessPaymentRequest();
                }

                // Process Payment
                _paymentService.GenerateOrderGuid(processPaymentRequest);

                processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
                processPaymentRequest.CustomerId = _workContext.CurrentCustomer.Id;
                processPaymentRequest.PaymentMethodSystemName = _genericAttributeService.GetAttribute<string>(
                    _workContext.CurrentCustomer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute,
                    _storeContext.CurrentStore.Id
                );
                processPaymentRequest.CustomValues.Add(Constants.CA_TOKEN_KEY, confirmationToken);

                HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", processPaymentRequest);
                var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest);

                // Post Process Payment
                if (placeOrderResult.Success)
                {
                    HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);
                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placeOrderResult.PlacedOrder
                    };
                    _paymentService.PostProcessPayment(postProcessPaymentRequest);

                    return Json(new { redirect = Url.RouteUrl("CheckoutCompleted", new { orderId = placeOrderResult.PlacedOrder.Id }) });
                }

                // Log errors
                foreach (var error in placeOrderResult.Errors)
                {
                    _logger.Error("ChargeAfter Order Place Error", new NopException(error));
                }

                // Clear process payment request data
                HttpContext.Session.Remove(ProcessPaymentRequestKey);

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
