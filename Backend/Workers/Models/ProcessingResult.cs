namespace BackgroundServices.Models;

/// <summary>
/// Result of processing operation
/// </summary>
public class ProcessingResult
{
    public DateTime Date { get; set; }
    public bool Success { get; set; }
    public int TotalProcessed { get; set; }
    public int TotalErrors { get; set; }
    public int ReceiptsProcessed { get; set; }
    public int ReceiptErrors { get; set; }
    public int ShipmentsProcessed { get; set; }
    public int ShipmentErrors { get; set; }
    public string? ErrorMessage { get; set; }

    public override string ToString()
    {
        return Success
            ? $"Success: {TotalProcessed} processed, {TotalErrors} errors " +
              $"(Receipts: {ReceiptsProcessed}/{ReceiptErrors}, Shipments: {ShipmentsProcessed}/{ShipmentErrors})"
            : $"Failed: {ErrorMessage}";
    }
}