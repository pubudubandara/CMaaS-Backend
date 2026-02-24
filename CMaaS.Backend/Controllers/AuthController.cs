using CMaaS.Backend.Dtos;
using CMaaS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CMaaS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Register a new company with an admin user
        [HttpPost("register-company")]
        public async Task<IActionResult> RegisterCompany(RegisterRequestDto request)
        {
            var result = await _authService.RegisterCompanyAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        // Authenticate user and get JWT token
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserDto request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(new { token = result.Data });
        }

        // Verify email with token
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailRequestDto request)
        {
            var result = await _authService.VerifyEmailAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { message = result.Data });
        }

        // Resend verification email
        [HttpPost("resend-verification-email")]
        public async Task<IActionResult> ResendVerificationEmail(ResendVerificationEmailDto request)
        {
            var result = await _authService.ResendVerificationEmailAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { message = result.Data });
        }

        // Forgot password - sends reset email
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto request)
        {
            var result = await _authService.ForgotPasswordAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { message = result.Data });
        }

        // Validate password reset token
        [HttpPost("validate-reset-token")]
        public async Task<IActionResult> ValidateResetToken(ValidatePasswordResetTokenRequestDto request)
        {
            var result = await _authService.ValidatePasswordResetTokenAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { message = result.Data });
        }

        // Reset password with token
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto request)
        {
            var result = await _authService.ResetPasswordAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { message = result.Data });
        }
    }
}