using System;

namespace Nop.Plugin.Payments.ChargeAfter.Core
{
    public class ChargeAfterEnvironment : Nop.Plugin.Payments.ChargeAfter.Core.Http.Environment
    {
        protected string baseUrl;
        private string clientPublic;
        private string clientSecret;
        private bool useProduction;
        private string apiVersion = "/v2";
        private bool isExternal = false;

        public ChargeAfterEnvironment(string clientPublic, string clientSecret, bool useProduction, bool isExternal = false)
        {
            this.clientPublic = clientPublic;
            this.clientSecret = clientSecret;
            this.useProduction = useProduction;
            this.isExternal = isExternal;

            this.baseUrl = this.GenerateBaseUrl();
        }

        private string GenerateBaseUrl()
        {
            if (this.isExternal)
                this.apiVersion = "";

            return string.Concat("https://api", ChargeAfterHelper.GetCaHostByUseProduction(this.useProduction), this.apiVersion);
        }

        public string BaseUrl()
        {
            return this.baseUrl;
        }

        public string ClientPublic()
        {
            return this.clientPublic;
        }

        public string ClientPrivate()
        {
            return this.clientSecret;
        }
    }

    public class CdnEnvironment : ChargeAfterEnvironment
    {
        public CdnEnvironment(string clientPublic, string clientSecret = "", bool useProduction = true) : base(clientPublic, clientSecret, useProduction)
        {
            this.baseUrl = ChargeAfterHelper.GetCdnUrl(useProduction);
        }
    }

    public class SandboxEnvironment : ChargeAfterEnvironment
    {
        public SandboxEnvironment(string clientPublic, string clientSecret, bool isExternal = false) : base(clientPublic, clientSecret, false, isExternal)
        { }
    }

    public class LiveEnvironment : ChargeAfterEnvironment
    {
        public LiveEnvironment(string clientPublic, string clientSecret, bool isExternal = false) : base(clientPublic, clientSecret, true, isExternal)
        { }
    }
}
