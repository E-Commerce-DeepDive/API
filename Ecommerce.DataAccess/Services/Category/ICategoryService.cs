using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Entities.DTO.Category;
using Ecommerce.Entities.Shared.Bases;
using Ecommerce.Entities.Models;

namespace Ecommerce.DataAccess.Services.Category
{
    public interface ICategoryService
    {
        Task<Response<Guid>> AddCategoryAsync(CreateCategoryRequest dto);
        Task<Response<List<GetCategoryResponse>>> GetAllCategoriesAsync();

        Task<Response<GetCategoryResponse>> GetCategoryByIdAsync(Guid id);
        Task<Response<GetCategoryResponse>> GetCategoryByNameAsync(string name);
        Task<Response<Ecommerce.Entities.Models.Category>> DeleteCategoryAsync(Guid id);
        Task<Response<Guid>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest dto);
    }
}
