using System;
using System.Linq;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;

namespace Nop.Plugin.Payments.ChargeAfter.Services
{
    public interface IOrderSaleService
    {
        public void Capture(int orderId);
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

        public void Capture(int orderId)
        {
            try
            {
                var order = _orderService.GetOrderById(orderId);

                var errors = _orderProcessingService.Capture(order);
                LogEditOrder(order.Id);

                if(errors != null && errors.Count > 0)
                    throw new NopException(errors.First<string>());
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }

        private void LogEditOrder(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);

            _customerActivityService.InsertActivity("EditOrder",
                string.Format(_localizationService.GetResource("ActivityLog.EditOrder"), order.CustomOrderNumber), order);
        }

        #endregion
    }
}
