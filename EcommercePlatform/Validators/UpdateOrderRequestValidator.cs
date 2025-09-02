using Ecommerce.Entities.DTO.Order;
using Ecommerce.Utilities.Enums;
using FluentValidation;

namespace Ecommerce.API.Validators;

public class UpdateOrderRequestValidator : AbstractValidator<UpdateOrderRequest>
{
    public UpdateOrderRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid order status.")
            .Must(x => x == OrderStatus.Processing || x == OrderStatus.Shipped)
            .WithMessage("The new order status must be either Proccessing or Shipped");

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required.")
            .MaximumLength(500).WithMessage("Shipping address cannot exceed 500 characters.");

        RuleFor(x => x.CourierService)
            .NotEmpty().WithMessage("Courier service is required.")
            .MaximumLength(100).WithMessage("Courier service cannot exceed 100 characters.");
    }
}
