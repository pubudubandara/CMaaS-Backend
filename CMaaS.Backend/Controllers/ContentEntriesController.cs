using CMaaS.Backend.Dtos;
using CMaaS.Backend.Models;
using CMaaS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CMaaS.Backend.Filters;

namespace CMaaS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ContentEntriesController : ControllerBase
    {
        private readonly IContentEntryService _contentEntryService;

        public ContentEntriesController(IContentEntryService contentEntryService)
        {
            _contentEntryService = contentEntryService;
        }

        // Create a new content entry
        [HttpPost]
        public async Task<IActionResult> CreateEntry([FromBody] ContentEntry entry)
        {
            var result = await _contentEntryService.CreateEntryAsync(entry);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return CreatedAtAction(nameof(GetEntryById), new { id = result.Data!.Id }, result.Data);
        }


        // Get all content entries for a specific content type with filtering and pagination

        [HttpGet("{contentTypeId}")]
        public async Task<IActionResult> GetEntriesByType(int contentTypeId, [FromQuery] FilterDto filter)
        {
            var result = await _contentEntryService.GetEntriesByTypeAsync(contentTypeId, filter);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        // Get a single content entry by ID
        [HttpGet("entry/{id}")]
        public async Task<IActionResult> GetEntryById(int id)
        {
            var result = await _contentEntryService.GetEntryByIdAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        // DELETE: api/contententries/entry/{id}
        [HttpDelete("entry/{id}")]
        public async Task<IActionResult> DeleteEntry(int id)
        {
            var result = await _contentEntryService.DeleteEntryAsync(id);

            if (!result.IsSuccess)
            {
                if (result.ErrorMessage == "ContentEntry not found.")
                {
                    throw new KeyNotFoundException(result.ErrorMessage);
                }
                else if (result.ErrorMessage == "You can only delete your own data!")
                {
                    throw new UnauthorizedAccessException(result.ErrorMessage);
                }
                else
                {
                    throw new ArgumentException(result.ErrorMessage);
                }
            }

            return NoContent();
        }
    }
}
