using Nop.Core.Configuration;
using Nop.Plugin.Payments.ChargeAfter.Domain;
using Nop.Plugin.Payments.ChargeAfter.Domain.Promo;

namespace Nop.Plugin.Payments.ChargeAfter
{
    public class ChargeAfterPaymentSettings : ISettings
    {
        public bool UseProduction { get; set; }

        public string ProductionPublicKey { get; set; }

        public string ProductionPrivateKey { get; set; }

        public string SandboxPublicKey { get; set; }

        public string SandboxPrivateKey { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public TransactionType TypeTransaction { get; set; }

        public bool UseAutoCapture => TypeTransaction.Equals(TransactionType.Capture);

        public CheckoutBrandType TypeCheckoutBrand { get; set; } = CheckoutBrandType.MediumGeneric;

        public bool EnableLineOfCreditPromo { get; set; }

        public LineOfCreditType TypeLineOfCreditPromo { get; set; } = LineOfCreditType.ProductWidgetLineOfCredit;

        public string FinancingPageUrlLineOfCreditPromo { get; set; }

        public bool EnableSimplePromoBeforeContent { get; set; }

        public PromoWidgetType WidgetTypeSimplePromoBeforeContent { get; set; }

        public bool EnableSimplePromoAfterContent { get; set; }

        public PromoWidgetType WidgetTypeSimplePromoAfterContent { get; set; }

        public bool EnableSimplePromoProductBeforeContent { get; set; }

        public PromoWidgetType WidgetTypeSimplePromoProductBeforeContent { get; set; }

        public bool EnableSimplePromoProductAfterTitle { get; set; }

        public PromoWidgetType WidgetTypeSimplePromoProductAfterTitle { get; set; }

        public bool EnableSimplePromoProductAfterDesc { get; set; }

        public PromoWidgetType WidgetTypeSimplePromoProductAfterDesc { get; set; }

        public bool EnableAdvancedSetting { get; set; }
    }
}
