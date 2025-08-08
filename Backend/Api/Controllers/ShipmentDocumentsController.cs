using Application.DTOs.ShipmentDocument;
using Application.Interfaces;
using Domain.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Utilities.DataManipulation;
using Utilities.Responses;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipmentDocumentsController : ControllerBase
{
    private readonly IShipmentDocumentService _shipmentService;
    private readonly ILogger<ShipmentDocumentsController> _logger;

    public ShipmentDocumentsController(IShipmentDocumentService shipmentService, ILogger<ShipmentDocumentsController> logger)
    {
        _shipmentService = shipmentService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType<ApiResponse<ShipmentDocumentResponseDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateShipmentDocumentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.BadRequest("Validation failed", ModelState));
        }

        var (success, data, errorMessage) = await _shipmentService.CreateAsync(dto);
        
        if (!success)
        {
            return BadRequest(ApiResponseFactory.BadRequest(errorMessage ?? "Failed to create shipment document"));
        }

        return Created($"/api/shipmentdocuments/{data!.Id}", ApiResponseFactory.Created(data));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ApiResponse<ShipmentDocumentResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var (success, data, errorMessage) = await _shipmentService.GetByIdAsync(id);
        
        if (!success)
        {
            return NotFound(ApiResponseFactory.NotFound(errorMessage ?? "Shipment document not found"));
        }

        return Ok(ApiResponseFactory.Ok(data));
    }

    [HttpGet]
    [ProducesResponseType<ApiResponse<IEnumerable<ShipmentDocumentResponseDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string[]? documentNumbers = null,
        [FromQuery] int[]? clientIds = null,
        [FromQuery] ShipmentStatus[]? statuses = null,
        [FromQuery] int[]? resourceIds = null,
        [FromQuery] int[]? unitIds = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortField = null,
        [FromQuery] bool ascending = true,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        var filter = new ShipmentFilterModel
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            DocumentNumbers = documentNumbers?.ToList(),
            ClientIds = clientIds?.ToList(),
            Statuses = statuses?.ToList(),
            ResourceIds = resourceIds?.ToList(),
            UnitIds = unitIds?.ToList(),
            SearchTerm = search,
            SortedField = sortField ?? "Date",
            IsAscending = ascending,
            Page = page,
            Size = size
        };

        var (data, totalCount) = await _shipmentService.GetAllAsync(filter);
        return Ok(ApiResponseFactory.Ok(data, totalCount));
    }

    [HttpPut]
    [ProducesResponseType<ApiResponse<ShipmentDocumentResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromBody] UpdateShipmentDocumentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.BadRequest("Validation failed", ModelState));
        }

        var (success, data, errorMessage) = await _shipmentService.UpdateAsync(dto);
        
        if (!success)
        {
            return errorMessage?.Contains("not found") == true 
                ? NotFound(ApiResponseFactory.NotFound(errorMessage))
                : BadRequest(ApiResponseFactory.BadRequest(errorMessage ?? "Failed to update shipment document"));
        }

        return Ok(ApiResponseFactory.Ok(data));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await _shipmentService.DeleteAsync(id);
        
        if (!success)
        {
            return errorMessage?.Contains("not found") == true
                ? NotFound(ApiResponseFactory.NotFound(errorMessage))
                : BadRequest(ApiResponseFactory.BadRequest(errorMessage ?? "Failed to delete shipment document"));
        }

        return Ok(ApiResponseFactory.Ok("Shipment document deleted successfully"));
    }

    [HttpPatch("{id:int}/sign")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Sign(int id)
    {
        var (success, errorMessage) = await _shipmentService.SignAsync(id);
        
        if (!success)
        {
            return errorMessage?.Contains("not found") == true
                ? NotFound(ApiResponseFactory.NotFound(errorMessage))
                : BadRequest(ApiResponseFactory.BadRequest(errorMessage ?? "Failed to sign shipment document"));
        }

        return Ok(ApiResponseFactory.Ok("Shipment document signed successfully"));
    }

    [HttpPatch("{id:int}/revoke")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke(int id)
    {
        var (success, errorMessage) = await _shipmentService.RevokeAsync(id);
        
        if (!success)
        {
            return errorMessage?.Contains("not found") == true
                ? NotFound(ApiResponseFactory.NotFound(errorMessage))
                : BadRequest(ApiResponseFactory.BadRequest(errorMessage ?? "Failed to revoke shipment document"));
        }

        return Ok(ApiResponseFactory.Ok("Shipment document revoked successfully"));
    }
}
