using Application.DTOs.Unit;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Utilities.DataManipulation;
using Utilities.Responses;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UnitsController : ControllerBase
{
    private readonly IUnitService _unitService;
    private readonly ILogger<UnitsController> _logger;

    public UnitsController(IUnitService unitService, ILogger<UnitsController> logger)
    {
        _unitService = unitService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType<ApiResponse<UnitResponseDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUnitDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.BadRequest("Validation failed", ModelState));
        }

        var (success, data, errorMessage) = await _unitService.CreateAsync(dto);
        
        if (!success)
        {
            return BadRequest(ApiResponseFactory.BadRequest(errorMessage ?? "Failed to create unit"));
        }

        return Created($"/api/units/{data!.Id}", ApiResponseFactory.Created(data));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ApiResponse<UnitResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var (success, data, errorMessage) = await _unitService.GetByIdAsync(id);
        
        if (!success)
        {
            return NotFound(ApiResponseFactory.NotFound(errorMessage ?? "Unit not found"));
        }

        return Ok(ApiResponseFactory.Ok(data));
    }

    [HttpGet]
    [ProducesResponseType<ApiResponse<IEnumerable<UnitResponseDto>>>(StatusCodes.Status200OK)]
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

        var (data, totalCount) = await _unitService.GetAllAsync(searchModel);
        return Ok(ApiResponseFactory.Ok(data, totalCount));
    }

    [HttpPut]
    [ProducesResponseType<ApiResponse<UnitResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromBody] UpdateUnitDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.BadRequest("Validation failed", ModelState));
        }

        var (success, data, errorMessage) = await _unitService.UpdateAsync(dto);
        
        if (!success)
        {
            return errorMessage?.Contains("not found") == true 
                ? NotFound(ApiResponseFactory.NotFound(errorMessage))
                : BadRequest(ApiResponseFactory.BadRequest(errorMessage ?? "Failed to update unit"));
        }

        return Ok(ApiResponseFactory.Ok(data));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await _unitService.DeleteAsync(id);
        
        if (!success)
        {
            return errorMessage?.Contains("not found") == true
                ? NotFound(ApiResponseFactory.NotFound(errorMessage))
                : BadRequest(ApiResponseFactory.BadRequest(errorMessage ?? "Failed to delete unit"));
        }

        return Ok(ApiResponseFactory.Ok("Unit deleted successfully"));
    }

    [HttpPatch("{id:int}/archive")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(int id)
    {
        var (success, errorMessage) = await _unitService.ArchiveAsync(id);
        
        if (!success)
        {
            return NotFound(ApiResponseFactory.NotFound(errorMessage ?? "Unit not found"));
        }

        return Ok(ApiResponseFactory.Ok("Unit archived successfully"));
    }

    [HttpPatch("{id:int}/unarchive")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unarchive(int id)
    {
        var (success, errorMessage) = await _unitService.UnarchiveAsync(id);
        
        if (!success)
        {
            return NotFound(ApiResponseFactory.NotFound(errorMessage ?? "Unit not found"));
        }

        return Ok(ApiResponseFactory.Ok("Unit unarchived successfully"));
    }
}
