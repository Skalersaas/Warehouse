using Application.Models.Resources;
using Application.Services.Base;
using Domain.Models.Entities;
using Persistence.Data.Interfaces;

namespace Application.Services;

public class ResourceService(IRepository<Resource> repo) 
    : ArchiveService<Resource, CreateResourceDto, UpdateResourceDto>(repo)
{
}
