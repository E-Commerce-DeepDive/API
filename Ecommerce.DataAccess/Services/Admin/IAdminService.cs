using System.Linq.Expressions;
using Ecommerce.Entities.DTO.Product;
using Ecommerce.Entities.Shared.Bases;

namespace Ecommerce.DataAccess.Services.Admin;

public interface IAdminService
{
    Task<Response<Guid>> AddProductAsync(CreateProductRequest dto);

    Task<Response<GetProductResponse>> GetProductAsync(Expression<Func<Entities.Models.Product, bool>> predicate);
    Task<Response<List<GetProductResponse>>> GetProductsByIdsAsync(List<Guid> ids);
    Task<Response<List<GetProductResponse>>> GetProductsAsync(
        Expression<Func<Entities.Models.Product, bool>> predicate);
    Task<Response<GetProductResponse>> GetProductByIdAsync(Guid id);
    Task<Response<string>> DeleteProductAsync(Guid productId);
    Task<Response<Guid>> UpdateProductAsync(Guid productId, UpdateProductRequest dto);

    Task<Response<List<GetProductResponse>>> GetProductsByCategoryNameAsync(string Category);
}