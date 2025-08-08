using Application.DTOs.ReceiptDocument;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Utilities.DataManipulation;
using Utilities.Responses;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReceiptDocumentsController : ControllerBase
{
    private readonly IReceiptDocumentService _receiptService;
    private readonly ILogger<ReceiptDocumentsController> _logger;

    public ReceiptDocumentsController(IReceiptDocumentService receiptService, ILogger<ReceiptDocumentsController> logger)
    {
        _receiptService = receiptService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType<ApiResponse<ReceiptDocumentResponseDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateReceiptDocumentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.BadRequest("Validation failed", ModelState));
        }

        var (success, data, errorMessage) = await _receiptService.CreateAsync(dto);
        
        if (!success)
        {
            return BadRequest(ApiResponseFactory.BadRequest(errorMessage ?? "Failed to create receipt document"));
        }

        return Created($"/api/receiptdocuments/{data!.Id}", ApiResponseFactory.Created(data));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ApiResponse<ReceiptDocumentResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var (success, data, errorMessage) = await _receiptService.GetByIdAsync(id);
        
        if (!success)
        {
            return NotFound(ApiResponseFactory.NotFound(errorMessage ?? "Receipt document not found"));
        }

        return Ok(ApiResponseFactory.Ok(data));
    }

    [HttpGet]
    [ProducesResponseType<ApiResponse<IEnumerable<ReceiptDocumentResponseDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string[]? documentNumbers = null,
        [FromQuery] int[]? resourceIds = null,
        [FromQuery] int[]? unitIds = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortField = null,
        [FromQuery] bool ascending = true,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        var filter = new DocumentFilterModel
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            DocumentNumbers = documentNumbers?.ToList(),
            ResourceIds = resourceIds?.ToList(),
            UnitIds = unitIds?.ToList(),
            SearchTerm = search,
            SortedField = sortField ?? "Date",
            IsAscending = ascending,
            Page = page,
            Size = size
        };

        var (data, totalCount) = await _receiptService.GetAllAsync(filter);
        return Ok(ApiResponseFactory.Ok(data, totalCount));
    }

    [HttpPut]
    [ProducesResponseType<ApiResponse<ReceiptDocumentResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromBody] UpdateReceiptDocumentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.BadRequest("Validation failed", ModelState));
        }

        var (success, data, errorMessage) = await _receiptService.UpdateAsync(dto);
        
        if (!success)
        {
            return errorMessage?.Contains("not found") == true 
                ? NotFound(ApiResponseFactory.NotFound(errorMessage))
                : BadRequest(ApiResponseFactory.BadRequest(errorMessage ?? "Failed to update receipt document"));
        }

        return Ok(ApiResponseFactory.Ok(data));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await _receiptService.DeleteAsync(id);
        
        if (!success)
        {
            return errorMessage?.Contains("not found") == true
                ? NotFound(ApiResponseFactory.NotFound(errorMessage))
                : BadRequest(ApiResponseFactory.BadRequest(errorMessage ?? "Failed to delete receipt document"));
        }

        return Ok(ApiResponseFactory.Ok("Receipt document deleted successfully"));
    }
}
