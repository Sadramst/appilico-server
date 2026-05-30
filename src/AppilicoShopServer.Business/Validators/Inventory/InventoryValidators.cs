using AppilicoShopServer.Business.DTOs.Inventory;
using FluentValidation;

namespace AppilicoShopServer.Business.Validators.Inventory;

/// <summary>Validator for AdjustInventoryRequest.</summary>
public class AdjustInventoryRequestValidator : AbstractValidator<AdjustInventoryRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public AdjustInventoryRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.TransactionType).IsInEnum();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
