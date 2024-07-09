using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.ChargeAfter.Charges
{
    [DataContract]
    public class Charge
    {
        public Charge() { }

        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string ChargeId;

        [DataMember(Name = "consumerId", EmitDefaultValue = false)]
        public string ConsumerId;

        [DataMember(Name = "state", EmitDefaultValue = false)]
        public string State;

        [DataMember(Name = "totalAmount", EmitDefaultValue = false)]
        public decimal TotalAmount;

        [DataMember(Name = "settledAmount", EmitDefaultValue = false)]
        public decimal SettledAmount;

        [DataMember(Name = "refundedAmount", EmitDefaultValue = false)]
        public decimal RefundedAmount;

        [DataMember(Name = "createdAt", EmitDefaultValue = false)]
        public string CreatedAt;
    }
}
