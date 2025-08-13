using Application.Models.ReceiptDocument;
using Application.Services.Base;
using Domain.Models.Entities;
using Persistence.Data.Interfaces;

namespace Application.Services;

public class ReceiptDocumentService(IRepository<ReceiptDocument> docs) : ModelService<ReceiptDocument, ReceiptDocumentCreateDto, ReceiptDocumentUpdateDto, ReceiptDocumentResponseDto>(docs)
{
}
