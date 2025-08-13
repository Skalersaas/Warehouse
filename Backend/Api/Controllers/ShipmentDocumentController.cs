using Api.Controllers.Base;
using Application.Models.ShipmentDocument;
using Application.Services;
using Domain.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Utilities.Responses;
namespace Api.Controllers;

public class ShipmentDocumentController(ShipmentDocumentService service) : CrudController<ShipmentDocument, ShipmentDocumentCreateDto, ShipmentDocumentUpdateDto, ShipmentDocumentResponseDto>(service)
{
    [HttpPatch("{id}/sign")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> Archive(int id)
    {
        throw new NotImplementedException("Archive method is not implemented yet.");
    }
    [HttpPatch("{id}/revoke")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> Revoke(int id)
    {
        throw new NotImplementedException("Revoke method is not implemented yet.");
    }
}
