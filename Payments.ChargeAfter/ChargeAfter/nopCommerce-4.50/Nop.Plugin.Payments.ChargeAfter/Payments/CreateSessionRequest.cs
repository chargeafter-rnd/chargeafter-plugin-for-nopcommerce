using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.ChargeAfter.Payments
{
    [DataContract]
    public class CreateSessionRequest
    {
        public CreateSessionRequest() { }

        [DataMember(Name = "requestInfo", EmitDefaultValue = false)]
        public SessionRequestInfo RequestInfo;
    }

    public class SessionRequestInfo
    {
        public string FlowType { get; set; }

        public string Channel { get; set; }

        public string Source { get; set; }
    }
}
