using CMaaS.Backend.Data;
using CMaaS.Backend.Dtos;
using CMaaS.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CMaaS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 1. REGISTER USER
        [HttpPost("register-company")]
        public async Task<IActionResult> RegisterCompany(RegisterRequestDto request)
        {
            // 1. Check if email already exists (Validations)
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest("User email already exists.");
            }

            // 2. Start Transaction (Everything will only be saved if this succeeds)
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // -- Step A: Create Tenant (Company) --
                var tenant = new Tenant
                {
                    Name = request.OrganizationName,
                    PlanType = SubscriptionPlan.Free,
                    // Generate API Key right here (Random string)
                    ApiKey = Guid.NewGuid().ToString("N")
                };

                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync(); // Tenant ID is generated here

                // -- Step B: Create Admin User --
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

                // -- Step C: Everything is successful, Commit to Database --
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Company registered successfully!",
                    tenantId = tenant.Id,
                    apiKey = tenant.ApiKey // Show the API Key (tell them to save it)
                });
            }
            catch (Exception ex)
            {
                // If something goes wrong, remove all partially created data
                await transaction.RollbackAsync();
                return StatusCode(500, $"Registration failed: {ex.Message}");
            }
        }

        // 2. LOGIN USER
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            // If user doesn't exist or password is incorrect (check using Verify method)
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("Wrong email or password.");
            }

            // If credentials are valid, create token
            string token = CreateToken(user);
            return Ok(token);
        }

        // Private method to create token
        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("TenantId", user.TenantId.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("JwtSettings:SecretKey").Value!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1), // Valid for one day
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}