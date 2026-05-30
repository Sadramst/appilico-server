using AppilicoShopServer.Business.DTOs.Product;
using FluentValidation;

namespace AppilicoShopServer.Business.Validators.Product;

/// <summary>Validator for CreateProductRequest.</summary>
public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.SKU).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.BrandId).NotEmpty();
        RuleFor(x => x.BasePrice).GreaterThan(0);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinStockLevel).GreaterThanOrEqualTo(0);
    }
}

/// <summary>Validator for UpdateProductRequest.</summary>
public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.BrandId).NotEmpty();
        RuleFor(x => x.BasePrice).GreaterThan(0);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinStockLevel).GreaterThanOrEqualTo(0);
    }
}

/// <summary>Validator for CreateProductVariantRequest.</summary>
public class CreateProductVariantRequestValidator : AbstractValidator<CreateProductVariantRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public CreateProductVariantRequestValidator()
    {
        RuleFor(x => x.VariantName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SKU).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
    }
}
