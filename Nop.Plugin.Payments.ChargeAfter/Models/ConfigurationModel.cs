using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Payments.ChargeAfter.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.UseProduction")]
        public bool UseProduction { get; set; }
        public bool UseProduction_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.ProductionPublicKey")]
        [NoTrim]
        public string ProductionPublicKey { get; set; }
        public bool ProductionPublicKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.ProductionPrivateKey")]
        [DataType(DataType.Password)]
        [NoTrim]
        public string ProductionPrivateKey { get; set; }
        public bool ProductionPrivateKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.SandboxPublicKey")]
        [NoTrim]
        public string SandboxPublicKey { get; set; }
        public bool SandboxPublicKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.SandboxPrivateKey")]
        [DataType(DataType.Password)]
        [NoTrim]
        public string SandboxPrivateKey { get; set; }
        public bool SandboxPrivateKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.BrandId")]
        [NoTrim]
        public string BrandId { get; set; }
        public bool BrandId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.EnableLineOfCreditPromo")]
        public bool EnableLineOfCreditPromo { get; set; }
        public bool EnableLineOfCreditPromo_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.FinancingPageUrlLineOfCreditPromo")]
        [NoTrim]
        public string FinancingPageUrlLineOfCreditPromo { get; set; }
        public bool FinancingPageUrlLineOfCreditPromo_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.EnableSimplePromoBeforeContent")]
        public bool EnableSimplePromoBeforeContent { get; set; }
        public bool EnableSimplePromoBeforeContent_OverrideForStore { get; set; }

        public int WidgetTypeSimplePromoBeforeContentId { get; set; }
        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.WidgetTypeSimplePromoBeforeContent")]
        public SelectList WidgetTypeSimplePromoBeforeContentValues { get; set; }
        public bool WidgetTypeSimplePromoBeforeContentId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.EnableSimplePromoAfterContent")]
        public bool EnableSimplePromoAfterContent { get; set; }
        public bool EnableSimplePromoAfterContent_OverrideForStore { get; set; }

        public int WidgetTypeSimplePromoAfterContentId { get; set; }
        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.WidgetTypeSimplePromoAfterContent")]
        public SelectList WidgetTypeSimplePromoAfterContentValues { get; set; }
        public bool WidgetTypeSimplePromoAfterContentId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.EnableSimplePromoProductBeforeContent")]
        public bool EnableSimplePromoProductBeforeContent { get; set; }
        public bool EnableSimplePromoProductBeforeContent_OverrideForStore { get; set; }

        public int WidgetTypeSimplePromoProductBeforeContentId { get; set; }
        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.WidgetTypeSimplePromoProductBeforeContent")]
        public SelectList WidgetTypeSimplePromoProductBeforeContentValues { get; set; }
        public bool WidgetTypeSimplePromoProductBeforeContentId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.EnableSimplePromoProductAfterTitle")]
        public bool EnableSimplePromoProductAfterTitle { get; set; }
        public bool EnableSimplePromoProductAfterTitle_OverrideForStore { get; set; }

        public int WidgetTypeSimplePromoProductAfterTitleId { get; set; }
        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.WidgetTypeSimplePromoProductAfterTitle")]
        public SelectList WidgetTypeSimplePromoProductAfterTitleValues { get; set; }
        public bool WidgetTypeSimplePromoProductAfterTitleId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.EnableSimplePromoProductAfterDesc")]
        public bool EnableSimplePromoProductAfterDesc { get; set; }
        public bool EnableSimplePromoProductAfterDesc_OverrideForStore { get; set; }

        public int WidgetTypeSimplePromoProductAfterDescId { get; set; }
        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.WidgetTypeSimplePromoProductAfterDesc")]
        public SelectList WidgetTypeSimplePromoProductAfterDescValues { get; set; }
        public bool WidgetTypeSimplePromoProductAfterDescId_OverrideForStore { get; set; }
    }
}
