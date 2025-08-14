using Api.Attributes;
using Application.Interfaces;
using Domain.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Utilities.DataManipulation;
using Utilities.Responses;

namespace Api.Controllers.Base
{
    [ApiController]
    [ModelValidation]
    [Route("[controller]")]
    public abstract class CrudController<TModel, TCreate, TUpdate, TResponse>(IModelService<TModel, TCreate, TUpdate> service)
        : ControllerBase
        where TModel : class, IModel, new()
        where TCreate : class
        where TUpdate : class, IModel
        where TResponse : class, new()
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

            if (!result.Success)
            {
                return ApiResponseFactory.BadRequest(result.Message, result.Errors);
            }

            var responseData = Mapper.FromDTO<TResponse, TModel>(result.Data);
            return ApiResponseFactory.Ok(responseData);
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

            if (!result.Success)
            {
                return ApiResponseFactory.NotFound(result.Message);
            }

            var responseData = Mapper.FromDTO<TResponse, TModel>(result.Data);
            return ApiResponseFactory.Ok(responseData);
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

            if (!result.Success)
            {
                return ApiResponseFactory.BadRequest(result.Message, result.Errors);
            }

            var responseData = result.Data.list.Select(Mapper.FromDTO<TResponse, TModel>).ToList();
            return ApiResponseFactory.Ok(responseData, result.Data.count);
        }

        /// <summary>
        /// Queries entities with advanced filtering.
        /// </summary>
        /// <param name="model">The search filter parameters.</param>
        /// <returns>Filtered list of entities or error response.</returns>
        [HttpPost("query")]
        [ProducesResponseType<Result<object>>(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Query([FromBody] SearchFilterModel model)
        {
            var result = await _service.QueryBy(model);

            if (!result.Success)
            {
                return ApiResponseFactory.BadRequest(result.Message, result.Errors);
            }

            var responseData = result.Data.list.Select(Mapper.FromDTO<TResponse, TModel>).ToList();
            return ApiResponseFactory.Ok(responseData, result.Data.count);
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

            if (!result.Success)
            {
                return ApiResponseFactory.BadRequest(result.Message, result.Errors);
            }

            var responseData = Mapper.FromDTO<TResponse, TModel>(result.Data);
            return ApiResponseFactory.Ok(responseData);
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

            if (!result.Success)
            {
                return ApiResponseFactory.NotFound(result.Message);
            }

            return ApiResponseFactory.Ok(result.Success);
        }
    }
}
