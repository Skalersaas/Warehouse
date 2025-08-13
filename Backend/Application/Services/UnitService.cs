using Application.Models.Units;
using Application.Services.Base;
using Domain.Models.Entities;
using Persistence.Data.Interfaces;

namespace Application.Services;

public class UnitService(IRepository<Unit> repo) : ArchiveService<Unit, CreateUnitDto, UpdateUnitDto>(repo)
{
}
