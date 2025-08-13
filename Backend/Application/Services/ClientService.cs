using Application.Models.Client;
using Application.Services.Base;
using Domain.Models.Entities;
using Persistence.Data.Interfaces;

namespace Application.Services;

public class ClientService(IRepository<Client> clients) : ArchiveService<Client, CreateClientDto, UpdateClientDto>(clients)
{

}
