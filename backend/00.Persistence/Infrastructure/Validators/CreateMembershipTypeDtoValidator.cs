using FluentValidation;
using Infrastructure.Dto;

namespace Infrastructure.Validators;

public class CreateMembershipTypeDtoValidator : AbstractValidator<CreateMembershipTypeDto>
{
    public CreateMembershipTypeDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(255);
        RuleFor(x => x.DurationDays).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
