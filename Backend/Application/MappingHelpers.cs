using Application.Models.ReceiptDocument;
using Application.Models.ReceiptItem;
using Application.Models.ShipmentDocument;
using Application.Models.ShipmentItem;
using Domain.Models.Entities;
using Utilities.DataManipulation;

namespace Application;

public static class DocumentMappingHelpers
{
    public static ReceiptDocumentResponseDto ToResponseDto(this ReceiptDocument document)
    {
        return Mapper.FromDTO<ReceiptDocumentResponseDto, ReceiptDocument>(document, map => map
            .Map(dest => dest.Items, src => src.Items.Select(item => item.ToResponseDto()))
        );
    }

    public static ReceiptItemResponseDto ToResponseDto(this ReceiptItem item)
    {
        return Mapper.FromDTO<ReceiptItemResponseDto, ReceiptItem>(item, map => map
            .Map(dest => dest.ResourceName, src => src.Resource.Name)
            .Map(dest => dest.UnitName, src => src.Unit.Name)
        );
    }
    public static ShipmentDocumentResponseDto ToResponseDto(this ShipmentDocument document)
    {
        return Mapper.FromDTO<ShipmentDocumentResponseDto, ShipmentDocument>(document, map => map
            .Map(dest => dest.Items, src => src.Items.Select(item => item.ToResponseDto()))
        );
    }

    public static ShipmentItemResponseDto ToResponseDto(this ShipmentItem item)
    {
        return Mapper.FromDTO<ShipmentItemResponseDto, ShipmentItem>(item, map => map
            .Map(dest => dest.ResourceName, src => src.Resource.Name)
            .Map(dest => dest.UnitName, src => src.Unit.Name)
        );
    }
}
