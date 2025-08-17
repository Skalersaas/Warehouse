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

public class ReceiptDocumentController(ReceiptDocumentService service) : CrudController<ReceiptDocument, CreateReceiptDocumentDto, UpdateReceiptDocumentDto, ReceiptDocumentResponseDto, SearchFilterModel>(service)
{
    private readonly static Func<IQueryable<ReceiptDocument>, IQueryable<ReceiptDocument>> DefaultIncludes =
        query => query
            .Include(s => s.Items)
            .ThenInclude(i => i.Resource)
            .Include(s => s.Items)
            .ThenInclude(i => i.Unit);

    public override async Task<IActionResult> GetAll([FromQuery] SearchModel? model = null)
    {
        var result = await service.QueryBy(model, DefaultIncludes);

        return result.Success
            ? ApiResponseFactory.Ok(result.Data.list.Select(doc => doc.ToResponseDto()), result.Count)
            : ApiResponseFactory.BadRequest(result.Message, result.Errors);
    }
    public override async Task<IActionResult> GetById(int id)
    {
        var result = await service.GetByIdAsync(id, DefaultIncludes);

        return result.Success
            ? ApiResponseFactory.Ok(result.Data.ToResponseDto())
            : ApiResponseFactory.NotFound(result.Message);
    }
    public override async Task<IActionResult> Query([FromBody] SearchFilterModel model)
    {
        var result = await service.QueryBy(model, DefaultIncludes);

        return result.Success
            ? ApiResponseFactory.Ok(result.Data.list.Select(doc => doc.ToResponseDto()), result.Count)
            : ApiResponseFactory.BadRequest(result.Message, result.Errors);
    }
}
