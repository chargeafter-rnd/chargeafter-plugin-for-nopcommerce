using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.ChargeAfter.Models
{
    public class NonLeasableProductModel : BaseNopModel
    {
        public int ProductId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.ChargeAfter.Fields.NonLeasable")]
        public bool CaNonLeasable { get; set; }
    }
}
