using Nop.Plugin.Payments.ChargeAfter.Core.Http;
using Nop.Plugin.Payments.ChargeAfter.Payments;
using System.Net.Http;

namespace Nop.Plugin.Payments.ChargeAfter.Sessions
{
    public class SessionCreateRequest : HttpRequest
    {
        public SessionCreateRequest() : base("/api/sessions", HttpMethod.Post, typeof(Session))
        {
            this.ContentType = "application/json";
        }

        public SessionCreateRequest RequestBody(CreateSessionRequest createSessionRequest)
        {
            this.Body = createSessionRequest;
            return this;
        }
    }
}
