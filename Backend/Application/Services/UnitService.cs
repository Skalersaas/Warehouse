using Application.Models.Units;
using Application.Services.Base;
using Domain.Models.Entities;
using Microsoft.Extensions.Logging;
using Persistence.Data;

namespace Application.Services;

public class UnitService(ApplicationContext repo, ILogger<UnitService> logger) 
    : ArchiveService<Unit, CreateUnitDto, UpdateUnitDto>(repo, logger)
{
}
