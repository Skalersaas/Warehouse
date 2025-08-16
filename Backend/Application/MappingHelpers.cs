using Application.Models.ReceiptDocument;
using Application.Models.ReceiptItem;
using Application.Models.ShipmentDocument;
using Application.Models.ShipmentItem;
using Domain.Models.Entities;
using Utilities.DataManipulation;

namespace Application;

public static class MappingHelpers
{
    public static ReceiptDocumentResponseDto ToResponseDto(this ReceiptDocument document)
    {
        return Mapper.AutoMap<ReceiptDocumentResponseDto, ReceiptDocument>(document);
    }

    public static ReceiptItemResponseDto ToResponseDto(this ReceiptItem item)
    {
        return Mapper.AutoMap<ReceiptItemResponseDto, ReceiptItem>(item);
    }

    public static ShipmentDocumentResponseDto ToResponseDto(this ShipmentDocument document)
    {
        return Mapper.AutoMap<ShipmentDocumentResponseDto, ShipmentDocument>(document);
    }

    public static ShipmentItemResponseDto ToResponseDto(this ShipmentItem item)
    {
        return Mapper.AutoMap<ShipmentItemResponseDto, ShipmentItem>(item);
    }
}
