using Application.Interfaces;
using Domain.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Utilities.Responses;

namespace Api.Controllers.Base;

public class ArchiveController<TModel, TCreate, TUpdate, TResponse>(IArchiveService<TModel, TCreate, TUpdate> service) : 
    CrudController<TModel, TCreate, TUpdate, TResponse>(service)
    where TModel : class, IArchivable, IModel, new()
    where TCreate : class
    where TUpdate : class, IModel
    where TResponse : class, new()
{
    protected new IArchiveService<TModel, TCreate, TUpdate> _service = service;

    /// <summary>
    /// Archives an entity by setting its IsArchived flag to true.
    /// </summary>
    /// <param name="id">The entity ID to archive.</param>
    /// <returns>Success confirmation or error response.</returns>
    [HttpPatch("{id}/archive")]
    [ProducesResponseType<Result<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(int id)
    {
        var result = await _service.ArchiveAsync(id);

        return result.Success
            ? ApiResponseFactory.OkMessage(result.Message)
            : ApiResponseFactory.NotFound(result.Message);
    }

    /// <summary>
    /// Unarchives an entity by setting its IsArchived flag to false.
    /// </summary>
    /// <param name="id">The entity ID to unarchive.</param>
    /// <returns>Success confirmation or error response.</returns>
    [HttpPatch("{id}/unarchive")]
    [ProducesResponseType<Result<object>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unarchive(int id)
    {
        var result = await _service.UnarchiveAsync(id);
        

        return result.Success
            ? ApiResponseFactory.Ok(result.Success)
            : ApiResponseFactory.NotFound(result.Message);
    }
}
