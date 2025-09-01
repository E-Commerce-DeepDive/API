using Ecommerce.Entities.DTO.Wishlist;
using FluentValidation;

namespace Ecommerce.API.Validators.WishLists
{
    public class WishlistAddItemValidator : AbstractValidator<WishlistAddItemRequest>
    {
        public WishlistAddItemValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required.");
        }
    }
}
