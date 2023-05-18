using Nop.Plugin.Payments.ChargeAfter.Core.Http;
using System;
using System.IO;
using System.Net.Http;

namespace Nop.Plugin.Payments.ChargeAfter.Sessions
{
    public class SessionGetMerchantSettingRequest : HttpRequest
    {
        public SessionGetMerchantSettingRequest(string merchantId) : base("assets/merchants/{merchant_id}/settings.json", HttpMethod.Get, typeof(SessionMerchantSettings))
        {
            try
            {
                this.Path = this.Path.Replace("{merchant_id}", Uri.EscapeDataString(Convert.ToString(merchantId)));
            }
            catch (IOException) { }

            this.ContentType = "application/json";
        }
    }
}
