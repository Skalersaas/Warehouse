using Domain.Models.Interfaces;
using Utilities.Responses;

namespace Application.Interfaces;

public interface IArchiveService<TModel, TCreate, TUpdate> : IModelService<TModel, TCreate, TUpdate>
    where TModel : class, IArchivable, IModel, new()
{
    Task<Result> UnarchiveAsync(int id);
    Task<Result> ArchiveAsync(int id);
}
