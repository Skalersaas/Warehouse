using Api.Controllers.Base;
using Application;
using Application.Models.ShipmentDocument;
using Application.Services;
using Domain.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Utilities.DataManipulation;
using Utilities.Responses;
namespace Api.Controllers;

public class ShipmentDocumentController(ShipmentDocumentService service) : CrudController<ShipmentDocument, CreateShipmentDocumentDto, UpdateShipmentDocumentDto, ShipmentDocumentResponseDto, SearchFilterModelDates>(service)
{
    public override async Task<IActionResult> GetAll([FromQuery] SearchModel? model = null)
    {
        var result = await service.QueryBy(model,
            query => query
            .Include(s => s.Client)
            .Include(s => s.Items)
            .ThenInclude(i => i.Resource)
            .Include(s => s.Items)
            .ThenInclude(i => i.Unit));

        return result.Success
            ? ApiResponseFactory.Ok(result.Data.list.Select(model => model.ToResponseDto()), result.Count)
            : ApiResponseFactory.BadRequest(result.Message, result.Errors);
    }
    public override async Task<IActionResult> Query([FromBody] SearchFilterModelDates model)
    {
        var result = await service.QueryBy(model,
            query => query
            .Include(s => s.Client)
            .Include(s => s.Items)
            .ThenInclude(i => i.Resource)
            .Include(s => s.Items)
            .ThenInclude(i => i.Unit));

        return result.Success
            ? ApiResponseFactory.Ok(result.Data.list.Select(model => model.ToResponseDto()), result.Count)
            : ApiResponseFactory.BadRequest(result.Message, result.Errors);
    }

    [HttpPatch("{id}/sign")]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> Sign(int id)
    {
        var result = await service.Sign(id);

        return result.Success
            ? ApiResponseFactory.Ok(result.Message)
            : ApiResponseFactory.BadRequest(result.Message, result.Errors);
    }
    [HttpPatch("{id}/revoke")]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> Revoke(int id)
    {
        var result = await service.Revoke(id);

        return result.Success
            ? ApiResponseFactory.Ok(result.Message) 
            : ApiResponseFactory.BadRequest(result.Message, result.Errors);
    }
}
