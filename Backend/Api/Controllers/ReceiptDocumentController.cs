using Api.Controllers.Base;
using Application.Models.ReceiptDocument;
using Application.Services;
using Domain.Models.Entities;

namespace Api.Controllers;

public class ReceiptDocumentController(ReceiptDocumentService service) : CrudController<ReceiptDocument, ReceiptDocumentCreateDto, ReceiptDocumentUpdateDto, ReceiptDocumentResponseDto>(service)
{
}
