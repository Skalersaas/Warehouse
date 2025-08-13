using Domain.Models.Interfaces;

namespace Application.Interfaces;

public interface IArchiveService<TModel, TCreate, TUpdate, TResponse> : IModelService<TModel, TCreate, TUpdate, TResponse>
    where TModel : class, IArchivable, IModel, new()
    where TResponse : class, new()
{
    Task<bool> UnarchiveAsync(int id);
    Task<bool> ArchiveAsync(int id);
}
