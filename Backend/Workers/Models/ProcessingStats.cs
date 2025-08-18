
namespace BackgroundServices.Models
{

    /// <summary>
    /// Statistics for monitoring daily processing
    /// </summary>
    public class ProcessingStats
    {
        public DateTime Date { get; set; }
        public int ReceiptDocumentsCount { get; set; }
        public int ShipmentDocumentsCount { get; set; }
        public int TotalDocumentsCount { get; set; }

        public override string ToString()
        {
            return $"Date: {Date:yyyy-MM-dd}, Total: {TotalDocumentsCount} " +
                   $"(Receipts: {ReceiptDocumentsCount}, Shipments: {ShipmentDocumentsCount})";
        }
    }
}
