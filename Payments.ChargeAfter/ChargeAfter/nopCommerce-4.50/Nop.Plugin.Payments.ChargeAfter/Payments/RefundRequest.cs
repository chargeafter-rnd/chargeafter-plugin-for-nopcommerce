
using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.ChargeAfter.Payments
{
    [DataContract]
    public class RefundRequest
    {
        public RefundRequest() { }

        [DataMember(Name = "amount", EmitDefaultValue = false)]
        public decimal Amount;
    }
}
