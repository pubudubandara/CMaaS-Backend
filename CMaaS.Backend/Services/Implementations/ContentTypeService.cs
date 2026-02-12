using CMaaS.Backend.Data;
using CMaaS.Backend.Dtos;
using CMaaS.Backend.Models;
using CMaaS.Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace CMaaS.Backend.Services.Implementations
{
    public class ContentTypeService : IContentTypeService
    {
        private readonly AppDbContext _context;
        private readonly IUserContextService _userContext;

        public ContentTypeService(AppDbContext context, IUserContextService userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<ServiceResult<ContentType>> CreateContentTypeAsync(ContentType contentType)
        {
            // Get tenant ID from authenticated user (NOT from request body)
            var tenantId = _userContext.GetTenantId();
            if (tenantId == null)
            {
                return ServiceResult<ContentType>.Failure("Authentication required. Please provide a valid JWT token or API key.");
            }

            // Validation
            if (contentType == null)
            {
                return ServiceResult<ContentType>.Failure("ContentType is required.");
            }

            if (string.IsNullOrWhiteSpace(contentType.Name))
            {
                return ServiceResult<ContentType>.Failure("Name is required.");
            }

            if (contentType.Schema == null)
            {
                return ServiceResult<ContentType>.Failure("Schema is required.");
            }

            // Set the tenant ID from authenticated user (security fix)
            contentType.TenantId = tenantId.Value;

            // Check if content type with same name already exists for this tenant
            var nameExists = await _context.ContentTypes
                .AnyAsync(ct => ct.Name == contentType.Name && ct.TenantId == contentType.TenantId);
            
            if (nameExists)
            {
                return ServiceResult<ContentType>.Failure($"A content type with name '{contentType.Name}' already exists for this tenant.");
            }

            try
            {
                _context.ContentTypes.Add(contentType);
                await _context.SaveChangesAsync();

                return ServiceResult<ContentType>.Success(contentType);
            }
            catch (Exception ex)
            {
                return ServiceResult<ContentType>.Failure($"Failed to create content type: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<ContentType>>> GetAllContentTypesAsync()
        {
            //find the tenent ID from the authenticated user (NOT from request body)
            var tenantId = _userContext.GetTenantId();

            if (tenantId == null)
            {
                return ServiceResult<List<ContentType>>.Failure("Authentication required.");
            }

            try
            {
                var contentTypes = await _context.ContentTypes
                    .Where(ct => ct.TenantId == tenantId.Value)
                    .AsNoTracking()
                    .ToListAsync();

                return ServiceResult<List<ContentType>>.Success(contentTypes);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ContentType>>.Failure(ex.Message);
            }
        }
    }
}
