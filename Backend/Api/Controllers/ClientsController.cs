using Application.DTOs.Client;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Utilities.DataManipulation;
using Utilities.Responses;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;
    private readonly ILogger<ClientsController> _logger;

    public ClientsController(IClientService clientService, ILogger<ClientsController> logger)
    {
        _clientService = clientService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType<ApiResponse<ClientResponseDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateClientDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.BadRequest("Validation failed", ModelState));
        }

        var (success, data, errorMessage) = await _clientService.CreateAsync(dto);
        
        if (!success)
        {
            return BadRequest(ApiResponseFactory.BadRequest(errorMessage ?? "Failed to create client"));
        }

        return Created($"/api/clients/{data!.Id}", ApiResponseFactory.Created(data));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ApiResponse<ClientResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var (success, data, errorMessage) = await _clientService.GetByIdAsync(id);
        
        if (!success)
        {
            return NotFound(ApiResponseFactory.NotFound(errorMessage ?? "Client not found"));
        }

        return Ok(ApiResponseFactory.Ok(data));
    }

    [HttpGet]
    [ProducesResponseType<ApiResponse<IEnumerable<ClientResponseDto>>>(StatusCodes.Status200OK)]
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

        var (data, totalCount) = await _clientService.GetAllAsync(searchModel);
        return Ok(ApiResponseFactory.Ok(data, totalCount));
    }

    [HttpPut]
    [ProducesResponseType<ApiResponse<ClientResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromBody] UpdateClientDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.BadRequest("Validation failed", ModelState));
        }

        var (success, data, errorMessage) = await _clientService.UpdateAsync(dto);
        
        if (!success)
        {
            return errorMessage?.Contains("not found") == true 
                ? NotFound(ApiResponseFactory.NotFound(errorMessage))
                : BadRequest(ApiResponseFactory.BadRequest(errorMessage ?? "Failed to update client"));
        }

        return Ok(ApiResponseFactory.Ok(data));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await _clientService.DeleteAsync(id);
        
        if (!success)
        {
            return NotFound(ApiResponseFactory.NotFound(errorMessage ?? "Client not found"));
        }

        return Ok(ApiResponseFactory.Ok("Client deleted successfully"));
    }

    [HttpPatch("{id:int}/archive")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(int id)
    {
        var (success, errorMessage) = await _clientService.ArchiveAsync(id);
        
        if (!success)
        {
            return NotFound(ApiResponseFactory.NotFound(errorMessage ?? "Client not found"));
        }

        return Ok(ApiResponseFactory.Ok("Client archived successfully"));
    }

    [HttpPatch("{id:int}/unarchive")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unarchive(int id)
    {
        var (success, errorMessage) = await _clientService.UnarchiveAsync(id);
        
        if (!success)
        {
            return NotFound(ApiResponseFactory.NotFound(errorMessage ?? "Client not found"));
        }

        return Ok(ApiResponseFactory.Ok("Client unarchived successfully"));
    }
}
