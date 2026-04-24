using Appilico.Server.Business.DTOs.Category;
using FluentValidation;

namespace Appilico.Server.Business.Validators.Category;

/// <summary>Validator for CreateCategoryRequest.</summary>
public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

/// <summary>Validator for UpdateCategoryRequest.</summary>
public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
