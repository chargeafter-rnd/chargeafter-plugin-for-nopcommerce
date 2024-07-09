using System;
using System.IO;
using System.Net.Http;
using Nop.Plugin.Payments.ChargeAfter.Core.Http;
using Nop.Plugin.Payments.ChargeAfter.Payments;

namespace Nop.Plugin.Payments.ChargeAfter.Charges
{
    public class ChargeGetRequest : HttpRequest
    {
        public ChargeGetRequest(string ChargeId) : base("/post-sale/charges/{charge_id}", HttpMethod.Get, typeof(Charge))
        {
            try
            {
                this.Path = this.Path.Replace("{charge_id}", Uri.EscapeDataString(Convert.ToString(ChargeId)));
            }
            catch (IOException) { }

            this.ContentType = "application/json";
        }
    }
}
