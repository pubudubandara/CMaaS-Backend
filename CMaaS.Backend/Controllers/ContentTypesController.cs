using CMaaS.Backend.Models;
using CMaaS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMaaS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContentTypesController : ControllerBase
    {
        private readonly IContentTypeService _contentTypeService;

        public ContentTypesController(IContentTypeService contentTypeService)
        {
            _contentTypeService = contentTypeService;
        }

        // Create content type
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

        // Get all content types
        [HttpGet]
        public async Task<IActionResult> GetContentTypes()
        {
            var tenantId = HttpContext.User.FindFirst("TenantId")?.Value;
            if (tenantId == null || !int.TryParse(tenantId, out int tenantIdInt))
            {
                return Unauthorized("Authentication required.");
            }

            var result = await _contentTypeService.GetContentTypesByTenantAsync(tenantIdInt);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        // Get a specific content type by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContentTypeById(int id)
        {
            var result = await _contentTypeService.GetContentTypeByIdAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        // Update a content type
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
