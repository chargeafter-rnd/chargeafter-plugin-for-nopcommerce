using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.ChargeAfter.Sessions
{
    [DataContract]
    public class SessionMerchantId
    {
        public SessionMerchantId() { }

        [DataMember(Name = "data", EmitDefaultValue = false)]
        public MerchantData Data;

    }

    [DataContract]
    public class MerchantData
    {
        public MerchantData() { }

        [DataMember(Name = "merchantId", EmitDefaultValue = false)]
        public string MerchantId;
    }
}
