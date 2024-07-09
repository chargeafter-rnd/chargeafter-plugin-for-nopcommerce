using System.Net.Http;
using Nop.Plugin.Payments.ChargeAfter.Core.Http;
using Nop.Plugin.Payments.ChargeAfter.Payments;

namespace Nop.Plugin.Payments.ChargeAfter.Charges
{
    public class ChargeAuthorizeRequest : HttpRequest
    {
        public ChargeAuthorizeRequest() : base("/payment/charges", HttpMethod.Post, typeof(Charge))
        {
            this.ContentType = "application/json";
        }

        public ChargeAuthorizeRequest RequestBody(AuthorizeRequest authorizeRequest)
        {
            this.Body = authorizeRequest;
            return this;
        }
    }
}
