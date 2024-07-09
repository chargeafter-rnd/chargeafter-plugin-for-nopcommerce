using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.ChargeAfter.Core.Http
{
    [DataContract]
    public class HttpErrorResponse
    {
        [DataMember(Name = "requestId", EmitDefaultValue = false)]
        public string RequestId { get; set; }

        [DataMember(Name = "errors", EmitDefaultValue = false)]
        public ErrorItem[] Errors { get; set; }

        [DataContract]
        public class ErrorItem
        {
            [DataMember(Name = "serviceCode", EmitDefaultValue = false)]
            public string ServiceCode { get; set; }

            [DataMember(Name = "domainCode", EmitDefaultValue = false)]
            public string DomainCode { get; set; }

            [DataMember(Name = "code", EmitDefaultValue = false)]
            public string Code { get; set; }

            [DataMember(Name = "description", EmitDefaultValue = false)]
            public string Description { get; set; }
        }
    }
}