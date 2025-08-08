using Application.DTOs.Resource;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Utilities.DataManipulation;
using Utilities.Responses;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourcesController : ControllerBase
{
    private readonly IResourceService _resourceService;
    private readonly ILogger<ResourcesController> _logger;

    public ResourcesController(IResourceService resourceService, ILogger<ResourcesController> logger)
    {
        _resourceService = resourceService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType<ApiResponse<ResourceResponseDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateResourceDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.BadRequest("Validation failed", ModelState));
        }

        var (success, data, errorMessage) = await _resourceService.CreateAsync(dto);
        
        if (!success)
        {
            return BadRequest(ApiResponseFactory.BadRequest(errorMessage ?? "Failed to create resource"));
        }

        return Created($"/api/resources/{data!.Id}", ApiResponseFactory.Created(data));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ApiResponse<ResourceResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var (success, data, errorMessage) = await _resourceService.GetByIdAsync(id);
        
        if (!success)
        {
            return NotFound(ApiResponseFactory.NotFound(errorMessage ?? "Resource not found"));
        }

        return Ok(ApiResponseFactory.Ok(data));
    }

    [HttpGet]
    [ProducesResponseType<ApiResponse<IEnumerable<ResourceResponseDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? search = null, 
                                           [FromQuery] string? sortField = null, 
                                           [FromQuery] bool ascending = true,
                                           [FromQuery] int page = 1, 
                                           [FromQuery] int size = 10)
    {
        var searchModel = new SearchModel
        {
            SearchTerm = search,
            SortedField = sortField ?? "Name",
            IsAscending = ascending,
            Page = page,
            Size = size
        };

        var (data, totalCount) = await _resourceService.GetAllAsync(searchModel);
        return Ok(ApiResponseFactory.Ok(data, totalCount));
    }

    [HttpPut]
    [ProducesResponseType<ApiResponse<ResourceResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromBody] UpdateResourceDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.BadRequest("Validation failed", ModelState));
        }

        var (success, data, errorMessage) = await _resourceService.UpdateAsync(dto);
        
        if (!success)
        {
            return errorMessage?.Contains("not found") == true 
                ? NotFound(ApiResponseFactory.NotFound(errorMessage))
                : BadRequest(ApiResponseFactory.BadRequest(errorMessage ?? "Failed to update resource"));
        }

        return Ok(ApiResponseFactory.Ok(data));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await _resourceService.DeleteAsync(id);
        
        if (!success)
        {
            return errorMessage?.Contains("not found") == true
                ? NotFound(ApiResponseFactory.NotFound(errorMessage))
                : BadRequest(ApiResponseFactory.BadRequest(errorMessage ?? "Failed to delete resource"));
        }

        return Ok(ApiResponseFactory.Ok("Resource deleted successfully"));
    }

    [HttpPatch("{id:int}/archive")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(int id)
    {
        var (success, errorMessage) = await _resourceService.ArchiveAsync(id);
        
        if (!success)
        {
            return NotFound(ApiResponseFactory.NotFound(errorMessage ?? "Resource not found"));
        }

        return Ok(ApiResponseFactory.Ok("Resource archived successfully"));
    }

    [HttpPatch("{id:int}/unarchive")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unarchive(int id)
    {
        var (success, errorMessage) = await _resourceService.UnarchiveAsync(id);
        
        if (!success)
        {
            return NotFound(ApiResponseFactory.NotFound(errorMessage ?? "Resource not found"));
        }

        return Ok(ApiResponseFactory.Ok("Resource unarchived successfully"));
    }
}
