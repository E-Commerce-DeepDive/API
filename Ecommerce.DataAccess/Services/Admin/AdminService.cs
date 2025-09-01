using System.Linq.Expressions;
using Ecommerce.DataAccess.ApplicationContext;
using Ecommerce.DataAccess.Services.ImageUploading;
using Ecommerce.Entities.DTO.Product;
using Ecommerce.Entities.Models;
using Ecommerce.Entities.Shared.Bases;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.DataAccess.Services.Admin;

public class AdminService:IAdminService
{
    private readonly EcommerceContext _context;
    private readonly IImageUploadService _imageUploadService;
    private readonly ResponseHandler _responseHandler;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        EcommerceContext context,
        IImageUploadService imageUploadService,
        ResponseHandler responseHandler,
        ILogger<AdminService> logger)
    {
        _context = context;
        _imageUploadService = imageUploadService;
        _responseHandler = responseHandler;
        _logger = logger;
    }
    
  public async Task<Response<Guid>> AddProductAsync(CreateProductRequest dto)
{
    if (dto == null)
    {
        _logger.LogWarning("AddProductAsync called with null request.");
        return _responseHandler.BadRequest<Guid>("Product data is required.");
    }

    var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == dto.CategoryId);
    if (category == null || category.IsDeleted)
    {
        _logger.LogWarning(
            "Invalid category. CategoryId {CategoryId} not found or deleted.", 
            dto.CategoryId);

        return _responseHandler.BadRequest<Guid>("Invalid category selected.");
    }

    var existingProduct = await _context.Products
        .Include(p => p.Images)
        .FirstOrDefaultAsync(p =>
            p.Name.ToLower().Trim() == dto.Name.ToLower().Trim() &&
            p.Description.ToLower().Trim() == dto.Description.ToLower().Trim() &&
            p.Price == dto.Price &&
            p.CategoryId == dto.CategoryId &&
            !p.IsDeleted);

    if (existingProduct != null)
    {
        existingProduct.StockQuantity += dto.StockQuantity;
        _context.Products.Update(existingProduct);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Stock updated. ProductId {ProductId}, NewQuantity {Quantity}", 
            existingProduct.Id, existingProduct.StockQuantity);

        return _responseHandler.Success<Guid>(existingProduct.Id,
            "Product already exists. Stock quantity has been updated.");
    }

    try
    {
        var productId = Guid.NewGuid();
        var product = new Entities.Models.Product
        {
            Id = productId,
            Name = dto.Name?.Trim(),
            Description = dto.Description?.Trim(),
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            StockQuantity = dto.StockQuantity,
            IsActive = true,
            IsDeleted = false,
            Images = new List<ProductImage>()
        };

        var images = await UploadImagesAsync(dto.Images, productId);
        product.Images = images.ToList();

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "New product created. ProductId {ProductId}, Name {ProductName}, CategoryId {CategoryId}", 
            productId, product.Name, product.CategoryId);

        return _responseHandler.Created<Guid>(productId,
            "Product created successfully.");
    }
    catch (DbUpdateException ex)
    {
        _logger.LogError(ex, 
            "Database error while creating product. Name {ProductName}, CategoryId {CategoryId}", 
            dto.Name, dto.CategoryId);

        return _responseHandler.InternalServerError<Guid>(
            "Database error occurred while creating the product.");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, 
            "Unexpected error while creating product. Name {ProductName}, CategoryId {CategoryId}", 
            dto.Name, dto.CategoryId);

        return _responseHandler.InternalServerError<Guid>(
            "An unexpected error occurred while creating the product.");
    }
}


    public Task<Response<GetProductResponse>> GetProductAsync(Expression<Func<Product, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Task<Response<List<GetProductResponse>>> GetProductsByIdsAsync(List<Guid> ids)
    {
        throw new NotImplementedException();
    }

    public Task<Response<List<GetProductResponse>>> GetProductsAsync(Expression<Func<Product, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Task<Response<GetProductResponse>> GetProductByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }
    
    
    private async Task<IList<ProductImage>> UploadImagesAsync(IEnumerable<IFormFile> files, Guid productId)
    {
        var images = new List<ProductImage>();
        bool isFirstImage = true;

        foreach (var file in files)
        {
            try
            {
                var imageUrl = await _imageUploadService.UploadAsync(file);

                if (string.IsNullOrEmpty(imageUrl))
                {
                    _logger.LogError("Failed to upload image {FileName} for product {ProductId}", file.FileName, productId);
                    throw new Exception("Image upload failed.");
                }

                images.Add(new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ImageUrl = imageUrl, 
                    ProductId = productId,
                    CreatedAt = DateTime.UtcNow,
                    IsPrimary = isFirstImage
                });

                isFirstImage = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while uploading image {FileName} for product {ProductId}",
                    file.FileName, productId);
                throw;
            }
        }

        return images;
    }

}