using Api.Controllers.Base;
using Application.Models.Resources;
using Application.Services;
using Domain.Models.Entities;

namespace Api.Controllers;

public class ResourceController(ResourceService service) 
    : ArchiveController<Resource, CreateResourceDto, UpdateResourceDto, ResourceResponseDto>(service)
{
}
