using CMaaS.Backend.Data;
using CMaaS.Backend.Dtos;
using CMaaS.Backend.Models;
using CMaaS.Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMaaS.Backend.Services.Implementations
{
    public class ContentEntryService : IContentEntryService
    {
        private readonly AppDbContext _context;

        public ContentEntryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<ContentEntry>> CreateEntryAsync(ContentEntry entry)
        {
            // Validation
            if (entry == null)
            {
                return ServiceResult<ContentEntry>.Failure("ContentEntry is required.");
            }

            if (entry.Data == null)
            {
                return ServiceResult<ContentEntry>.Failure("Data is required.");
            }

            if (entry.ContentTypeId == 0)
            {
                return ServiceResult<ContentEntry>.Failure("ContentTypeId is required.");
            }

            // Verify content type exists
            var contentType = await _context.ContentTypes.FindAsync(entry.ContentTypeId);
            if (contentType == null)
            {
                return ServiceResult<ContentEntry>.Failure("Invalid ContentTypeId.");
            }

            try
            {
                _context.ContentEntries.Add(entry);
                await _context.SaveChangesAsync();

                return ServiceResult<ContentEntry>.Success(entry);
            }
            catch (Exception ex)
            {
                return ServiceResult<ContentEntry>.Failure($"Failed to create entry: {ex.Message}");
            }
        }

        public async Task<ServiceResult<PaginatedResultDto<ContentEntry>>> GetEntriesByTypeAsync(int contentTypeId, FilterDto filter)
        {
            // Validation
            if (contentTypeId <= 0)
            {
                return ServiceResult<PaginatedResultDto<ContentEntry>>.Failure("Invalid ContentTypeId.");
            }

            if (filter.Page <= 0)
            {
                return ServiceResult<PaginatedResultDto<ContentEntry>>.Failure("Page must be greater than 0.");
            }

            if (filter.PageSize <= 0)
            {
                return ServiceResult<PaginatedResultDto<ContentEntry>>.Failure("PageSize must be greater than 0.");
            }

            try
            {
                // Get all entries for the content type from database
                var allEntries = await _context.ContentEntries
                                    .Where(e => e.ContentTypeId == contentTypeId)
                                    .ToListAsync();

                // Apply search filter in memory (client-side)
                IEnumerable<ContentEntry> filteredEntries = allEntries;

                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    filteredEntries = allEntries.Where(e =>
                        e.Data.RootElement.ToString().Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase));
                }

                // Get total count after filtering
                var totalRecords = filteredEntries.Count();

                // Apply pagination
                var entries = filteredEntries
                                    .Skip((filter.Page - 1) * filter.PageSize)
                                    .Take(filter.PageSize)
                                    .ToList();

                var result = new PaginatedResultDto<ContentEntry>
                {
                    TotalRecords = totalRecords,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalRecords / filter.PageSize),
                    Data = entries
                };

                return ServiceResult<PaginatedResultDto<ContentEntry>>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PaginatedResultDto<ContentEntry>>.Failure($"Failed to retrieve entries: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ContentEntry>> GetEntryByIdAsync(int id)
        {
            if (id <= 0)
            {
                return ServiceResult<ContentEntry>.Failure("Invalid entry ID.");
            }

            try
            {
                var entry = await _context.ContentEntries.FindAsync(id);
                
                if (entry == null)
                {
                    return ServiceResult<ContentEntry>.Failure("ContentEntry not found.");
                }

                return ServiceResult<ContentEntry>.Success(entry);
            }
            catch (Exception ex)
            {
                return ServiceResult<ContentEntry>.Failure($"Failed to retrieve entry: {ex.Message}");
            }
        }
    }
}
