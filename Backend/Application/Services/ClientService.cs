using Application.Models.Client;
using Application.Services.Base;
using Domain.Models.Entities;
using Persistence.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ClientService(IRepository<Client> clients, ILogger<ClientService> logger) 
    : ArchiveService<Client, CreateClientDto, UpdateClientDto>(clients, logger)
{

}
