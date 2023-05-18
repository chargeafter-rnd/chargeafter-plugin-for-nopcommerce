
using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.ChargeAfter.Payments
{
    [DataContract]
    public class CaptureRequest
    {
        public CaptureRequest() { }

        [DataMember(Name = "amount", EmitDefaultValue = false)]
        public decimal Amount;
    }
}
