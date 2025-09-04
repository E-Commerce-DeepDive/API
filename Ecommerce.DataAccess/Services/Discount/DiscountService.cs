using Ecommerce.DataAccess.ApplicationContext;
using Ecommerce.Entities.DTO.Discount;
using Ecommerce.Entities.Models;
using Ecommerce.Entities.Shared.Bases;
using Ecommerce.Utilities.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.DataAccess.Services.Discount
{
    public class DiscountService : IDiscountService
    {
        private readonly EcommerceContext _context;
        private readonly ResponseHandler _responseHandler;
        private readonly ILogger<DiscountService> _logger;
        private readonly IValidator<CreateDiscountDto> _createValidator;
        private readonly IValidator<UpdateDiscountDto> _updateValidator;

        public DiscountService(
            EcommerceContext context,
            ResponseHandler responseHandler,
            ILogger<DiscountService> logger,
            IValidator<CreateDiscountDto> createValidator,
            IValidator<UpdateDiscountDto> updateValidator)
        {
            _context = context;
            _responseHandler = responseHandler;
            _logger = logger;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<Response<Guid>> CreateDiscountAsync(CreateDiscountDto dto)
        {
            if (dto == null)
                return _responseHandler.BadRequest<Guid>("Discount data is required.");

            var validation = await _createValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return _responseHandler.BadRequest<Guid>(
                    string.Join("; ", validation.Errors.Select(e => e.ErrorMessage))
                );

            try
            {
                var discountId = Guid.NewGuid();
                var discount = new Entities.Models.Discount
                {
                    Id = discountId,
                    Code = dto.Code,
                    Type = dto.Type,
                    Value = dto.Value,
                    ApplicableProductIds = dto.ApplicableProductIds ?? new(),
                    ApplicableCategoryIds = dto.ApplicableCategoryIds ?? new(),
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    IsActive = true,
                    IsDeleted = false,
                    UsageCount = 0
                };

                await _context.Discounts.AddAsync(discount);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Discount {DiscountId} created successfully.", discountId);
                return _responseHandler.Created(discountId, "Discount created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating discount.");
                return _responseHandler.InternalServerError<Guid>("An unexpected error occurred while creating discount.");
            }
        }

        public async Task<Response<IEnumerable<DiscountDetailsDto>>> GetAllDiscountsAsync(string? status = null)
        {
            try
            {
                var query = _context.Discounts.AsQueryable();

                if (!string.IsNullOrEmpty(status))
                {
                    if (status.Equals("active", StringComparison.OrdinalIgnoreCase))
                        query = query.Where(d => d.IsActive && !d.IsDeleted);
                    else if (status.Equals("expired", StringComparison.OrdinalIgnoreCase))
                        query = query.Where(d => d.EndDate < DateTime.UtcNow);
                }

                var discounts = await query
                    .Select(d => new DiscountDetailsDto
                    {
                        Id = d.Id,
                        Code = d.Code,
                        Type = d.Type,
                        Value = d.Value,
                        Status = d.IsDeleted ? "Deleted" : (d.EndDate < DateTime.UtcNow ? "Expired" : "Active"),
                        StartDate = d.StartDate,
                        EndDate = d.EndDate
                    })
                    .ToListAsync();

                if (!discounts.Any())
                    return _responseHandler.NotFound<IEnumerable<DiscountDetailsDto>>("No discounts found.");

                return _responseHandler.Success<IEnumerable<DiscountDetailsDto>>(discounts, "Discounts retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching discounts.");
                return _responseHandler.InternalServerError<IEnumerable<DiscountDetailsDto>>("An error occurred while fetching discounts.");
            }
        }

 
        public async Task<Response<DiscountDetailsDto>> GetDiscountByIdAsync(Guid id)
        {
            try
            {
                var discount = await _context.Discounts.FindAsync(id);
                if (discount == null || discount.IsDeleted)
                    return _responseHandler.NotFound<DiscountDetailsDto>("Discount not found.");

                var dto = new DiscountDetailsDto
                {
                    Id = discount.Id,
                    Code = discount.Code,
                    Type = discount.Type,
                    Value = discount.Value,
                    StartDate = discount.StartDate,
                    EndDate = discount.EndDate,
                    ApplicableProductIds = discount.ApplicableProductIds,
                    ApplicableCategoryIds = discount.ApplicableCategoryIds,
                    Status = discount.IsDeleted ? "Deleted" : (discount.EndDate < DateTime.UtcNow ? "Expired" : "Active"),
                    UsageCount = discount.UsageCount
                };

                return _responseHandler.Success(dto, "Discounts retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching discount {Id}.", id);
                return _responseHandler.InternalServerError<DiscountDetailsDto>("An error occurred while fetching discount.");
            }
        }

  
        public async Task<Response<bool>> UpdateDiscountAsync(UpdateDiscountDto dto)
        {
            if (dto == null)
                return _responseHandler.BadRequest<bool>("Discount data is required.");

            var validation = await _updateValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return _responseHandler.BadRequest<bool>(
                    string.Join("; ", validation.Errors.Select(e => e.ErrorMessage))
                );

            try
            {
                var discount = await _context.Discounts.FindAsync(dto.Id);
                if (discount == null || discount.IsDeleted)
                    return _responseHandler.NotFound<bool>("Discount not found.");

                discount.Type = dto.Type;
                discount.Value = dto.Value;
                discount.StartDate = dto.StartDate;
                discount.EndDate = dto.EndDate;
                discount.ApplicableProductIds = dto.ApplicableProductIds ?? new();
                discount.ApplicableCategoryIds = dto.ApplicableCategoryIds ?? new();

                _context.Discounts.Update(discount);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Discount {Id} updated successfully.", dto.Id);
                return _responseHandler.Success(true, "Discount updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating discount {Id}.", dto.Id);
                return _responseHandler.InternalServerError<bool>("An error occurred while updating discount.");
            }
        }

   
        public async Task<Response<bool>> DeleteDiscountAsync(Guid id)
        {
            try
            {
                var discount = await _context.Discounts.FindAsync(id);
                if (discount == null || discount.IsDeleted)
                    return _responseHandler.NotFound<bool>("Discount not found.");

                discount.IsDeleted = true;
                discount.IsActive = false;

                _context.Discounts.Update(discount);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Discount {Id} deleted successfully.", id);
                return _responseHandler.Success(true, "Discount deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting discount {Id}.", id);
                return _responseHandler.InternalServerError<bool>("An error occurred while deleting discount.");
            }
        }


        public async Task<Response<decimal>> ApplyDiscountAsync(string code, decimal cartTotal, List<Guid> productIds, List<Guid> categoryIds)
        {
            try
            {
                var discount = await _context.Discounts.FirstOrDefaultAsync(d =>
                    d.Code == code && !d.IsDeleted && d.IsActive && d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow);

                if (discount == null)
                    return _responseHandler.NotFound<decimal>("Invalid or expired discount code.");

                bool applicable = discount.ApplicableProductIds.Intersect(productIds).Any() ||
                                  discount.ApplicableCategoryIds.Intersect(categoryIds).Any();

                if (!applicable && (discount.ApplicableProductIds.Any() || discount.ApplicableCategoryIds.Any()))
                    return _responseHandler.BadRequest<decimal>("Discount not applicable to selected products.");

                decimal newTotal = discount.Type switch
                {
                    DiscountType.Percentage => cartTotal - (cartTotal * (discount.Value / 100)),
                    DiscountType.FixedAmount => cartTotal - discount.Value,
                    _ => cartTotal
                };

                if (newTotal < 0) newTotal = 0;

                discount.UsageCount++;
                _context.Discounts.Update(discount);
                await _context.SaveChangesAsync();

                return _responseHandler.Success(newTotal, "Discount applied successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying discount {Code}.", code);
                return _responseHandler.InternalServerError<decimal>("An error occurred while applying discount.");
            }
        }
    }
}
