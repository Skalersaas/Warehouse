using Api.Attributes;
using Application.Interfaces;
using Domain.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Utilities.DataManipulation;
using Utilities.Responses;

namespace Api.Controllers.Base
{
    [ApiController]
    [ModelValidation]
    [Route("[controller]")]
    public abstract class CrudController<TModel, TCreate, TUpdate, TResponse, TSearchFilter>(
        IModelService<TModel, TCreate, TUpdate> service)
        : ControllerBase
        where TModel : class, IModel, new()
        where TCreate : class
        where TUpdate : class, IModel
        where TResponse : class, new()
        where TSearchFilter : SearchFilterModel, new()
    {
        protected IModelService<TModel, TCreate, TUpdate> _service = service;

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="entity">The entity data to create.</param>
        /// <returns>The created entity or error response.</returns>
        [HttpPost]
        [ProducesResponseType<Result<object>>(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Create([FromBody] TCreate entity)
        {
            var result = await _service.CreateAsync(entity);

            return result.Success
                ? ApiResponseFactory.Ok(Mapper.AutoMap<TResponse, TModel>(result.Data))
                : ApiResponseFactory.BadRequest(result.Message, result.Errors);
        }

        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <param name="id">The entity ID.</param>
        /// <returns>The entity or error response.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            return result.Success
                ? ApiResponseFactory.Ok(Mapper.AutoMap<TResponse, TModel>(result.Data))
                : ApiResponseFactory.NotFound(result.Message);
        }

        /// <summary>
        /// Gets all entities with optional filtering.
        /// </summary>
        /// <param name="model">Optional search parameters.</param>
        /// <returns>List of entities or error response.</returns>
        [HttpGet]
        [ProducesResponseType<Result<object>>(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetAll([FromQuery] SearchModel? model = null)
        {
            model ??= new SearchModel();
            var result = await _service.QueryBy(model);

            return result.Success
                ? ApiResponseFactory.Ok(result.Data.list.Select(item => Mapper.AutoMap<TResponse, TModel>(item)).ToList(), result.Data.count)
                : ApiResponseFactory.BadRequest(result.Message, result.Errors);
        }

        /// <summary>
        /// Queries entities with advanced filtering.
        /// </summary>
        /// <param name="model">The search filter parameters.</param>
        /// <returns>Filtered list of entities or error response.</returns>
        [HttpPost("query")]
        [ProducesResponseType<Result<object>>(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Query([FromBody] TSearchFilter model)
        {
            var result = await _service.QueryBy(model);

            return result.Success
                ? ApiResponseFactory.Ok(result.Data.list.Select(item => Mapper.AutoMap<TResponse, TModel>(item)).ToList(), result.Data.count)
                : ApiResponseFactory.BadRequest(result.Message, result.Errors);
        }

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity">The entity data to update.</param>
        /// <returns>The updated entity or error response.</returns>
        [HttpPut]
        [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<Result<object>>(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Update([FromBody] TUpdate entity)
        {
            var result = await _service.UpdateAsync(entity);

            return result.Success
                ? ApiResponseFactory.Ok(Mapper.AutoMap<TResponse, TModel>(result.Data))
                : ApiResponseFactory.BadRequest(result.Message, result.Errors);
        }

        /// <summary>
        /// Deletes an entity by its ID.
        /// </summary>
        /// <param name="id">The entity ID to delete.</param>
        /// <returns>Success confirmation or error response.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType<Result<object>>(StatusCodes.Status200OK)]
        [ProducesResponseType<Result<object>>(StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            return result.Success
                ? ApiResponseFactory.OkMessage(result.Message)
                : ApiResponseFactory.NotFound(result.Message);
        }
    }

    public abstract class CrudController<TModel, TCreate, TUpdate, TResponse>(
        IModelService<TModel, TCreate, TUpdate> service)
        : CrudController<TModel, TCreate, TUpdate, TResponse, SearchFilterModel>(service)
        where TModel : class, IModel, new()
        where TCreate : class
        where TUpdate : class, IModel
        where TResponse : class, new()
    {
    }
}