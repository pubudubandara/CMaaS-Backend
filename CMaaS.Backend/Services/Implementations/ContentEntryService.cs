using CMaaS.Backend.Data;
using CMaaS.Backend.Dtos;
using CMaaS.Backend.Models;
using CMaaS.Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json; // For JSON operations

namespace CMaaS.Backend.Services.Implementations
{
    public class ContentEntryService : IContentEntryService
    {
        private readonly AppDbContext _context;
        private readonly IUserContextService _userContext;

        public ContentEntryService(AppDbContext context, IUserContextService userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<ServiceResult<ContentEntry>> CreateEntryAsync(ContentEntry entry)
        {
            var tenantId = _userContext.GetTenantId();
            if (tenantId == null) return ServiceResult<ContentEntry>.Failure("Authentication required.");

            // Basic Validation
            if (entry == null || entry.Data == null) return ServiceResult<ContentEntry>.Failure("Invalid data.");
            if (entry.ContentTypeId == 0) return ServiceResult<ContentEntry>.Failure("ContentTypeId is required.");

            // Verify Content Type & Tenant
            var isValidContentType = await _context.ContentTypes
                .AnyAsync(ct => ct.Id == entry.ContentTypeId && ct.TenantId == tenantId);

            if (!isValidContentType)
            {
                return ServiceResult<ContentEntry>.Failure("Invalid ContentTypeId or access denied.");
            }

            entry.TenantId = tenantId.Value;

            try
            {
                _context.ContentEntries.Add(entry);
                await _context.SaveChangesAsync();
                return ServiceResult<ContentEntry>.Success(entry);
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                return ServiceResult<ContentEntry>.Failure($"DB Error: {inner}");
            }
        }

        // --- Optimized Method ---
        public async Task<ServiceResult<PaginatedResultDto<ContentEntry>>> GetEntriesByTypeAsync(int contentTypeId, FilterDto filter)
        {
            var tenantId = _userContext.GetTenantId();
            if (tenantId == null) return ServiceResult<PaginatedResultDto<ContentEntry>>.Failure("Authentication required.");

            try
            {
                // 1. Build the query (without pulling data yet)
                var query = _context.ContentEntries
                    .Where(e => e.ContentTypeId == contentTypeId && e.TenantId == tenantId)
                    .AsNoTracking() // Faster for read-only
                    .AsQueryable();

                // 2. Search Logic (JSON search is complex, keeping it simple )
                // Note: PostgreSQL JSONB search requires separate LINQ config.
                // For now, if search term exists, filter client-side to avoid performance issues

                // 3. Get total count (for pagination)
                var totalRecords = await query.CountAsync();

                // 4. Pagination (Skip/Take from database directly)
                var entries = await query
                    .OrderByDescending(e => e.Id) // Newest first
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                // 5. Return result
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
                return ServiceResult<PaginatedResultDto<ContentEntry>>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ContentEntry>> GetEntryByIdAsync(int id)
        {
            var tenantId = _userContext.GetTenantId();
            if (tenantId == null) return ServiceResult<ContentEntry>.Failure("Authentication required.");

            var entry = await _context.ContentEntries
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId);

            if (entry == null) return ServiceResult<ContentEntry>.Failure("ContentEntry not found.");

            return ServiceResult<ContentEntry>.Success(entry);
        }

        public async Task<ServiceResult<bool>> DeleteEntryAsync(int id)
        {
            var tenantId = _userContext.GetTenantId();
            if (tenantId == null) return ServiceResult<bool>.Failure("Authentication required.");

            // Checking with TenantId prevents deletion of others' entries
            var entry = await _context.ContentEntries
                .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId);

            if (entry == null) return ServiceResult<bool>.Failure("ContentEntry not found or access denied.");

            _context.ContentEntries.Remove(entry);
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<ContentEntry>> ToggleVisibilityAsync(int id)
        {
            var tenantId = _userContext.GetTenantId();
            if (tenantId == null)
                return ServiceResult<ContentEntry>.Failure("Authentication required.");

            if (id <= 0)
                return ServiceResult<ContentEntry>.Failure("Invalid content entry ID.");

            try
            {
                // Find the entry and verify it belongs to the authenticated tenant
                var entry = await _context.ContentEntries
                    .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId);

                if (entry == null)
                    return ServiceResult<ContentEntry>.Failure("ContentEntry not found or access denied.");

                // Toggle the visibility
                entry.IsVisible = !entry.IsVisible;

                await _context.SaveChangesAsync();

                return ServiceResult<ContentEntry>.Success(entry);
            }
            catch (Exception ex)
            {
                return ServiceResult<ContentEntry>.Failure($"Failed to toggle visibility: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ContentEntry>> UpdateEntryAsync(int id, ContentEntry entry)
        {
            // Get tenant ID from authenticated user
            var tenantId = _userContext.GetTenantId();
            if (tenantId == null)
            {
                return ServiceResult<ContentEntry>.Failure("Authentication required. Please provide a valid JWT token or API key.");
            }

            // Validation
            if (entry == null)
            {
                return ServiceResult<ContentEntry>.Failure("ContentEntry is required.");
            }

            if (entry.Data == null)
            {
                return ServiceResult<ContentEntry>.Failure("Data is required.");
            }

            try
            {
                // Find existing entry and verify it belongs to the authenticated tenant
                var existingEntry = await _context.ContentEntries
                    .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId.Value);

                if (existingEntry == null)
                {
                    return ServiceResult<ContentEntry>.Failure("ContentEntry not found or access denied.");
                }

                // Update the data (do not allow changing ContentTypeId or TenantId)
                existingEntry.Data = entry.Data;

                await _context.SaveChangesAsync();

                return ServiceResult<ContentEntry>.Success(existingEntry);
            }
            catch (Exception ex)
            {
                return ServiceResult<ContentEntry>.Failure($"Failed to update entry: {ex.Message}");
            }
        }
    }
}