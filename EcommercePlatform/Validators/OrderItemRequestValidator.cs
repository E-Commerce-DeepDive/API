using Ecommerce.Entities.DTO.Order;
using FluentValidation;

namespace Ecommerce.API.Validators;

public class OrderItemRequestValidator : AbstractValidator<OrderItemRequest>
{
    public OrderItemRequestValidator()
    {
        RuleFor(oi => oi.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required.");

        RuleFor(oi => oi.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero.");

        RuleFor(oi => oi.UnitPrice)
            .GreaterThan(0)
            .WithMessage("Unit price must be greater than zero.");

    }
}