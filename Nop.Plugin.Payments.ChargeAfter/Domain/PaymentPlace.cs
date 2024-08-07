using Microsoft.AspNetCore.Mvc;

namespace Nop.Plugin.Payments.ChargeAfter.Domain
{
    public class PaymentPlace
    {
        [BindProperty(Name = "ca_token")]
        public string Token { get; set; }

        public Details Data { get; set; }

        public class Details
        {
            public LenderDetails Lender { get; set; }

            public class LenderDetails
            {
                public string Name { get; set; }

                public InformationDetails Information { get; set; }

                public class InformationDetails
                {
                    public string LeaseId { get; set; }
                }
            }
        }
    }
}
