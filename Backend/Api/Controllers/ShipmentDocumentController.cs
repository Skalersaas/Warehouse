using Api.Controllers.Base;
using Application.Models.ShipmentDocument;
using Application.Services;
using Domain.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Utilities.Responses;
namespace Api.Controllers;

public class ShipmentDocumentController(ShipmentDocumentService service) : CrudController<ShipmentDocument, CreateShipmentDocumentDto, UpdateShipmentDocumentDto, ShipmentDocumentResponseDto>(service)
{
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
