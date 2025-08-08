using Application.DTOs.Balance;
using Domain.Models.Entities;
using Utilities.DataManipulation;

namespace Application.Interfaces;

public interface IBalanceService
{
    Task<(IEnumerable<BalanceResponseDto> Data, int TotalCount)> GetWarehouseBalanceAsync(WarehouseFilterModel? filter = null);
    Task<bool> HasSufficientBalanceAsync(int resourceId, int unitId, decimal requiredQuantity);
    Task<decimal> GetCurrentBalanceAsync(int resourceId, int unitId);
    Task UpdateBalanceOnReceiptAsync(ReceiptDocument receipt);
    Task UpdateBalanceOnReceiptUpdateAsync(ReceiptDocument oldReceipt, ReceiptDocument newReceipt);
    Task UpdateBalanceOnReceiptDeleteAsync(ReceiptDocument receipt);
    Task UpdateBalanceOnShipmentSignAsync(ShipmentDocument shipment);
    Task UpdateBalanceOnShipmentRevokeAsync(ShipmentDocument shipment);
    Task ValidateShipmentBalanceAsync(ShipmentDocument shipment);
}
