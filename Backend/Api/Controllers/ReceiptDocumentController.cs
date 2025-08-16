using Api.Controllers.Base;
using Application.Models.ReceiptDocument;
using Application.Services;
using Domain.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Utilities.DataManipulation;
using Utilities.Responses;
using Application;

namespace Api.Controllers;

public class ReceiptDocumentController(ReceiptDocumentService service) : CrudController<ReceiptDocument, CreateReceiptDocumentDto, UpdateReceiptDocumentDto, ReceiptDocumentResponseDto, SearchFilterModelDates>(service)
{
    public override async Task<IActionResult> GetById(int id)
    {
        var result = await service.GetByIdAsync(id,
            query => query
            .Include(s => s.Items)
            .ThenInclude(i => i.Resource)
            .Include(s => s.Items)
            .ThenInclude(i => i.Unit));

        return result.Success
            ? ApiResponseFactory.Ok(result.Data.ToResponseDto())
            : ApiResponseFactory.BadRequest(result.Message, result.Errors);
    }
    public override async Task<IActionResult> Query([FromBody] SearchFilterModelDates model)
    {
        var result = await service.QueryBy(model,
            query => query
            .Include(s => s.Items)
            .ThenInclude(i => i.Resource)
            .Include(s => s.Items)
            .ThenInclude(i => i.Unit));

        return result.Success
            ? ApiResponseFactory.Ok(result.Data.list.Select(doc => doc.ToResponseDto()))
            : ApiResponseFactory.BadRequest(result.Message, result.Errors);
    }
}
