using Appilico.Server.Business.DTOs.Order;
using FluentValidation;

namespace Appilico.Server.Business.Validators.Order;

/// <summary>Validator for CreateOrderRequest.</summary>
public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.ShippingAddressId).NotEmpty();
        RuleFor(x => x.BillingAddressId).NotEmpty();
    }
}

/// <summary>Validator for UpdateOrderStatusRequest.</summary>
public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.NewStatus).IsInEnum();
    }
}
