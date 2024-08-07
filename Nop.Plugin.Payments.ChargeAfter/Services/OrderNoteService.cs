using System;
using System.Globalization;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Services.Localization;
using Nop.Services.Orders;

namespace Nop.Plugin.Payments.ChargeAfter.Services
{
    public interface IOrderNoteService
    {
        public Task ChargeAuthorizeAsync(int orderId, string chargeId);

        public Task ChargeVoidAsync(int orderId, string voidId);

        public Task ChargeSettleAsync(int orderId, string settleId);

        public Task ChargeRefundAsync(int orderId, string refundId, decimal amount);
    }

    public class OrderNoteService : IOrderNoteService
    {
        private readonly ILocalizationService _localizationService;

        private readonly IOrderService _orderService;

        public OrderNoteService(
            ILocalizationService localizationService,
            IOrderService orderService
        ) {
            _localizationService = localizationService;
            _orderService = orderService;
        }

        public async Task ChargeAuthorizeAsync(int orderId, string chargeId)
        {
            var message = await GetResourceAsync("ChargeAuthorize");

            await AddOrderNoteAsync(orderId, message.Replace("{0}", chargeId));
        }

        public async Task ChargeVoidAsync(int orderId, string voidId)
        {
            var message = await GetResourceAsync("ChargeVoid");

            await AddOrderNoteAsync(orderId, message.Replace("{0}", voidId));
        }

        public async Task ChargeSettleAsync(int orderId, string settleId)
        {
            var message = await GetResourceAsync("ChargeSettle");

            await AddOrderNoteAsync(orderId, message.Replace("{0}", settleId));
        }

        public async Task ChargeRefundAsync(int orderId, string refundId, decimal amount)
        {
            var message = await GetResourceAsync("ChargeRefund");

            await AddOrderNoteAsync(
                orderId,
                message.Replace("{0}", amount.ToString()).Replace("{1}", refundId)
            );
        }

        private Task<string> GetResourceAsync(string resourceKey)
        {
            return _localizationService.GetResourceAsync(
                string.Concat("Plugins.Payment.ChargeAfter.OrderNote.", resourceKey)
            );
        }

        private Task AddOrderNoteAsync(int orderId, string message)
        {
            var orderNote = new OrderNote
            {
                OrderId = orderId,
                DisplayToCustomer = false,
                Note = message,
                CreatedOnUtc = DateTime.UtcNow
            };

            return _orderService.InsertOrderNoteAsync(orderNote);
        }
    }
}
