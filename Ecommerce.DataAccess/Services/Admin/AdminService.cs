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

public async Task<Response<List<GetProductResponse>>> GetProductsByIdsAsync(List<Guid> ids)
{
    return await GetProductsAsync(p => ids.Contains(p.Id) 
                                       && p.IsActive 
                                       && !p.IsDeleted 
                                       );
}
public async Task<Response<GetProductResponse>> GetProductAsync(Expression<Func<Entities.Models.Product,bool>> predicate)
{
    var product = await _context.Products
        .Include(p => p.Images)
        .Include(p => p.Category)
        .FirstOrDefaultAsync(predicate);

    if (product == null)
    {
        _logger.LogWarning("Product not found with given predicate.");
        return _responseHandler.NotFound<GetProductResponse>("Product not found.");
    }

    var result = new GetProductResponse
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        CategoryId = product.CategoryId,
        CategoryName = product.Category?.Name,
        StockQuantity = product.StockQuantity,
        IsActive = product.IsActive,
        ImageUrls = product.Images.Select(img => img.ImageUrl).ToList()
    };

    return _responseHandler.Success(result, "Product retrieved successfully.");
}


public async Task<Response<List<GetProductResponse>>> GetProductsAsync(Expression<Func<Entities.Models.Product,bool>> predicate)
{
    var products = await _context.Products
        .Include(p => p.Images)
        .Include(p => p.Category)
        .Where(predicate)
        .ToListAsync();

    if (!products.Any())
    {
        _logger.LogWarning("No products found with given predicate.");
        return _responseHandler.NotFound<List<GetProductResponse>>("No products found.");
    }

    var result = products.Select(p => new GetProductResponse
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name,
        StockQuantity = p.StockQuantity,
        IsActive = p.IsActive,
        ImageUrls = p.Images.Select(img => img.ImageUrl).ToList()
    }).ToList();

    return _responseHandler.Success(result, "Products retrieved successfully.");
}

  
    public async Task<Response<GetProductResponse>> GetProductByIdAsync(Guid id)
    {
        return await GetProductAsync(p => p.Id == id && p.IsActive && !p.IsDeleted);
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
    public async Task<Response<string>> DeleteProductAsync(Guid productId)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p =>
                p.Id == productId  && !p.IsDeleted);
        if (product == null)
        {
            _logger.LogWarning("Delete failed: Product {ProductId} not found.");
            return _responseHandler.NotFound<string>("Product not found or you are not authorized to delete it.");
        }

        product.IsDeleted = true;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} soft-deleted ", productId);

        return _responseHandler.Success<string>(null,
            "Product deleted successfully and is no longer visible to buyers.");
    }
    
   public async Task<Response<Guid>> UpdateProductAsync(Guid productId, UpdateProductRequest dto)
{
    _logger.LogInformation("UpdateProductAsync called for ProductId={ProductId}", productId);

    try
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);

        if (product == null)
        {
            _logger.LogWarning("Product update failed: ProductId={ProductId} not found", productId);
            return _responseHandler.NotFound<Guid>("Product not found.");
        }
        
        if (dto.Price.HasValue && dto.Price.Value < 0)
            return _responseHandler.BadRequest<Guid>("Price cannot be negative.");

        if (dto.StockQuantity.HasValue && dto.StockQuantity.Value < 0)
            return _responseHandler.BadRequest<Guid>("Stock quantity cannot be negative.");
        
        if (!string.IsNullOrWhiteSpace(dto.Name) && product.Name != dto.Name.Trim())
        {
            _logger.LogInformation("ProductId={ProductId} Name changed from '{Old}' to '{New}'",
                product.Id, product.Name, dto.Name);
            product.Name = dto.Name.Trim();
        }

        if (dto.Price.HasValue && dto.Price.Value != product.Price)
        {
            _logger.LogInformation("ProductId={ProductId} Price changed from {Old} to {New}",
                product.Id, product.Price, dto.Price.Value);
            product.Price = dto.Price.Value;
        }

        if (dto.StockQuantity.HasValue && dto.StockQuantity.Value != product.StockQuantity)
        {
            _logger.LogInformation("ProductId={ProductId} StockQuantity changed from {Old} to {New}",
                product.Id, product.StockQuantity, dto.StockQuantity.Value);
            product.StockQuantity = dto.StockQuantity.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.Description) && dto.Description != product.Description)
        {
            _logger.LogInformation("ProductId={ProductId} Description updated", product.Id);
            product.Description = dto.Description.Trim();
        }

        if (dto.Images != null && dto.Images.Any())
        {
            var uploadedImages = await ReplaceProductImagesAsync(product.Id, dto.Images);
            product.Images = uploadedImages.ToList();
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("ProductId={ProductId} updated successfully", product.Id);
        return _responseHandler.Success<Guid>(product.Id, "Product updated successfully.");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "An error occurred while updating ProductId={ProductId}", productId);
        return _responseHandler.InternalServerError<Guid>("An error occurred while updating the product.");
    }
}

    
    private async Task<IList<ProductImage>> ReplaceProductImagesAsync(Guid productId, IEnumerable<IFormFile> files)
{
    await using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
        
        var existingImages = await _context.ProductImages
            .Where(i => i.ProductId == productId)
            .ToListAsync();

        var newImages = new List<ProductImage>();
        bool isFirst = true;

        foreach (var file in files)
        {
            var uploadResult = await _imageUploadService.UploadAsync(file);

            if (string.IsNullOrWhiteSpace(uploadResult))
            {
                _logger.LogError("Upload failed for image: {FileName}", file.FileName);
                throw new Exception($"Failed to upload image {file.FileName}");
            }

            newImages.Add(new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                ImageUrl = uploadResult,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsPrimary = isFirst
            });

            isFirst = false;
        }
        
        foreach (var oldImage in existingImages)
        {
            if (!string.IsNullOrWhiteSpace(oldImage.ImageUrl))
            {
                try
                {
                    await _imageUploadService.DeleteAsync(oldImage.ImageUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete image: {ImageUrl}", oldImage.ImageUrl);
                }
            }
        }

        _context.ProductImages.RemoveRange(existingImages);
        await _context.ProductImages.AddRangeAsync(newImages);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return newImages;
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Error replacing images for ProductId={ProductId}", productId);
        throw;
    }
}
    
    
}