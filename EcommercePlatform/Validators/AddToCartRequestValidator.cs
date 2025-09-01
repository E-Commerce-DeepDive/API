using Ecommerce.Entities.DTO.Cart;

using FluentValidation;

namespace Ecommerce.API.Validators
{
    public class AddToCartRequestValidator : AbstractValidator<AddToCartRequest>
    {
        public AddToCartRequestValidator() 
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("ProductId is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be at least 1.");
        }
    }
}
