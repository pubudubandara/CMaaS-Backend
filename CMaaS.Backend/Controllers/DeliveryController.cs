using CMaaS.Backend.Data;
using CMaaS.Backend.Dtos;
using CMaaS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMaaS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DeliveryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserContextService _userContext;
        private readonly IContentEntryService _contentEntryService;

        public DeliveryController(AppDbContext context, IUserContextService userContext, IContentEntryService contentEntryService)
        {
            _context = context;
            _userContext = userContext;
            _contentEntryService = contentEntryService;
        }


        // Get all content entries for a specific content type with pagination and filtering
        [HttpGet("{contentTypeName}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetContent(string contentTypeName, [FromQuery] FilterDto filter)
        {   

            // 1. Get Tenant ID from User Context (Works for both JWT and API Key!)
            var tenantId = _userContext.GetTenantId();

            if (tenantId == null)
            {
                return Unauthorized(new { message = "Valid authentication required (JWT Token or API Key)" });
            }

            // 2. Validate filter parameters
            if (filter.Page <= 0 || filter.PageSize <= 0)
            {
                return BadRequest(new { message = "Page and PageSize must be greater than 0" });
            }

            // 3. Find Content Type by friendly name
            var contentType = await _context.ContentTypes
                .FirstOrDefaultAsync(c => c.Name.ToLower() == contentTypeName.ToLower() && c.TenantId == tenantId);

            if (contentType == null)
            {
                return NotFound(new { message = $"Content Type '{contentTypeName}' not found for this tenant." });
            }

            try
            {
                // 4. Query data with pagination and search - only visible items
                var query = _context.ContentEntries
                    .Where(e => e.ContentTypeId == contentType.Id && e.TenantId == tenantId && e.IsVisible)
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    query = query.Where(e => e.Data.ToString().Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase));
                }

                var totalRecords = await query.CountAsync();
                var data = await query
                    .OrderByDescending(e => e.Id)
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(e => new
                    {
                        id = e.Id,
                        data = e.Data,
                        contentTypeId = e.ContentTypeId,
                        tenantId = e.TenantId
                    })
                    .ToListAsync();

                return Ok(new
                {
                    meta = new
                    {
                        page = filter.Page,
                        pageSize = filter.PageSize,
                        total = totalRecords,
                        totalPages = (int)Math.Ceiling((double)totalRecords / filter.PageSize)
                    },
                    data = data
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error retrieving content: {ex.Message}" });
            }
        }
        
        // Get a specific content entry by ID for a given content type
        [HttpGet("{contentTypeName}/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetContentById(string contentTypeName, int id)
        {
            // 1. Get Tenant ID from User Context (Works for both JWT and API Key!)
            var tenantId = _userContext.GetTenantId();

            if (tenantId == null)
            {
                return Unauthorized(new { message = "Valid authentication required (JWT Token or API Key)" });
            }

            // 2. Validate ID parameter
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid content entry ID" });
            }

            try
            {
                // 3. Find Content Type by friendly name
                var contentType = await _context.ContentTypes
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == contentTypeName.ToLower() && c.TenantId == tenantId);

                if (contentType == null)
                {
                    return NotFound(new { message = $"Content Type '{contentTypeName}' not found for this tenant." });
                }

                // 4. Get the specific content entry - only if visible
                var entry = await _context.ContentEntries
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == id && e.ContentTypeId == contentType.Id && e.TenantId == tenantId && e.IsVisible);

                if (entry == null)
                {
                    return NotFound(new { message = "Content entry not found or access denied" });
                }

                return Ok(new
                {
                    id = entry.Id,
                    data = entry.Data,
                    contentTypeId = entry.ContentTypeId,
                    contentTypeName = contentType.Name,
                    tenantId = entry.TenantId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error retrieving content: {ex.Message}" });
            }
        }
    }
}