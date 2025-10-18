using FluentValidation;
using Infrastructure.Dto;

namespace Infrastructure.Validators;

public class CreatePaymentMethodDtoValidator : AbstractValidator<CreatePaymentMethodDto>
{
    public CreatePaymentMethodDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
