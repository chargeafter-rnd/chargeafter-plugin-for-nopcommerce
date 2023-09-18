using System;
using System.Text.RegularExpressions;

namespace Nop.Plugin.Payments.ChargeAfter.Domain
{
    public static class EnumExtension
    {
        public static string ToKebabCaseString(this Enum value) 
        {
            return Regex.Replace(value.ToString(), "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])", "-$1", RegexOptions.Compiled)
                        .Trim()
                        .ToLower();
        }
    }
}
