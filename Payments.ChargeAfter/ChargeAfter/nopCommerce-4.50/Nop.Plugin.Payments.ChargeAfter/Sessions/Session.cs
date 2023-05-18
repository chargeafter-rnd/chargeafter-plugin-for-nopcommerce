using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.ChargeAfter.Sessions
{
    [DataContract]
    public class Session
    {
        public Session() { }

        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string SessionId;

    }
}
