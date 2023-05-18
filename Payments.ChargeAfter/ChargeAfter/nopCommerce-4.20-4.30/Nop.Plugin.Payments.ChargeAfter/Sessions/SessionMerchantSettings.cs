using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.ChargeAfter.Sessions
{
    [DataContract]
    public class SessionMerchantSettings
    {
        public SessionMerchantSettings() { }

        [DataMember(Name = "brandId", EmitDefaultValue = false)]
        public string BrandId;
    }
}
