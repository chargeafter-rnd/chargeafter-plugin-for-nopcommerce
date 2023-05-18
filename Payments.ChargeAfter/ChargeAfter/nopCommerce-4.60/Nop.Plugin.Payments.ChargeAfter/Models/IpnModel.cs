using Microsoft.AspNetCore.Http;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.ChargeAfter.Models
{
    public record IpnModel : BaseNopModel
    {
        public FormCollection Form { get; set; }
    }
}
