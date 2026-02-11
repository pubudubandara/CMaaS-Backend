using CMaaS.Backend.Data;
using CMaaS.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMaaS.Backend.Dtos;



namespace CMaaS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentEntriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ContentEntriesController(AppDbContext context)
        {
            _context = context;
        }

        //Create Content Entry (POST: api/contententries)
        [HttpPost]
        public async Task<IActionResult> CreateEntry([FromBody] ContentEntry entry)
        {
            //validation
            if (entry == null)
            {
                return BadRequest("ContentEntry is required.");
            }

            if (entry.Data == null)
            {
                return BadRequest("Data is required.");
            }

            var contentType = await _context.ContentTypes.FindAsync(entry.ContentTypeId);
            if (contentType == null)
            {
                return BadRequest("Invalid ContentTypeId.");
            }
            _context.ContentEntries.Add(entry);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEntryById), new { id = entry.Id }, entry);
        }

        //Get Entries by Type (GET: api/contententries/{contentTypeId})
        [HttpGet("{contentTypeId}")]
        public async Task<IActionResult> GetEntriesByType(int contentTypeId, [FromQuery] FilterDto filter)
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

            var response = new
            {
                TotalRecords = totalRecords,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / filter.PageSize),
                Data = entries
            };

            return Ok(response);
        }

        // 3. Get Single Entry (GET: api/contententries/entry/{id})
        [HttpGet("entry/{id}")]
        public async Task<IActionResult> GetEntryById(int id)
        {
            var entry = await _context.ContentEntries.FindAsync(id);
            if (entry == null) return NotFound();
            return Ok(entry);
        }
    }
}
