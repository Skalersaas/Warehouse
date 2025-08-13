using Application.Interfaces;
using Domain.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Utilities.Responses;

namespace Api.Controllers.Base;

public class ArchiveController<TModel, TCreate, TUpdate, TResponse>(IArchiveService<TModel, TCreate, TUpdate, TResponse> service) : 
    CrudController<TModel, TCreate, TUpdate, TResponse>(service)

        where TModel : class, IArchivable, IModel, new()
        where TCreate : class
        where TUpdate : class, IModel
        where TResponse : class, new()
{
    protected IArchiveService<TModel, TCreate, TUpdate, TResponse> _service = service;
    [HttpPatch("{id}/archive")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> Archive(int id)
    {
        var result = await _service.ArchiveAsync(id);
        return result
            ? ApiResponseFactory.Ok(result)
            : ApiResponseFactory.NotFound("Entity with such Id was not found");
    }
    [HttpPatch("{id}/unarchive")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
    public async Task<ObjectResult> Unarchive(int id)
    {
        var result = await _service.UnarchiveAsync(id);
        return result
            ? ApiResponseFactory.Ok(result)
            : ApiResponseFactory.NotFound("Entity with such Id was not found");
    }
}
