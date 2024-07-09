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
        public void UpdateTaxFree(int orderId);
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

        public void UpdateTaxFree(int orderId)
        {
            var order = _orderService.GetOrderById(orderId)
                ?? throw new Exception(string.Format("Invalid order data (Order Id {0})", orderId));

            // Update order
            UpdateOrderTax(order);

            // Update order items
            UpdateOrderItemsTax(order);

            //add a note
            AddNoteEditOrderTax(order);

            //log update
            LogEditOrder(order);
        }

        private void UpdateOrderTax(Order order)
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

            _orderService.UpdateOrder(order);
        }

        private void UpdateOrderItemsTax(Order order)
        {
            var orderItems = _orderService.GetOrderItems(order.Id);

            foreach (var orderItem in orderItems)
            {
                orderItem.PriceInclTax = orderItem.PriceExclTax;
                orderItem.UnitPriceInclTax = orderItem.UnitPriceExclTax;
                orderItem.DiscountAmountInclTax = orderItem.DiscountAmountExclTax;

                _orderService.UpdateOrderItem(orderItem);
            }
        }

        private void AddNoteEditOrderTax(Order order)
        {
            _orderService.InsertOrderNote(new OrderNote
            {
                OrderId = order.Id,
                Note = "Order tax have been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
        }

        private void LogEditOrder(Order order)
        {
            _customerActivityService.InsertActivity("EditOrder",
                string.Format(_localizationService.GetResource("ActivityLog.EditOrder"), order.CustomOrderNumber), order);
        }

        #endregion
    }
}
