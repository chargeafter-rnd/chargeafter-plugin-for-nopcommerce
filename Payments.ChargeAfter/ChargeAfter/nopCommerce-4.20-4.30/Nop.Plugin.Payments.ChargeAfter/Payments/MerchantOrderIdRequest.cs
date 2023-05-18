using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.ChargeAfter.Payments
{
    [DataContract]
    public class MerchantOrderIdRequest
    {
        public MerchantOrderIdRequest() { }

        [DataMember(Name = "merchantOrderId", EmitDefaultValue = false)]
        public string MerchantOrderId;
    }
}
