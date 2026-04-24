using Appilico.Server.Business.DTOs.Auth;
using FluentValidation;

namespace Appilico.Server.Business.Validators.Auth;

/// <summary>Validator for LoginRequest.</summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

/// <summary>Validator for RegisterRequest.</summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Passwords do not match");
    }
}

/// <summary>Validator for ForgotPasswordRequest.</summary>
public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

/// <summary>Validator for ResetPasswordRequest.</summary>
public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
    }
}

/// <summary>Validator for UpdateProfileRequest.</summary>
public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    /// <summary>Initializes validation rules.</summary>
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}
