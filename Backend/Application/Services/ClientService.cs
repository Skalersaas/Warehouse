using Application.Models.Client;
using Application.Services.Base;
using Domain.Models.Entities;
using Microsoft.Extensions.Logging;
using Persistence.Data;

namespace Application.Services;

public class ClientService
    : ArchiveService<Client, CreateClientDto, UpdateClientDto>
{
    public ClientService(ApplicationContext _context, ILogger<ClientService> logger) : base(_context, logger)
    {
        UniqueFieldName = "name";
    }

    protected override bool CanArchive(int id)
    {
        return !_context.ShipmentDocuments.Any(c => c.ClientId == id);
    }
}
