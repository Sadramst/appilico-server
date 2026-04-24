using Appilico.Server.Business.DTOs.Voucher;
using FluentValidation;

namespace Appilico.Server.Business.Validators.Voucher;

/// <summary>Validator for CreateVoucherRequest.</summary>
public class CreateVoucherRequestValidator : AbstractValidator<CreateVoucherRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public CreateVoucherRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Value).GreaterThan(0);
        RuleFor(x => x.StartDate).LessThan(x => x.ExpiryDate);
    }
}
