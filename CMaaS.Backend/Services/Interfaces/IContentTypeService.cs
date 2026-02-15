using CMaaS.Backend.Dtos;
using CMaaS.Backend.Models;

namespace CMaaS.Backend.Services.Interfaces
{
    public interface IContentTypeService
    {
        // Creates a new content type (schema)
        Task<ServiceResult<ContentType>> CreateContentTypeAsync(ContentType contentType);

        // Gets all content types for a specific tenant
        Task<ServiceResult<List<ContentType>>> GetContentTypesByTenantAsync(int tenantId);

        // Gets a specific content type by ID for the authenticated tenant
        Task<ServiceResult<ContentType>> GetContentTypeByIdAsync(int id);

        // Updates an existing content type
        Task<ServiceResult<ContentType>> UpdateContentTypeAsync(int id, ContentType contentType);
    }
}
