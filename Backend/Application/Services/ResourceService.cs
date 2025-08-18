using Application.Models.Resources;
using Application.Services.Base;
using Domain.Models.Entities;
using Microsoft.Extensions.Logging;
using Persistence.Data;

namespace Application.Services;

public class ResourceService 
    : ArchiveService<Resource, CreateResourceDto, UpdateResourceDto>
{
    public ResourceService(ApplicationContext _context, ILogger<ResourceService> logger) : base(_context, logger)
    {
        UniqueFieldName = "name";
    }
    protected override bool CanArchive(int id)
    {
        return !_context.ShipmentItems.Any(c => c.ResourceId == id) && !_context.ReceiptItems.Any(c => c.ResourceId == id);
    }
}
