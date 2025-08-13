using Api.Controllers.Base;
using Application.Models.Client;
using Application.Services;
using Domain.Models.Entities;

namespace Api.Controllers;

public class ClientController(ClientService service): 
    ArchiveController<Client, ClientCreateDto, ClientUpdateDto, ClientResponseDto>(service)
{
}
