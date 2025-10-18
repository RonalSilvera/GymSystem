using FluentValidation;
using Infrastructure.Dto;

namespace Infrastructure.Validators;

public class UpdateMembershipTypeDtoValidator : AbstractValidator<UpdateMembershipTypeDto>
{
    public UpdateMembershipTypeDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(255);
        RuleFor(x => x.DurationDays).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
