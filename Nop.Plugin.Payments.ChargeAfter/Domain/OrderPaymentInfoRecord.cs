using Nop.Core;

namespace Nop.Plugin.Payments.ChargeAfter.Domain
{
    public partial class OrderPaymentInfoRecord : BaseEntity
    {
        public int StoreId { get; set; }

        public int OrderId { get; set; }

        public string PaymentInfo { get; set; }
    }
}
