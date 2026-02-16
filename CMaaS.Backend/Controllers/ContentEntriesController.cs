using CMaaS.Backend.Dtos;
using CMaaS.Backend.Models;
using CMaaS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMaaS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContentEntriesController : ControllerBase
    {
        private readonly IContentEntryService _contentEntryService;

        public ContentEntriesController(IContentEntryService contentEntryService)
        {
            _contentEntryService = contentEntryService;
        }

        // Create content entry
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

        // Update content entry
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateEntry(int id, [FromBody] ContentEntry entry)
        {
            var result = await _contentEntryService.UpdateEntryAsync(id, entry);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        // Get all entries for content type
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

        // Get single entry by ID
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

        // Toggle visibility of a content entry
        [HttpPatch("{id}/toggle-visibility")]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            var result = await _contentEntryService.ToggleVisibilityAsync(id);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(new
            {
                message = "Visibility toggled successfully",
            });
        }
    }
}
