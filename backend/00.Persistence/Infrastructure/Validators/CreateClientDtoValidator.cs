using FluentValidation;
using Infrastructure.Dto;

namespace Infrastructure.Validators;

public class CreateClientDtoValidator : AbstractValidator<CreateClientDto>
{
    public CreateClientDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DocumentNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(150).When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.Address).MaximumLength(255);
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MembershipTypeId).GreaterThan(0).When(x => x.MembershipTypeId.HasValue);
    }
}
