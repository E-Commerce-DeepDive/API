using Ecommerce.Entities.DTO.Discount;
using FluentValidation;

namespace Ecommerce.API.Validators.WishLists
{
    public class UpdateDiscountDtoValidator : AbstractValidator<UpdateDiscountDto>
    {
        public UpdateDiscountDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Discount Id is required.");
            RuleFor(x => x.Value).GreaterThan(0).WithMessage("Value must be greater than zero.");
            RuleFor(x => x.StartDate).LessThan(x => x.EndDate).WithMessage("Start date must be before end date.");
        }
    }
}
