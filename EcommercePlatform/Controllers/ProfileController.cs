using Ecommerce.DataAccess.Services.Auth;
using Ecommerce.Entities.DTO.Account;
using Ecommerce.Entities.DTO.Account.Auth.ResetPassword;
using Ecommerce.Entities.Shared.Bases;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;
        private readonly ResponseHandler _responseHandler;

        public ProfileController(IAuthService authService, IValidator<ChangePasswordRequest> changePasswordValidator, ResponseHandler responseHandler)
        {
            _authService = authService;
            _changePasswordValidator = changePasswordValidator;
            _responseHandler = responseHandler;
        }
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var result = await _authService.GetProfileAsync(User);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDto dto)
        {
            var result = await _authService.UpdateProfileAsync(User, dto);
            return StatusCode((int)result.StatusCode, result);
        }

    }
}
