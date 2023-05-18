using Nop.Plugin.Payments.ChargeAfter.Core.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace Nop.Plugin.Payments.ChargeAfter.Sessions
{
    public class SessionGetMerchantIdRequest : HttpRequest
    {
        public SessionGetMerchantIdRequest(string SessionId): base("/api/sessions/{session_id}?projection=MerchantId", HttpMethod.Get, typeof(SessionMerchantId))
        {
            try {
                this.Path = this.Path.Replace("{session_id}", Uri.EscapeDataString(Convert.ToString(SessionId)));
            } 
            catch(IOException) { }

            this.ContentType = "application/json";
        }
    }
}
