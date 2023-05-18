using System;
using System.IO;
using System.Net.Http;
using Nop.Plugin.Payments.ChargeAfter.Core.Http;
using Nop.Plugin.Payments.ChargeAfter.Payments;

namespace Nop.Plugin.Payments.ChargeAfter.Charges
{
    public class ChargeRefundRequest : HttpRequest
    {
        public ChargeRefundRequest(string ChargeId) : base("/post-sale/charges/{charge_id}/refunds", HttpMethod.Post, typeof(ChargeRefund))
        {
            try
            {
                this.Path = this.Path.Replace("{charge_id}", Uri.EscapeDataString(Convert.ToString(ChargeId)));
            }
            catch (IOException) { }

            this.ContentType = "application/json";
        }

        public ChargeRefundRequest RequestBody(RefundRequest refundRequest)
        {
            this.Body = refundRequest;
            return this;
        }
    }
}
