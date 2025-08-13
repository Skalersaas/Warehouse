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
        [HttpPost]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status409Conflict)]
        public virtual async Task<ObjectResult> Create([FromBody] TCreate entity)
        {
            var (suceed, result) = await _service.CreateAsync(entity);

            return suceed
                ? ApiResponseFactory.Ok(Mapper.FromDTO<TResponse, TModel>(result))
                : ApiResponseFactory.BadRequest("Entity with such Number exists");
        }
        [HttpGet("{id}")]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
        public virtual async Task<ObjectResult> GetById(int id)
        {
            var (succeed, result) = await _service.GetByIdAsync(id);

            return succeed
                ? ApiResponseFactory.Ok(Mapper.FromDTO<TResponse, TModel>(result))
                : ApiResponseFactory.NotFound("Entity with such Id was not found");
        }
        [HttpGet]

        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
        public virtual async Task<ObjectResult> GetAll([FromBody] SearchModel model)
        {
            var (data, fullCount) = await _service.QueryBy(model);

            var responseData = data.Select(Mapper.FromDTO<TResponse, TModel>).ToList();
            return ApiResponseFactory.Ok(responseData, fullCount);
        }
        [HttpPost("query")]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status400BadRequest)]
        public virtual async Task<ObjectResult> Query([FromBody] SearchFilterModel model)
        {
            var (data, fullCount) = await _service.QueryBy(model);

            var responseData = data.Select(Mapper.FromDTO<TResponse, TModel>).ToList();
            return ApiResponseFactory.Ok(responseData, fullCount);
        }
        [HttpPut]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
        public virtual async Task<ObjectResult> Update([FromBody] TUpdate entity)
        {
            var (succeed, result) = await _service.UpdateAsync(entity);

            return succeed
                ? ApiResponseFactory.Ok(Mapper.FromDTO<TResponse, TModel>(result))
                : ApiResponseFactory.BadRequest("Entity with such GUID exists");
        }
        [HttpDelete("{id}")]
        [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status404NotFound)]
        public virtual async Task<ObjectResult> Delete(int id)
        {
            var suceed = await _service.DeleteAsync(id);

            return suceed
                ? ApiResponseFactory.NotFound("Entity with such GUID was not found")
                : ApiResponseFactory.Ok("No data");
        }
    }
}