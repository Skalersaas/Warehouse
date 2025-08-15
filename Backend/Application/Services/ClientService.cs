using Application.Models.Client;
using Application.Services.Base;
using Domain.Models.Entities;
using Microsoft.Extensions.Logging;
using Persistence.Data;

namespace Application.Services;

public class ClientService(ApplicationContext context, ILogger<ClientService> logger) 
    : ArchiveService<Client, CreateClientDto, UpdateClientDto>(context, logger)
{

}
