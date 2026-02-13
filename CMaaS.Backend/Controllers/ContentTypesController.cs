using CMaaS.Backend.Models;
using CMaaS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMaaS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ContentTypesController : ControllerBase
    {
        private readonly IContentTypeService _contentTypeService;

        public ContentTypesController(IContentTypeService contentTypeService)
        {
            _contentTypeService = contentTypeService;
        }

        // Create a new content type (schema)
        [HttpPost]
        public async Task<IActionResult> CreateContentType([FromBody] ContentType contentType)
        {
            var result = await _contentTypeService.CreateContentTypeAsync(contentType);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return CreatedAtAction(nameof(GetContentTypes), new { tenantId = result.Data!.TenantId }, result.Data);
        }

        // Get all content types (schemas) for the authenticated tenant
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetContentTypes()
        {
            var result = await _contentTypeService.GetAllContentTypesAsync();

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        // Get a single content type by ID
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetContentType(int id)
        {
            var result = await _contentTypeService.GetContentTypeByIdAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        // Update an existing content type
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContentType(int id, [FromBody] ContentType contentType)
        {
            var result = await _contentTypeService.UpdateContentTypeAsync(id, contentType);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }
    }
}
