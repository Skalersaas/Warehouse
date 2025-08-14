using Application.Models.Units;
using Application.Services.Base;
using Domain.Models.Entities;
using Persistence.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class UnitService(IRepository<Unit> repo, ILogger<UnitService> logger) 
    : ArchiveService<Unit, CreateUnitDto, UpdateUnitDto>(repo, logger)
{
}
