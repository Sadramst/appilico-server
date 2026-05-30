using AppilicoShopServer.Business.DTOs.Cart;
using FluentValidation;

namespace AppilicoShopServer.Business.Validators.Cart;

/// <summary>Validator for AddToCartRequest.</summary>
public class AddToCartRequestValidator : AbstractValidator<AddToCartRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public AddToCartRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

/// <summary>Validator for UpdateCartItemRequest.</summary>
public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public UpdateCartItemRequestValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
