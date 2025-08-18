using Application.Services;
using BackgroundServices.Models;
using BackgroundServices.Services;
using Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence.Data;

namespace BackgroundServices;

public class DailyBalanceWorker(IServiceProvider serviceProvider, ILogger<DailyBalanceWorker> logger) : BackgroundService
{
    private const string WORKER_NAME = "DailyBalanceWorker";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Daily Balance Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                var nextMidnight = DateTime.Today.AddDays(1);
                var delay = nextMidnight - now;

                logger.LogInformation("Next balance processing scheduled for {NextRun} (in {Delay})",
                    nextMidnight, delay);

                await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                    break;

                await ProcessDailyBalanceUpdates();
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Daily Balance Worker cancelled");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Daily Balance Worker main loop");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        logger.LogInformation("Daily Balance Worker stopped");
    }

    /// <summary>
    /// Main method that processes all document types for today's date
    /// </summary>
    private async Task ProcessDailyBalanceUpdates()
    {
        var today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, DateTimeKind.Utc);
        logger.LogInformation("Starting daily balance processing for {Date}", today);

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        var balanceService = scope.ServiceProvider.GetRequiredService<BalanceService>();
        var executionService = scope.ServiceProvider.GetRequiredService<WorkerExecutionService>();

        try
        {
            // Check if already executed for today
            if (!await executionService.TryStartExecutionAsync(WORKER_NAME, today))
            {
                logger.LogInformation("Daily balance processing already completed for {Date}", today);
                return;
            }

            using var transaction = await context.Database.BeginTransactionAsync();

            var totalProcessed = 0;
            var totalErrors = 0;

            // Process Receipt Documents
            var receiptResult = await ProcessReceiptDocuments(context, balanceService, today);
            totalProcessed += receiptResult.Processed;
            totalErrors += receiptResult.Errors;

            // Process Signed Shipment Documents
            var shipmentResult = await ProcessShipmentDocuments(context, balanceService, today);
            totalProcessed += shipmentResult.Processed;
            totalErrors += shipmentResult.Errors;

            await transaction.CommitAsync();

            var result = new ProcessingResult
            {
                Date = today,
                Success = totalErrors == 0,
                TotalProcessed = totalProcessed,
                TotalErrors = totalErrors,
                ReceiptsProcessed = receiptResult.Processed,
                ReceiptErrors = receiptResult.Errors,
                ShipmentsProcessed = shipmentResult.Processed,
                ShipmentErrors = shipmentResult.Errors
            };

            // Record execution completion
            await executionService.CompleteExecutionAsync(WORKER_NAME, today, result);

            logger.LogInformation(
                "Daily balance processing completed for {Date}. " +
                "Total processed: {Processed}, Total errors: {Errors}",
                today, totalProcessed, totalErrors);

            if (totalErrors > 0)
            {
                logger.LogWarning("Daily balance processing had {ErrorCount} errors on {Date}",
                    totalErrors, today);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Critical error during daily balance processing for {Date}", today);

            // Record failed execution
            var failedResult = new ProcessingResult
            {
                Date = today,
                Success = false,
                ErrorMessage = ex.Message
            };
            await executionService.CompleteExecutionAsync(WORKER_NAME, today, failedResult);

            throw;
        }
    }

    /// <summary>
    /// Manual trigger for processing with execution tracking
    /// </summary>
    public async Task<ProcessingResult> ProcessManuallyAsync(DateTime? date = null, bool forceReprocess = false)
    {
        var now = DateTime.Now;
        var targetDate = date ?? new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        logger.LogInformation("Manual balance processing triggered for {Date}", targetDate);

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        var balanceService = scope.ServiceProvider.GetRequiredService<BalanceService>();
        var executionService = scope.ServiceProvider.GetRequiredService<WorkerExecutionService>();

        try
        {
            // Force reset if requested
            if (forceReprocess)
            {
                await executionService.ResetExecutionAsync(WORKER_NAME, targetDate);
                logger.LogInformation("Force reprocessing enabled for {Date}", targetDate);
            }

            // Check if already executed
            if (!await executionService.TryStartExecutionAsync(WORKER_NAME, targetDate))
            {
                logger.LogWarning("Manual processing skipped - already executed for {Date}. Use forceReprocess=true to override.", targetDate);
                return new ProcessingResult
                {
                    Date = targetDate,
                    Success = false,
                    ErrorMessage = "Already processed for this date. Use forceReprocess=true to override."
                };
            }

            using var transaction = await context.Database.BeginTransactionAsync();

            var receiptResult = await ProcessReceiptDocuments(context, balanceService, targetDate);
            var shipmentResult = await ProcessShipmentDocuments(context, balanceService, targetDate);

            await transaction.CommitAsync();

            var result = new ProcessingResult
            {
                Date = targetDate,
                Success = true,
                TotalProcessed = receiptResult.Processed + shipmentResult.Processed,
                TotalErrors = receiptResult.Errors + shipmentResult.Errors,
                ReceiptsProcessed = receiptResult.Processed,
                ReceiptErrors = receiptResult.Errors,
                ShipmentsProcessed = shipmentResult.Processed,
                ShipmentErrors = shipmentResult.Errors
            };

            // Record execution completion
            await executionService.CompleteExecutionAsync(WORKER_NAME, targetDate, result);

            logger.LogInformation("Manual processing completed for {Date}: {Result}", targetDate, result);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during manual processing for {Date}", targetDate);

            var failedResult = new ProcessingResult
            {
                Date = targetDate,
                Success = false,
                ErrorMessage = ex.Message
            };

            await executionService.CompleteExecutionAsync(WORKER_NAME, targetDate, failedResult);
            return failedResult;
        }
    }

    /// <summary>
    /// Process receipt documents that became due today
    /// </summary>
    private async Task<(int Processed, int Errors)> ProcessReceiptDocuments(
        ApplicationContext context, BalanceService balanceService, DateTime today)
    {
        try
        {
            logger.LogInformation("Processing receipt documents for {Date}", today);

            var startOfDay = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, DateTimeKind.Utc);
            var endOfDay = startOfDay.AddDays(1);

            var dueReceipts = await context.ReceiptDocuments
                .Include(r => r.Items)
                .Where(r => r.Date >= startOfDay && r.Date < endOfDay)
                .ToListAsync();

            if (!dueReceipts.Any())
            {
                logger.LogInformation("No receipt documents due for processing on {Date}", today);
                return (0, 0);
            }

            var processed = 0;
            var errors = 0;

            foreach (var receipt in dueReceipts)
            {
                try
                {
                    var items = receipt.Items.Select(i => (i.ResourceId, i.UnitId, i.Quantity)).ToList();
                    var balanceResult = await balanceService.BulkUpdateAsync(items);

                    if (balanceResult.Success)
                    {
                        processed++;
                        logger.LogDebug("Applied balance for receipt document {DocumentId} dated {Date}",
                            receipt.Id, receipt.Date);
                    }
                    else
                    {
                        errors++;
                        logger.LogError("Failed to apply balance for receipt document {DocumentId}: {Error}",
                            receipt.Id, balanceResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    errors++;
                    logger.LogError(ex, "Error processing receipt document {DocumentId}", receipt.Id);
                }
            }

            logger.LogInformation("Receipt documents processing completed: {Processed} processed, {Errors} errors",
                processed, errors);

            return (processed, errors);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during receipt documents processing for {Date}", today);
            throw;
        }
    }

    /// <summary>
    /// Process signed shipment documents that became due today
    /// </summary>
    private async Task<(int Processed, int Errors)> ProcessShipmentDocuments(
        ApplicationContext context, BalanceService balanceService, DateTime today)
    {
        try
        {
            logger.LogInformation("Processing shipment documents for {Date}", today);

            var startOfDay = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, DateTimeKind.Utc);
            var endOfDay = startOfDay.AddDays(1);

            var dueShipments = await context.ShipmentDocuments
                .Include(s => s.Items)
                .Where(s => s.Date >= startOfDay && s.Date < endOfDay && s.Status == ShipmentStatus.Signed)
                .ToListAsync();

            if (!dueShipments.Any())
            {
                logger.LogInformation("No signed shipment documents due for processing on {Date}", today);
                return (0, 0);
            }

            var processed = 0;
            var errors = 0;

            foreach (var shipment in dueShipments)
            {
                try
                {
                    // For shipments, we subtract quantities (negative values)
                    var items = shipment.Items.Select(i => (i.ResourceId, i.UnitId, -i.Quantity)).ToList();
                    var balanceResult = await balanceService.BulkUpdateAsync(items);

                    if (balanceResult.Success)
                    {
                        processed++;
                        logger.LogDebug("Applied balance changes for shipment document {DocumentId} dated {Date}",
                            shipment.Id, shipment.Date);
                    }
                    else
                    {
                        errors++;
                        logger.LogError("Failed to apply balance changes for shipment document {DocumentId}: {Error}",
                            shipment.Id, balanceResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    errors++;
                    logger.LogError(ex, "Error processing shipment document {DocumentId}", shipment.Id);
                }
            }

            logger.LogInformation("Shipment documents processing completed: {Processed} processed, {Errors} errors",
                processed, errors);

            return (processed, errors);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during shipment documents processing for {Date}", today);
            throw;
        }
    }

    /// <summary>
    /// Get processing statistics for monitoring/health checks
    /// </summary>
    public async Task<ProcessingStats> GetProcessingStatsAsync(DateTime? date = null)
    {
        var now = DateTime.Now;
        var targetDate = date ?? new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        try
        {
            var startOfDay = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, 0, 0, 0, DateTimeKind.Utc);
            var endOfDay = startOfDay.AddDays(1);

            var receiptCount = await context.ReceiptDocuments
                .CountAsync(r => r.Date >= startOfDay && r.Date < endOfDay);

            var shipmentCount = await context.ShipmentDocuments
                .CountAsync(s => s.Date >= startOfDay && s.Date < endOfDay && s.Status == ShipmentStatus.Signed);

            return new ProcessingStats
            {
                Date = targetDate,
                ReceiptDocumentsCount = receiptCount,
                ShipmentDocumentsCount = shipmentCount,
                TotalDocumentsCount = receiptCount + shipmentCount
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting processing stats for {Date}", targetDate);
            throw;
        }
    }
}
