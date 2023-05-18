using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.ChargeAfter.Models
{
    public record PaymentInfoModel : BaseNopModel
    {
        #region Properties

        public string DescriptionText { get; set; }

        #endregion
    }
}
