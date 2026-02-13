using CMaaS.Backend.Dtos;
using CMaaS.Backend.Models;

namespace CMaaS.Backend.Services.Interfaces
{
    public interface IContentTypeService
    {
        // Creates a new content type (schema)
        Task<ServiceResult<ContentType>> CreateContentTypeAsync(ContentType contentType);

        // Updates an existing content type
        Task<ServiceResult<ContentType>> UpdateContentTypeAsync(int id, ContentType contentType);

        // Gets all content types for a specific tenant
        Task<ServiceResult<List<ContentType>>> GetAllContentTypesAsync();

        // Gets a single content type by ID for the authenticated tenant
        Task<ServiceResult<ContentType>> GetContentTypeByIdAsync(int id);
    }
}
