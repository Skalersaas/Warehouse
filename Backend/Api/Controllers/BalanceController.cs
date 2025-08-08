using Application.DTOs.Balance;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Utilities.DataManipulation;
using Utilities.Responses;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BalanceController : ControllerBase
{
    private readonly IBalanceService _balanceService;
    private readonly ILogger<BalanceController> _logger;

    public BalanceController(IBalanceService balanceService, ILogger<BalanceController> logger)
    {
        _balanceService = balanceService;
        _logger = logger;
    }

    [HttpGet("warehouse")]
    [ProducesResponseType<ApiResponse<IEnumerable<BalanceResponseDto>>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWarehouseBalance(
        [FromQuery] int[]? resourceIds = null,
        [FromQuery] int[]? unitIds = null,
        [FromQuery] decimal? minQuantity = null,
        [FromQuery] decimal? maxQuantity = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortField = null,
        [FromQuery] bool ascending = true,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        var filter = new WarehouseFilterModel
        {
            ResourceIds = resourceIds?.ToList(),
            UnitIds = unitIds?.ToList(),
            MinQuantity = minQuantity,
            MaxQuantity = maxQuantity,
            SearchTerm = search,
            SortedField = sortField ?? "ResourceName",
            IsAscending = ascending,
            Page = page,
            Size = size
        };

        var (data, totalCount) = await _balanceService.GetWarehouseBalanceAsync(filter);
        return Ok(ApiResponseFactory.Ok(data, totalCount));
    }

    [HttpGet("check/{resourceId:int}/{unitId:int}/{quantity:decimal}")]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckSufficientBalance(int resourceId, int unitId, decimal quantity)
    {
        var hasSufficient = await _balanceService.HasSufficientBalanceAsync(resourceId, unitId, quantity);
        return Ok(ApiResponseFactory.Ok(hasSufficient));
    }

    [HttpGet("current/{resourceId:int}/{unitId:int}")]
    [ProducesResponseType<ApiResponse<decimal>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentBalance(int resourceId, int unitId)
    {
        var balance = await _balanceService.GetCurrentBalanceAsync(resourceId, unitId);
        return Ok(ApiResponseFactory.Ok(balance));
    }
}
