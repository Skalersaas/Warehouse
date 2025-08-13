using Api.Controllers.Base;
using Application.Models.Units;
using Application.Services;
using Domain.Models.Entities;

namespace Api.Controllers;

public class UnitController(UnitService service) : ArchiveController<Unit, UnitCreateDto, UnitUpdateDto, UnitResponseDto>(service)
{
}
