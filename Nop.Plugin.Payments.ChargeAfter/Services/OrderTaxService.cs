using Nop.Core.Domain.Orders;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using System;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.ChargeAfter.Services
{
    public interface IOrderTaxService
    {
        public Task UpdateTaxFreeAsync(int orderId);
    }

    public class OrderTaxService : IOrderTaxService
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;

        #endregion


        #region Ctor

        public OrderTaxService(
            IOrderService orderService, 
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService
        )
        {
            _orderService = orderService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
        }

        #endregion

        #region Methods

        public async Task UpdateTaxFreeAsync(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId)
                ?? throw new Exception(string.Format("Invalid order data (Order Id {0})", orderId));

            // Update order
            await UpdateOrderTaxAsync(order);

            // Update order items
            await UpdateOrderItemsTaxAsync(order);

            //add a note
            await AddNoteEditOrderTaxAsync(order);

            //log update
            await LogEditOrderAsync(order);
        }

        private async Task UpdateOrderTaxAsync(Order order)
        {
            // Order sub total
            order.OrderSubtotalInclTax = order.OrderSubtotalExclTax;
            order.OrderSubTotalDiscountInclTax = order.OrderSubTotalDiscountExclTax;

            // Order shipping & payment
            order.OrderShippingInclTax = order.OrderShippingExclTax;
            order.PaymentMethodAdditionalFeeInclTax = order.PaymentMethodAdditionalFeeExclTax;

            // Order total
            order.OrderTotal = order.OrderTotal - order.OrderTax;

            // Order Tax
            order.TaxRates = "0:0;";
            order.OrderTax = decimal.Zero;

            // Order Tax Display
            order.CustomerTaxDisplayType = Nop.Core.Domain.Tax.TaxDisplayType.ExcludingTax;

            await _orderService.UpdateOrderAsync(order);
        }

        private async Task UpdateOrderItemsTaxAsync(Order order)
        {
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);

            foreach (var orderItem in orderItems)
            {
                orderItem.PriceInclTax = orderItem.PriceExclTax;
                orderItem.UnitPriceInclTax = orderItem.UnitPriceExclTax;
                orderItem.DiscountAmountInclTax = orderItem.DiscountAmountExclTax;

                await _orderService.UpdateOrderItemAsync(orderItem);
            }
        }

        private async Task AddNoteEditOrderTaxAsync(Order order)
        {
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = "Order tax have been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
        }

        private async Task LogEditOrderAsync(Order order)
        {
            await _customerActivityService.InsertActivityAsync("EditOrder",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditOrder"), order.CustomOrderNumber), order);
        }

        #endregion
    }
}
