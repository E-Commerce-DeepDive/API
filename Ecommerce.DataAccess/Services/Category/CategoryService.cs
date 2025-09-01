using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ecommerce.DataAccess.ApplicationContext;
using Ecommerce.Entities.DTO.Category;
using Ecommerce.Entities.Models;
using Ecommerce.Entities.Shared.Bases;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace Ecommerce.DataAccess.Services.Category
{
    public class CategoryService : ICategoryService
    {
        private readonly EcommerceContext _context;
        private readonly ResponseHandler _responseHandler;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(EcommerceContext context, ResponseHandler responseHandler, ILogger<CategoryService> logger)
        {
            _context = context;
            _responseHandler = responseHandler;
            _logger = logger;
        }

        private async Task<GetCategoryResponse?> GetCategoryAsync(Expression<Func<Ecommerce.Entities.Models.Category, bool>> predicate)
        {
            return await _context.Categories
                .Where(predicate)
                .Where(c => !c.IsDeleted)
                .Select(c => new GetCategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Response<Guid>> AddCategoryAsync(CreateCategoryRequest dto)
        {
            _logger.LogInformation("AddCategoryAsync called with Name={CategoryName}", dto.Name);

            if (dto == null)
            {
                _logger.LogWarning("AddCategoryAsync called with null data.");
                return _responseHandler.BadRequest<Guid>("Category data is required.");
            }
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == dto.Name && c.Description == dto.Description);

            if (existingCategory != null)
            {
               
                return _responseHandler.BadRequest<Guid>("A category with the same name and description already exists.");
            }
            var category = new Ecommerce.Entities.Models.Category
            {
                Name = dto.Name,
                Description = dto.Description,
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New category created: {Name}", dto.Name);

            return _responseHandler.Created(category.Id, "Category created successfully.");
        }

        public async Task<Response<List<GetCategoryResponse>>> GetAllCategoriesAsync()
        {
            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => new GetCategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return _responseHandler.Success(categories, "Categories retrieved successfully.");
        }

        public async Task<Response<GetCategoryResponse>> GetCategoryByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogWarning("GetCategoryByIdAsync called with empty ID.");
                return _responseHandler.BadRequest<GetCategoryResponse>("Invalid category ID.");
            }

            var category = await GetCategoryAsync(c => c.Id == id);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {Id} not found.", id);
                return _responseHandler.NotFound<GetCategoryResponse>("Category not found.");
            }

            return _responseHandler.Success(category, "Category retrieved successfully.");
        }

        public async Task<Response<GetCategoryResponse>> GetCategoryByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("GetCategoryByNameAsync called with empty name.");
                return _responseHandler.BadRequest<GetCategoryResponse>("Invalid category name.");
            }

            var category = await GetCategoryAsync(c => c.Name == name);
            if (category == null)
            {
                _logger.LogWarning("Category with name {Name} not found.", name);
                return _responseHandler.NotFound<GetCategoryResponse>("Category not found.");
            }

            return _responseHandler.Success(category, "Category retrieved successfully.");
        }

        public async Task<Response<Ecommerce.Entities.Models.Category>> DeleteCategoryAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogWarning("DeleteCategoryAsync called with empty ID.");
                return _responseHandler.BadRequest<Ecommerce.Entities.Models.Category>("Invalid category ID.");
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null || category.IsDeleted)
            {
                _logger.LogWarning("DeleteCategoryAsync - Category not found or already deleted. ID: {Id}", id);
                return _responseHandler.NotFound<Ecommerce.Entities.Models.Category>("Category not found or already deleted.");
            }

            category.IsDeleted = true;
            category.UpdatedAt = DateTime.UtcNow;

            _logger.LogWarning("Category with ID {Id} is being soft deleted.", id);

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category with ID {Id} was deleted successfully.", id);

            return _responseHandler.Success(category, "Category deleted successfully.");
        }

        public async Task<Response<Guid>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest dto)
        {
            if (id == Guid.Empty || dto == null)
            {
                _logger.LogWarning("UpdateCategoryAsync called with invalid input. ID: {Id}", id);
                return _responseHandler.BadRequest<Guid>("Invalid input data.");
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null || category.IsDeleted)
            {
                _logger.LogWarning("UpdateCategoryAsync - Category not found. ID: {Id}", id);
                return _responseHandler.NotFound<Guid>("Category not found.");
            }
            
            if (category.Name == dto.Name && category.Description == dto.Description)
            {
                return _responseHandler.BadRequest<Guid>("No changes detected.");
            }

            // Check for duplication with another category (excluding current one)
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.Id != id &&
                    c.Name == dto.Name &&
                    c.Description == dto.Description &&
                    !c.IsDeleted);

            if (existingCategory != null)
            {
                return _responseHandler.BadRequest<Guid>("Another category with the same name and description already exists.");
            }
            category.Name = dto.Name;
            category.Description = dto.Description;
            category.UpdatedAt = DateTime.UtcNow;
            
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category with ID {Id} updated successfully.", id);

            return _responseHandler.Success(category.Id, "Category updated successfully.");
        }
    }
}
