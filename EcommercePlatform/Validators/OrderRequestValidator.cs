using Ecommerce.Entities.DTO.Order;
using FluentValidation;

namespace Ecommerce.API.Validators;

public class OrderRequestValidator : AbstractValidator<OrderRequest>
{
    public OrderRequestValidator()
    {
        
        
        RuleFor(order => order.ShippingAddress)
            .NotEmpty()
            .WithMessage("Shipping address is required.")
            .MaximumLength(500)
            .WithMessage("Shipping address cannot exceed 500 characters.");

        RuleFor(order => order.ShippingPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Shipping price must be a non-negative value.");

        RuleFor(order => order.CourierService)
            .NotEmpty()
            .WithMessage("Courier service is required.")
            .MaximumLength(100)
            .WithMessage("Courier service cannot exceed 100 characters.");

        RuleFor(order => order.TrackingNumber)
            .NotEmpty();

        RuleFor(order => order.TotalPrice)
            .GreaterThan(0)
            .WithMessage("Total price must be greater than zero.")
            .Equal(order => order.OrderItems.Sum(item => item.Subtotal) + order.ShippingPrice)
            .WithMessage("Total price must equal the sum of order items' subtotals plus shipping price.");

        RuleFor(order => order.OrderItems)
            .NotEmpty()
            .WithMessage("Order items cannot be empty.")
            .ForEach(item =>
            {
                item.SetValidator(new OrderItemRequestValidator());
            });
    }
}