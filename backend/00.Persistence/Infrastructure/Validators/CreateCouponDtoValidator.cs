using FluentValidation;
using Infrastructure.Dto;

namespace Infrastructure.Validators;

public class CreateCouponDtoValidator : AbstractValidator<CreateCouponDto>
{
    public CreateCouponDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
        RuleFor(x => x)
            .Must(x => x.DiscountAmount.HasValue ^ x.Percent.HasValue)
            .WithMessage("Specify either discount amount or percent");
        RuleFor(x => x.Percent)
            .InclusiveBetween(0, 100)
            .When(x => x.Percent.HasValue);
        RuleFor(x => x.ValidTo)
            .GreaterThanOrEqualTo(x => x.ValidFrom)
            .When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue);
        RuleFor(x => x.MaxUses)
            .GreaterThan(0)
            .When(x => x.MaxUses.HasValue);
    }
}
