using Microsoft.AspNetCore.Mvc.Filters;
using Utilities.Responses;

namespace Api.Attributes
{
    public class ModelValidationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .Select(e => new
                    {
                        field = e.Key,
                        error = e.Value?.Errors.First().ErrorMessage
                    });

                context.Result = ApiResponseFactory.BadRequest("Invalid Data", errors);
            }
        }
    }
}