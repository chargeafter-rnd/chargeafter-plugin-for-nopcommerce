using System;
using System.IO;
using System.Net.Http;
using Nop.Plugin.Payments.ChargeAfter.Core.Http;
using Nop.Plugin.Payments.ChargeAfter.Payments;

namespace Nop.Plugin.Payments.ChargeAfter.Charges
{
    public class ChargeMerchantOrderIdRequest : HttpRequest
    {
        public ChargeMerchantOrderIdRequest(string ChargeId) : base("/post-sale/charges/{charge_id}", HttpMethod.Patch, typeof(Charge))
        {
            try
            {
                this.Path = this.Path.Replace("{charge_id}", Uri.EscapeDataString(Convert.ToString(ChargeId)));
            }
            catch (IOException) { }

            this.ContentType = "application/json";
        }

        public ChargeMerchantOrderIdRequest RequestBody(MerchantOrderIdRequest merchantOrderIdRequest)
        {
            this.Body = merchantOrderIdRequest;
            return this;
        }
    }
}
