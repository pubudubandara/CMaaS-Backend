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

        public AuthService(AppDbContext context, IJwtTokenService jwtTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
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
                // Step A: Create Tenant (Company) - NO automatic API key
                var tenant = new Tenant
                {
                    Name = request.OrganizationName,
                    PlanType = SubscriptionPlan.Free
                    // API key generation removed - use ApiKeyService.CreateApiKeyAsync() instead
                };

                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync(); // Tenant ID is generated here

                // Step B: Create Admin User
                var user = new User
                {
                    FullName = request.AdminName,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    TenantId = tenant.Id,
                    Role = UserRole.Admin
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Step C: Commit Transaction
                await transaction.CommitAsync();

                // Return success response - NO API key returned
                var response = new RegisterResponseDto
                {
                    Message = "Company registered successfully! Use the Admin Dashboard to create API keys.",
                    TenantId = tenant.Id,
                    ApiKey = string.Empty // No API key returned during registration
                };

                return ServiceResult<RegisterResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                await transaction.RollbackAsync();
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

            // 4. Generate JWT token with tenant name
            try
            {
                var tenantName = user.Tenant?.Name ?? "Unknown";
                string token = _jwtTokenService.GenerateToken(user, tenantName);
                return ServiceResult<string>.Success(token);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Failure($"Token generation failed: {ex.Message}");
            }
        }
    }
}
