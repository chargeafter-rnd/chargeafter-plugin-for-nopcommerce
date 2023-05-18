using System;
using System.IO;
using System.Net.Http;
using Nop.Plugin.Payments.ChargeAfter.Core.Http;

namespace Nop.Plugin.Payments.ChargeAfter.Charges
{
    public class ChargeVoidRequest : HttpRequest
    {
        public ChargeVoidRequest(string ChargeId) : base("/post-sale/charges/{charge_id}/voids", HttpMethod.Post, typeof(ChargeVoid))
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
