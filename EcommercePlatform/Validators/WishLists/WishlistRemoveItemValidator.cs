using Ecommerce.Entities.DTO.Wishlist;
using FluentValidation;

namespace Ecommerce.API.Validators.WishLists
{
    public class WishlistRemoveItemValidator : AbstractValidator<WishlistRemoveItemRequest>
    {
        public WishlistRemoveItemValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required.");
        }
    }
}
