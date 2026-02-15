using CMaaS.Backend.Dtos;
using CMaaS.Backend.Models;

namespace CMaaS.Backend.Services.Interfaces
{
    public interface IContentEntryService
    {

        // Creates a new content entry
        Task<ServiceResult<ContentEntry>> CreateEntryAsync(ContentEntry entry);

        // Updates an existing content entry
        Task<ServiceResult<ContentEntry>> UpdateEntryAsync(int id, ContentEntry entry);

        // Gets all content entries for a specific content type with filtering and pagination
        Task<ServiceResult<PaginatedResultDto<ContentEntry>>> GetEntriesByTypeAsync(int contentTypeId, FilterDto filter);

        // Gets a single content entry by ID
        Task<ServiceResult<ContentEntry>> GetEntryByIdAsync(int id);

        // Deletes a content entry by ID
        Task<ServiceResult<bool>> DeleteEntryAsync(int id);

        // Toggles the visibility of a content entry
        Task<ServiceResult<ContentEntry>> ToggleVisibilityAsync(int id);
    }
}
