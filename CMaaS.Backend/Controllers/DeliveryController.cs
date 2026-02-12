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
    [Authorize(Roles = "Viewer")]
    public class DeliveryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserContextService _userContext;

        public DeliveryController(AppDbContext context, IUserContextService userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        [HttpGet("{contentTypeName}")]
        public async Task<IActionResult> GetContent(string contentTypeName, [FromQuery] FilterDto filter)
        {
            // 1. Get Tenant ID from User Context (Now works for both JWT and API Key!)
            var tenantId = _userContext.GetTenantId();

            if (tenantId == null)
            {
                return Unauthorized(new { message = "Valid API Key" });
            }

            // 2. Find Content Type by friendly name
            var contentType = await _context.ContentTypes
                .FirstOrDefaultAsync(c => c.Name.ToLower() == contentTypeName.ToLower() && c.TenantId == tenantId);

            if (contentType == null)
            {
                return NotFound($"Content Type '{contentTypeName}' not found for this tenant.");
            }

            // 3. Query data with pagination and search
            var query = _context.ContentEntries
                .Where(e => e.ContentTypeId == contentType.Id)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(e => e.Data.RootElement.ToString().Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            var totalRecords = await query.CountAsync();
            var data = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(e => e.Data)
                .ToListAsync();

            return Ok(new
            {
                meta = new
                {
                    page = filter.Page,
                    pageSize = filter.PageSize,
                    total = totalRecords
                },
                data = data
            });
        }
    }
}