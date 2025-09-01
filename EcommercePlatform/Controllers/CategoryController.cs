using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.DataAccess.Services.Category;
using Ecommerce.Entities.DTO.Category;
using Ecommerce.Entities.Shared.Bases;

namespace Ecommerce.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ResponseHandler _responseHandler;

        public CategoryController(ICategoryService categoryService, ResponseHandler responseHandler)
        {
            _categoryService = categoryService;
            _responseHandler = responseHandler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllCategoriesAsync();
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpGet("byName/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var result = await _categoryService.GetCategoryByNameAsync(name);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost("Add/Category")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromBody] CreateCategoryRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(_responseHandler.HandleModelStateErrors(ModelState));

            var result = await _categoryService.AddCategoryAsync(dto);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPut("{id:guid}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(_responseHandler.HandleModelStateErrors(ModelState));

            var result = await _categoryService.UpdateCategoryAsync(id, dto);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpDelete("{id:guid}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            return StatusCode((int)result.StatusCode, result);
        }
    }
}
