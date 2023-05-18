using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.ChargeAfter.Charges
{
    [DataContract]
    public class ChargeRefund
    {
        public ChargeRefund() { }

        [DataMember(Name = "chargeId", EmitDefaultValue = false)]
        public string ChargeId;

        [DataMember(Name = "merchantOrderId", EmitDefaultValue = false)]
        public string MerchantOrderId;

        [DataMember(Name = "amount", EmitDefaultValue = false)]
        public decimal Amount;

        [DataMember(Name = "createdAt", EmitDefaultValue = false)]
        public string CreatedAt;
    }
}
