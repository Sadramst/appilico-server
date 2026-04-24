using Appilico.Server.Business.DTOs.Review;
using FluentValidation;

namespace Appilico.Server.Business.Validators.Review;

/// <summary>Validator for CreateReviewRequest.</summary>
public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public CreateReviewRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
    }
}

/// <summary>Validator for UpdateReviewRequest.</summary>
public class UpdateReviewRequestValidator : AbstractValidator<UpdateReviewRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public UpdateReviewRequestValidator()
    {
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
    }
}
