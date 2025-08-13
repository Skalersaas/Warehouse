using Application.Models.ShipmentDocument;
using Application.Services.Base;
using Domain.Models.Entities;
using Persistence.Data.Interfaces;

namespace Application.Services;

public class ShipmentDocumentService(IRepository<ShipmentDocument> repo) : ModelService<ShipmentDocument, ShipmentDocumentCreateDto, ShipmentDocumentUpdateDto, ShipmentDocumentResponseDto>(repo)
{
}
