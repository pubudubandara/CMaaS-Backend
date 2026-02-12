using CMaaS.Backend.Dtos;
using CMaaS.Backend.Models;

namespace CMaaS.Backend.Services.Interfaces
{
    public interface IContentTypeService
    {
        // Creates a new content type (schema)
        Task<ServiceResult<ContentType>> CreateContentTypeAsync(ContentType contentType);

        // Gets all content types for a specific tenant
        Task<ServiceResult<List<ContentType>>> GetAllContentTypesAsync();
    }
}
