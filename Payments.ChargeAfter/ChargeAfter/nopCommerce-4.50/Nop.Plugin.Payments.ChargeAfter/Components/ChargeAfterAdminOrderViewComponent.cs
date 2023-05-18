using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.ChargeAfter.Components
{
    [ViewComponent(Name = Defaults.ADMIN_ORDER_VIEW_COMPONENT_NAME)]
    public class ChargeAfterAdminOrderViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public ChargeAfterAdminOrderViewComponent(
            IPaymentPluginManager paymentPluginManager,
            IStoreContext storeContext,
            IWorkContext workContext,
            IOrderService orderService
        ) {
            _paymentPluginManager = paymentPluginManager;
            _storeContext = storeContext;
            _workContext = workContext;
            _orderService = orderService;
        }

        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!await _paymentPluginManager.IsPluginActiveAsync(Defaults.SystemName, 
                                                                 await _workContext.GetCurrentCustomerAsync(), 
                                                                 _storeContext.GetCurrentStore().Id))
                return Content(string.Empty);

            //ensure that it's a proper widget zone
            if (!widgetZone.Equals(AdminWidgetZones.OrderDetailsButtons))
                return Content(string.Empty);

            var orderId = additionalData is OrderModel model ? model.Id : 0;
            if (orderId == 0)
                return Content(string.Empty);

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.PaymentMethodSystemName == null || !order.PaymentMethodSystemName.Equals(Defaults.SystemName))
                return Content(string.Empty);

            return View("~/Plugins/Payments.ChargeAfter/Areas/Admin/Views/Order.cshtml");
        }

        #endregion
    }
}
