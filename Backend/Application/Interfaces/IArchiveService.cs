using Domain.Models.Interfaces;

namespace Application.Interfaces;

public interface IArchiveService<TModel, TCreate, TUpdate> : IModelService<TModel, TCreate, TUpdate>
    where TModel : class, IArchivable, IModel, new()
{
    Task<bool> UnarchiveAsync(int id);
    Task<bool> ArchiveAsync(int id);
}
