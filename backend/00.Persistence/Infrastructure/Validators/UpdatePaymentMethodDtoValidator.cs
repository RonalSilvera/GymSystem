using FluentValidation;
using Infrastructure.Dto;

namespace Infrastructure.Validators;

public class UpdatePaymentMethodDtoValidator : AbstractValidator<UpdatePaymentMethodDto>
{
    public UpdatePaymentMethodDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
