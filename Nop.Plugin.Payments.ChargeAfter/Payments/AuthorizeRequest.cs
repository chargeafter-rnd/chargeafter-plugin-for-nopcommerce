using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.ChargeAfter.Payments
{
    [DataContract]
    public class AuthorizeRequest
    {
        public AuthorizeRequest() { }

        [DataMember(Name = "confirmationToken", EmitDefaultValue = false)]
        public string ConfirmationToken;
    }
}
