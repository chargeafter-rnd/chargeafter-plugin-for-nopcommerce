using FluentValidation;
using Nop.Plugin.Payments.ChargeAfter.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Payments.ChargeAfter.Validators
{
    public class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
    {
        #region Ctor

        public ConfigurationValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.SandboxPublicKey)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.Payments.ChargeAfter.Fields.SandboxPublicKey.Required"))
                .When(model => !model.UseProduction);

            RuleFor(model => model.SandboxPrivateKey)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.Payments.ChargeAfter.Fields.SandboxPrivateKey.Required"))
                .When(model => !model.UseProduction);

            RuleFor(model => model.ProductionPublicKey)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.Payments.ChargeAfter.Fields.ProductionPublicKey.Required"))
                .When(model => model.UseProduction);

            RuleFor(model => model.ProductionPrivateKey)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.Payments.ChargeAfter.Fields.ProductionPrivateKey.Required"))
                .When(model => model.UseProduction);

            RuleFor(model => model.FinancingPageUrlLineOfCreditPromo)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.Payments.ChargeAfter.Fields.ProductionPublicKey.Required"))
                .When(model => model.EnableLineOfCreditPromo);
        }

        #endregion
    }
}
