using Ecommerce.Entities.DTO.Cart;

using FluentValidation;

namespace Ecommerce.API.Validators
{
    public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
    {
        public UpdateCartItemRequestValidator()
        {
            RuleFor(x => x.CartItemId)
                .NotEmpty().WithMessage("CartItemId is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be at least 1.");
        }
    }
}
