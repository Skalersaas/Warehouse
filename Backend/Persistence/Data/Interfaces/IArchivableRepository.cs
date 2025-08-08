using Domain.Models.Interfaces;

namespace Persistence.Data.Interfaces;

public interface IArchivableRepository<T> : IRepository<T>
    where T : class, IModel, IArchivable
{
    Task<bool> ArchiveAsync(int id);
    Task<bool> UnarchiveAsync(int id);
    Task<bool> HardDeleteAsync(int id);
}
