using Nop.Core;

namespace Nop.Plugin.Payments.ChargeAfter.Domain
{
    public class ChargeAfterCheckoutUI : BaseEntity
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string BillingAddressLine1 { get; set; }

        public string BillingAddressLine2 { get; set; }

        public string BillingAddressCity { get; set; }

        public string BillingAddressZipCode { get; set; }

        public string BillingAddressState { get; set; }

        public string ShippingAddressLine1 { get; set; }

        public string ShippingAddressLine2 { get; set; }

        public string ShippingAddressCity { get; set; }

        public string ShippingAddressZipCode { get; set; }

        public string ShippingAddressState { get; set; }
    }
}
