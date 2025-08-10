using Domain.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data.Interfaces;

public interface IArchivableRepository<T>: IRepository<T>
    where T : class, IModel, IArchivable
{
    Task<bool> RestoreAsync(int id);
    Task<bool> HardDeleteAsync(int id);
}
