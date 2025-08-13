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
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> Sign(int id)
    {
        var (success, message) = await service.Sign(id);

        return success 
            ? ApiResponseFactory.Ok(message) 
            : ApiResponseFactory.BadRequest(message);
    }
    [HttpPatch("{id}/revoke")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> Revoke(int id)
    {
        var (success, message) = await service.Revoke(id);

        return success 
            ? ApiResponseFactory.Ok(message) 
            : ApiResponseFactory.BadRequest(message);
    }
}
