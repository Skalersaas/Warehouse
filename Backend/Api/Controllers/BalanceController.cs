using Application.Models.Balance;
using Application.Services;
using Domain.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Utilities.DataManipulation;
using Utilities.Responses;

namespace Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BalanceController(BalanceService balance) : ControllerBase
    {
        [HttpPost("query")]
        [ProducesResponseType<Result<string>>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<Result<IEnumerable<Balance>>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBalanceAsync([FromBody] SearchFilterModel query)
        {
            var result = await balance.QueryBy(query);

            if (!result.Success)
            {
                return ApiResponseFactory.BadRequest(result.Message, result.Errors);
            }

            var responseData = result.Data.list.Select(Mapper.FromDTO<BalanceResponseDto, Balance>).ToList();
            return ApiResponseFactory.Ok(responseData, result.Data.count);
        }
    }
}
