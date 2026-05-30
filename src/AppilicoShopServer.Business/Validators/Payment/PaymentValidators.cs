using AppilicoShopServer.Business.DTOs.Payment;
using FluentValidation;

namespace AppilicoShopServer.Business.Validators.Payment;

/// <summary>Validator for CreatePaymentRequest.</summary>
public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.PaymentMethod).IsInEnum();
    }
}

/// <summary>Validator for CreateRefundRequest.</summary>
public class CreateRefundRequestValidator : AbstractValidator<CreateRefundRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public CreateRefundRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
