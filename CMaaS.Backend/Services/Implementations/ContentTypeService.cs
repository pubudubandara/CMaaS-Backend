using CMaaS.Backend.Data;
using CMaaS.Backend.Dtos;
using CMaaS.Backend.Models;
using CMaaS.Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMaaS.Backend.Services.Implementations
{
    public class ContentTypeService : IContentTypeService
    {
        private readonly AppDbContext _context;

        public ContentTypeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<ContentType>> CreateContentTypeAsync(ContentType contentType)
        {
            // Validation
            if (contentType == null)
            {
                return ServiceResult<ContentType>.Failure("ContentType is required.");
            }

            if (contentType.TenantId == 0)
            {
                return ServiceResult<ContentType>.Failure("TenantId is required.");
            }

            if (string.IsNullOrWhiteSpace(contentType.Name))
            {
                return ServiceResult<ContentType>.Failure("Name is required.");
            }

            if (contentType.Schema == null)
            {
                return ServiceResult<ContentType>.Failure("Schema is required.");
            }

            // Check if tenant exists
            var tenantExists = await _context.Tenants.AnyAsync(t => t.Id == contentType.TenantId);
            if (!tenantExists)
            {
                return ServiceResult<ContentType>.Failure("Invalid TenantId. Tenant does not exist.");
            }

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

        public async Task<ServiceResult<List<ContentType>>> GetContentTypesByTenantAsync(int tenantId)
        {
            if (tenantId <= 0)
            {
                return ServiceResult<List<ContentType>>.Failure("Invalid TenantId.");
            }

            try
            {
                var contentTypes = await _context.ContentTypes
                    .Where(ct => ct.TenantId == tenantId)
                    .ToListAsync();

                return ServiceResult<List<ContentType>>.Success(contentTypes);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ContentType>>.Failure($"Failed to retrieve content types: {ex.Message}");
            }
        }
    }
}
