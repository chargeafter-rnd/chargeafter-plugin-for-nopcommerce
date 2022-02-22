using Nop.Data;
using Nop.Plugin.Payments.ChargeAfter.Domain;
using System;
using System.Linq;

namespace Nop.Plugin.Payments.ChargeAfter.Services
{
    public interface IOrderPaymentInfoService
    {
        void Set(OrderPaymentInfoRecord record);

        OrderPaymentInfoRecord Get(int storeId, int orderId);
    }
}

namespace Nop.Plugin.Payments.ChargeAfter.Services
{
    public class OrderPaymentInfoService : IOrderPaymentInfoService
    {
        #region Fields

        private readonly IRepository<OrderPaymentInfoRecord> _orderPaymentInfoRecordRepository;

        #endregion

        #region Ctor

        public OrderPaymentInfoService(IRepository<OrderPaymentInfoRecord> orderPaymentInfoRecordRepository)
        {
            _orderPaymentInfoRecordRepository = orderPaymentInfoRecordRepository;
        }

        #endregion

        #region Methods

        public virtual void Set(OrderPaymentInfoRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            _orderPaymentInfoRecordRepository.Insert(record);
        }

        public virtual OrderPaymentInfoRecord Get(int storeId, int orderId)
        {
            var query = _orderPaymentInfoRecordRepository.Table;

            if (string.IsNullOrEmpty(orderId.ToString()))
                throw new ArgumentNullException(nameof(orderId));

            query = query.Where(configuration => configuration.StoreId == storeId);

            return query.Where(configuration => configuration.OrderId == orderId).FirstOrDefault();
        }

        #endregion
    }
}
