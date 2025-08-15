using Api.Controllers.Base;
using Application.Models.ReceiptDocument;
using Application.Services;
using Domain.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Utilities.DataManipulation;
using Utilities.Responses;

namespace Api.Controllers;

public class ReceiptDocumentController(ReceiptDocumentService service) : CrudController<ReceiptDocument, CreateReceiptDocumentDto, UpdateReceiptDocumentDto, ReceiptDocumentResponseDto>(service)
{
    public override async Task<IActionResult> Query([FromBody] SearchFilterModel model)
    {
        var result = await service.QueryBy(model,
            query => query
            .Include(s => s.Items)
            .ThenInclude(i => i.Resource)
            .Include(s => s.Items)
            .ThenInclude(i => i.Unit));

        return result.Success
            ? ApiResponseFactory.Ok(result.Data, result.Count)
            : ApiResponseFactory.BadRequest(result.Message, result.Errors);
    }
}
