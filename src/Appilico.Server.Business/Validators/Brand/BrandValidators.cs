using Appilico.Server.Business.DTOs.Brand;
using FluentValidation;

namespace Appilico.Server.Business.Validators.Brand;

/// <summary>Validator for CreateBrandRequest.</summary>
public class CreateBrandRequestValidator : AbstractValidator<CreateBrandRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public CreateBrandRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

/// <summary>Validator for UpdateBrandRequest.</summary>
public class UpdateBrandRequestValidator : AbstractValidator<UpdateBrandRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public UpdateBrandRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
