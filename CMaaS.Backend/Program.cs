using CMaaS.Backend.Data;
using CMaaS.Backend.Middlewares; // Make sure this namespace exists for ApiKeyMiddleware & ExceptionMiddleware
using CMaaS.Backend.Services.Implementations;
using CMaaS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Add HttpContextAccessor (Required to access User/Token in services)
builder.Services.AddHttpContextAccessor();

// Add Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- Swagger Configuration (JWT + API Key) ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CMaaS API",
        Version = "v1",
        Description = "Content Management as a Service API"
    });

    // 1. JWT Bearer Definition (Auto-adds 'Bearer ' prefix)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http, // Changed to Http for better Bearer support
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your valid token in the text box below.\nExample: eyJhbGciOiJIUzI1NiIsIn..."
    });

    // 2. API Key Definition
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = "X-Api-Key",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Enter your API Key here"
    });

    // Apply Security Requirements
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        // Require Bearer Token
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        },
        // Require API Key
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
            },
            new string[] {}
        }
    });
});

// --- Database Connection ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- JWT Authentication Setup ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!)), 

        ValidateIssuer = false,  
        ValidateAudience = false, 

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// --- Register Custom Services ---
// Authentication Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ITenantService, TenantService>();

// Content Management Services
builder.Services.AddScoped<IContentEntryService, ContentEntryService>();
builder.Services.AddScoped<IContentTypeService, ContentTypeService>();

// API Key Management Service
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();

// Register UserContextService (This helps get TenantId easily)
builder.Services.AddScoped<IUserContextService, UserContextService>();

// --- CORS Policy ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();


// 2. Swagger (Development Only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 3. CORS (Must be before Auth)
app.UseCors("AllowAll");

// 4. Authentication (Standard JWT Check)
app.UseAuthentication();

// 5. Custom API Key Middleware (Checks API Key and sets User Context)
app.UseMiddleware<ApiKeyMiddleware>();


// 6. Authorization (Checks Roles/Permissions)
app.UseAuthorization();

// 7. Controllers
app.MapControllers();

app.Run();