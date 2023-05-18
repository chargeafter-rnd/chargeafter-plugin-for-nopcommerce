﻿using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.ChargeAfter.Charges
{
    [DataContract]
    public class ChargeCapture
    {
        public ChargeCapture() { }

        [DataMember(Name = "chargeId", EmitDefaultValue = false)]
        public string ChargeId;

        [DataMember(Name = "merchantOrderId", EmitDefaultValue = false)]
        public string MerchantOrderId;

        [DataMember(Name = "createdAt", EmitDefaultValue = false)]
        public string CreatedAt;
    }
}
