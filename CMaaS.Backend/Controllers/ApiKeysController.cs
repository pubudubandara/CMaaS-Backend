using CMaaS.Backend.Dtos;
using CMaaS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMaaS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ApiKeysController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;

        public ApiKeysController(IApiKeyService apiKeyService)
        {
            _apiKeyService = apiKeyService;
        }

        // Create API key
        [HttpPost]
        public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequestDto request)
        {
            var result = await _apiKeyService.CreateApiKeyAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return CreatedAtAction(nameof(GetApiKeyById), new { id = result.Data!.Id }, result.Data);
        }

        // Get all API keys
        [HttpGet]
        public async Task<IActionResult> GetAllApiKeys()
        {
            var result = await _apiKeyService.GetAllApiKeysAsync();

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        // Get specific API key
        [HttpGet("{id}")]
        public async Task<IActionResult> GetApiKeyById(int id)
        {
            var result = await _apiKeyService.GetApiKeyByIdAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        // Delete API key
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApiKey(int id)
        {
            var result = await _apiKeyService.DeleteApiKeyAsync(id);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return NoContent();
        }
    }
}
