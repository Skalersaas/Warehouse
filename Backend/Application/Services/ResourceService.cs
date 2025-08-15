using Application.Models.Resources;
using Application.Services.Base;
using Domain.Models.Entities;
using Microsoft.Extensions.Logging;
using Persistence.Data;

namespace Application.Services;

public class ResourceService(ApplicationContext repo, ILogger<ResourceService> logger) 
    : ArchiveService<Resource, CreateResourceDto, UpdateResourceDto>(repo, logger)
{
}
