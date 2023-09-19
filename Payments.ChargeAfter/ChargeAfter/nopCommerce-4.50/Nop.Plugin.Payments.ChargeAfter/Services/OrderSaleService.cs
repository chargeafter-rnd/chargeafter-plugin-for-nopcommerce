using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;

namespace Nop.Plugin.Payments.ChargeAfter.Services
{
    public interface IOrderSaleService
    {
        public Task CaptureAsync(int orderId);
    }

    public class OrderSaleService : IOrderSaleService
    {
        #region Fields

        private readonly IOrderProcessingService _orderProcessingService;
        
        private readonly IOrderService _orderService;

        private readonly ICustomerActivityService _customerActivityService;
        
        private readonly ILocalizationService _localizationService;

        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public OrderSaleService(
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            ILogger logger
        ) {
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _logger = logger;
        }

        #endregion

        #region Methods

        public async Task CaptureAsync(int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);

                var errors = await _orderProcessingService.CaptureAsync(order);
                await LogEditOrderAsync(order.Id);

                if(errors != null && errors.Count > 0)
                    throw new NopException(errors.First<string>());
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
            }
        }

        private async Task LogEditOrderAsync(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            await _customerActivityService.InsertActivityAsync("EditOrder",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditOrder"), order.CustomOrderNumber), order);
        }

        #endregion
    }
}
