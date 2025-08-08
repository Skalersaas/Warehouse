using Api.Attributes;
using Application.Interfaces;
using Application.Services;
using Domain.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Utilities.DataManipulation;
using Utilities.Responses;

namespace Api.Controllers.Base
{
    [ApiController]
    [ModelValidation]
    [Route("[controller]")]
    public abstract class CrudController<TModel, TCreate, TUpdate, TResponse>(IModelService<TModel, TCreate, TUpdate, TResponse> service)
        : ControllerBase
        where TModel : class, IModel, new()
        where TCreate : class
        where TUpdate : class, IModel
        where TResponse : class, new()
    {
        protected IModelService<TModel, TCreate, TUpdate, TResponse> service = service;
        [HttpPost]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status409Conflict)]
        public virtual async Task<ObjectResult> Create([FromBody] TCreate entity)
        {
            var (suceed, result) = await service.CreateAsync(entity);

            return suceed
                ? ApiResponseFactory.Ok(result)
                : ApiResponseFactory.BadRequest("Entity with such GUID exists");
        }
        [HttpGet("{int}")]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
        public virtual async Task<ObjectResult> GetById(int guid)
        {
            var (succeed, result) = await service.GetByIdAsync(guid);

            return succeed
                ? ApiResponseFactory.Ok(result)
                : ApiResponseFactory.NotFound("Entity with such GUID was not found");
        }
        [HttpGet("all")]

        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
        public virtual async Task<ObjectResult> GetAll([FromBody] SearchModel model)
        {
            var (data, fullCount) = await service.QueryBy(model);
            return ApiResponseFactory.Ok(data, fullCount);
        }
        [HttpPost("query")]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
        public virtual async Task<ObjectResult> Query([FromBody] SearchFilterModel model)
        {
            var (data, fullCount) = await service.QueryBy(model);
            return ApiResponseFactory.Ok(data, fullCount);
        }
        [HttpPut]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
        public virtual async Task<ObjectResult> Update([FromBody] TUpdate entity)
        {
            var (succeed, result) = await service.UpdateAsync(entity);

            return succeed
                ? ApiResponseFactory.Ok(result)
                : ApiResponseFactory.BadRequest("Entity with such GUID exists");
        }
        [HttpDelete]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
        public virtual async Task<ObjectResult> Delete(int id)
        {
            if (!await service.DeleteAsync(id))
                return ApiResponseFactory.NotFound("Entity with such GUID was not found");
            return ApiResponseFactory.Ok("No data");
        }
    }
}