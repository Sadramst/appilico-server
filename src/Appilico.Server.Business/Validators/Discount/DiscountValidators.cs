using Appilico.Server.Business.DTOs.Discount;
using FluentValidation;

namespace Appilico.Server.Business.Validators.Discount;

/// <summary>Validator for CreateDiscountRequest.</summary>
public class CreateDiscountRequestValidator : AbstractValidator<CreateDiscountRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public CreateDiscountRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Value).GreaterThan(0);
        RuleFor(x => x.StartDate).LessThan(x => x.EndDate);
    }
}
