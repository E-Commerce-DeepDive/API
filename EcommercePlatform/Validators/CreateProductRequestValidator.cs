using Ecommerce.API.Validators.Products;
using Ecommerce.Entities.DTO.Product;
using Ecommerce.Utilities.Enums;
using FluentValidation;

namespace Ecommerce.API.Validators
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category must be selected.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Product description is required.")
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.");

           
            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");

            RuleFor(x => x.Images)
                    .NotNull().WithMessage("Product images are required.")
                    .Must(images => images.Count >= 1).WithMessage("At least one product image is required.")
                    .Must(images => images.Count <= 10).WithMessage("Maximum 10 images are allowed.");

            RuleForEach(x => x.Images).ChildRules(images =>
            {
                images.RuleFor(file => file)
                    .NotNull().WithMessage("Image file cannot be null.")
                    .Must(f => f.Length > 0).WithMessage("Image file cannot be empty.");

                images.RuleFor(file => file.ContentType)
                    .Must(ct => ProductValidationHelpers.IsSupportedImageType(ct))
                    .WithMessage("Only JPEG and PNG images are supported.");
                RuleFor(x => x.ShippingOption)
                    .NotEmpty().WithMessage("Shipping option is required.")
                    .Must(ProductValidationHelpers.BeValidShippingOption)
                    .WithMessage("Invalid shipping option provided. Allowed values: " 
                                 + string.Join(", ", Enum.GetNames(typeof(ShippingOptions))));
               
            });
        }

        
    }
}
