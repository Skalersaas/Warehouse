using Application.Models.Units;
using Application.Services.Base;
using Domain.Models.Entities;
using Microsoft.Extensions.Logging;
using Persistence.Data;

namespace Application.Services;

public class UnitService
    : ArchiveService<Unit, CreateUnitDto, UpdateUnitDto>
{
        public UnitService(ApplicationContext _context, ILogger<UnitService> logger) : base(_context, logger)
    {
        UniqueFieldName = "name";
    }

    protected override bool CanArchive(int id)
    {
        return !_context.ShipmentItems.Any(c => c.UnitId == id) && !_context.ReceiptItems.Any(c => c.UnitId == id);
    }
}
