using Ecommerce.Entities.DTO.Discount;
using Ecommerce.Utilities.Enums;
using FluentValidation;

namespace Ecommerce.API.Validators
{
    public class CreateDiscountDtoValidator : AbstractValidator<CreateDiscountDto>
    {
        public CreateDiscountDtoValidator()
        {
            RuleFor(x => x.Code).NotEmpty();
            RuleFor(x => x.Type).IsInEnum();
            RuleFor(x => x.Value)
                .GreaterThan(0)
                .When(x => x.Type == DiscountType.FixedAmount)
                .WithMessage("Fixed amount must be greater than 0.")
                .InclusiveBetween(0, 100)
                .When(x => x.Type == DiscountType.Percentage)
                .WithMessage("Percentage must be between 0 and 100.");
            RuleFor(x => x.StartDate).LessThanOrEqualTo(x => x.EndDate);
            RuleFor(x => x.EndDate).GreaterThanOrEqualTo(DateTime.UtcNow);
        }
    }
}
