using Application.Models.Resources;
using Application.Services.Base;
using Domain.Models.Entities;
using Persistence.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ResourceService(IRepository<Resource> repo, ILogger<ResourceService> logger) 
    : ArchiveService<Resource, CreateResourceDto, UpdateResourceDto>(repo, logger)
{
}
