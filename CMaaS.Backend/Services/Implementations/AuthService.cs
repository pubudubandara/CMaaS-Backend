using CMaaS.Backend.Data;
using CMaaS.Backend.Dtos;
using CMaaS.Backend.Models;
using CMaaS.Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMaaS.Backend.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            AppDbContext context,
            IJwtTokenService jwtTokenService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ServiceResult<RegisterResponseDto>> RegisterCompanyAsync(RegisterRequestDto request)
        {
            // 1. Validate request
            if (string.IsNullOrWhiteSpace(request.OrganizationName))
            {
                return ServiceResult<RegisterResponseDto>.Failure("Organization name is required.");
            }

            if (string.IsNullOrWhiteSpace(request.AdminName))
            {
                return ServiceResult<RegisterResponseDto>.Failure("Admin name is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return ServiceResult<RegisterResponseDto>.Failure("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return ServiceResult<RegisterResponseDto>.Failure("Password is required.");
            }

            // 2. Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return ServiceResult<RegisterResponseDto>.Failure("User email already exists.");
            }

            // 3. Start Transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Step A: Create Tenant (Company)
                var tenant = new Tenant
                {
                    Name = request.OrganizationName,
                    PlanType = SubscriptionPlan.Free
                };

                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync(); // Tenant ID is generated here

                // Step B: Generate email verification token
                var verificationToken = GenerateToken();
                var tokenExpiry = DateTime.UtcNow.AddHours(24);

                // Step C: Create Admin User with verification token
                var user = new User
                {
                    FullName = request.AdminName,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    TenantId = tenant.Id,
                    Role = UserRole.Admin,
                    IsEmailVerified = false,
                    EmailVerificationToken = verificationToken,
                    EmailVerificationTokenExpiry = tokenExpiry
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Step D: Commit Transaction
                await transaction.CommitAsync();

                // Step E: Send verification email
                var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:3000";
                var verificationLink = $"{frontendUrl}/verify-email?email={Uri.EscapeDataString(request.Email)}&token={Uri.EscapeDataString(verificationToken)}";

                var emailSent = await _emailService.SendEmailVerificationAsync(
                    request.Email,
                    request.AdminName,
                    verificationToken,
                    verificationLink);

                if (!emailSent)
                {
                    _logger.LogWarning($"Failed to send verification email to {request.Email}, but user was created successfully.");
                }

                // Return success response
                var response = new RegisterResponseDto
                {
                    Message = "Company registered successfully! Please check your email to verify your account.",
                    TenantId = tenant.Id,
                    ApiKey = string.Empty
                };

                return ServiceResult<RegisterResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                await transaction.RollbackAsync();
                _logger.LogError($"Registration failed: {ex.Message}");
                return ServiceResult<RegisterResponseDto>.Failure($"Registration failed: {ex.Message}");
            }
        }

        public async Task<ServiceResult<string>> LoginAsync(UserDto request)
        {
            // 1. Validate request
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return ServiceResult<string>.Failure("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return ServiceResult<string>.Failure("Password is required.");
            }

            // 2. Find user by email (include Tenant for organization name)
            var user = await _context.Users
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            // 3. Validate user exists and password is correct
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return ServiceResult<string>.Failure("Wrong email or password.");
            }

            // 4. Check if email is verified
            if (!user.IsEmailVerified)
            {
                return ServiceResult<string>.Failure("Email not verified. Please check your email to verify your account.");
            }

            // 5. Generate JWT token with tenant name
            try
            {
                var tenantName = user.Tenant?.Name ?? "Unknown";
                string token = _jwtTokenService.GenerateToken(user, tenantName);
                return ServiceResult<string>.Success(token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Token generation failed: {ex.Message}");
                return ServiceResult<string>.Failure($"Token generation failed: {ex.Message}");
            }
        }

        public async Task<ServiceResult<string>> VerifyEmailAsync(VerifyEmailRequestDto request)
        {
            // 1. Validate request
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return ServiceResult<string>.Failure("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return ServiceResult<string>.Failure("Verification token is required.");
            }

            // 2. Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return ServiceResult<string>.Failure("User not found.");
            }

            // 3. Check if already verified
            if (user.IsEmailVerified)
            {
                return ServiceResult<string>.Failure("Email is already verified.");
            }

            // 4. Validate token and expiry
            if (user.EmailVerificationToken != request.Token)
            {
                return ServiceResult<string>.Failure("Invalid verification token.");
            }

            if (user.EmailVerificationTokenExpiry == null || user.EmailVerificationTokenExpiry < DateTime.UtcNow)
            {
                return ServiceResult<string>.Failure("Verification token has expired.");
            }

            // 5. Mark email as verified
            try
            {
                user.IsEmailVerified = true;
                user.EmailVerificationToken = null;
                user.EmailVerificationTokenExpiry = null;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);

                return ServiceResult<string>.Success("Email verified successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Email verification failed: {ex.Message}");
                return ServiceResult<string>.Failure($"Email verification failed: {ex.Message}");
            }
        }

        public async Task<ServiceResult<string>> ResendVerificationEmailAsync(ResendVerificationEmailDto request)
        {
            // 1. Validate request
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return ServiceResult<string>.Failure("Email is required.");
            }

            // 2. Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return ServiceResult<string>.Failure("User not found.");
            }

            // 3. Check if already verified
            if (user.IsEmailVerified)
            {
                return ServiceResult<string>.Failure("Email is already verified.");
            }

            // 4. Generate new verification token
            try
            {
                var verificationToken = GenerateToken();
                var tokenExpiry = DateTime.UtcNow.AddHours(24);

                user.EmailVerificationToken = verificationToken;
                user.EmailVerificationTokenExpiry = tokenExpiry;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // Send verification email
                var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:3000";
                var verificationLink = $"{frontendUrl}/verify-email?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(verificationToken)}";

                var emailSent = await _emailService.SendEmailVerificationAsync(
                    user.Email,
                    user.FullName,
                    verificationToken,
                    verificationLink);

                if (!emailSent)
                {
                    return ServiceResult<string>.Failure("Failed to send verification email. Please try again later.");
                }

                return ServiceResult<string>.Success("Verification email sent successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Resend verification email failed: {ex.Message}");
                return ServiceResult<string>.Failure($"Resend verification email failed: {ex.Message}");
            }
        }

        public async Task<ServiceResult<string>> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            // 1. Validate request
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return ServiceResult<string>.Failure("Email is required.");
            }

            // 2. Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                // Return generic message for security (don't reveal if user exists)
                return ServiceResult<string>.Success("If the email exists, a password reset link will be sent shortly.");
            }

            // 3. Generate password reset token
            try
            {
                var resetToken = GenerateToken();
                var tokenExpiry = DateTime.UtcNow.AddHours(1); // Token expires in 1 hour

                user.PasswordResetToken = resetToken;
                user.PasswordResetTokenExpiry = tokenExpiry;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // Send password reset email
                var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:3000";
                var resetLink = $"{frontendUrl}/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(resetToken)}";

                var emailSent = await _emailService.SendPasswordResetEmailAsync(
                    user.Email,
                    user.FullName,
                    resetToken,
                    resetLink);

                if (!emailSent)
                {
                    _logger.LogWarning($"Failed to send password reset email to {user.Email}, but token was generated.");
                }

                return ServiceResult<string>.Success("If the email exists, a password reset link will be sent shortly.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Forgot password failed: {ex.Message}");
                return ServiceResult<string>.Failure($"Forgot password failed: {ex.Message}");
            }
        }

        public async Task<ServiceResult<string>> ValidatePasswordResetTokenAsync(ValidatePasswordResetTokenRequestDto request)
        {
            // 1. Validate request
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return ServiceResult<string>.Failure("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return ServiceResult<string>.Failure("Reset token is required.");
            }

            // 2. Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return ServiceResult<string>.Failure("User not found.");
            }

            // 3. Validate token and expiry
            if (user.PasswordResetToken != request.Token)
            {
                return ServiceResult<string>.Failure("Invalid reset token.");
            }

            if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                return ServiceResult<string>.Failure("Reset token has expired.");
            }

            return ServiceResult<string>.Success("Token is valid.");
        }

        public async Task<ServiceResult<string>> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            // 1. Validate request
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return ServiceResult<string>.Failure("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return ServiceResult<string>.Failure("Reset token is required.");
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return ServiceResult<string>.Failure("New password is required.");
            }

            if (string.IsNullOrWhiteSpace(request.ConfirmPassword))
            {
                return ServiceResult<string>.Failure("Password confirmation is required.");
            }

            // 2. Validate password match
            if (request.NewPassword != request.ConfirmPassword)
            {
                return ServiceResult<string>.Failure("Passwords do not match.");
            }

            // 3. Validate password strength (minimum 8 characters, at least one uppercase, one lowercase, one number)
            if (request.NewPassword.Length < 8)
            {
                return ServiceResult<string>.Failure("Password must be at least 8 characters long.");
            }

            if (!request.NewPassword.Any(char.IsUpper))
            {
                return ServiceResult<string>.Failure("Password must contain at least one uppercase letter.");
            }

            if (!request.NewPassword.Any(char.IsLower))
            {
                return ServiceResult<string>.Failure("Password must contain at least one lowercase letter.");
            }

            if (!request.NewPassword.Any(char.IsDigit))
            {
                return ServiceResult<string>.Failure("Password must contain at least one digit.");
            }

            // 4. Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return ServiceResult<string>.Failure("User not found.");
            }

            // 5. Validate token and expiry
            if (user.PasswordResetToken != request.Token)
            {
                return ServiceResult<string>.Failure("Invalid reset token.");
            }

            if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                return ServiceResult<string>.Failure("Reset token has expired.");
            }

            // 6. Reset password
            try
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return ServiceResult<string>.Success("Password reset successfully! You can now login with your new password.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Password reset failed: {ex.Message}");
                return ServiceResult<string>.Failure($"Password reset failed: {ex.Message}");
            }
        }

        // Helper method to generate secure random tokens
        private string GenerateToken()
        {
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[32];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData).Replace("+", "").Replace("/", "").Replace("=", "").Substring(0, 32);
            }
        }
    }
}
