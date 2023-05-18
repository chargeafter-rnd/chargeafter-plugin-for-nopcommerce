using Microsoft.AspNetCore.Http;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.ChargeAfter.Models
{
    public class IpnModel : BaseNopModel
    {
        public FormCollection Form { get; set; }
    }
}
