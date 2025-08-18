using BackgroundServices.Models;
using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Data;

namespace BackgroundServices.Services;

public class WorkerExecutionService(ApplicationContext context, ILogger<WorkerExecutionService> logger)
{

    /// <summary>
    /// Check if worker has already been executed for the given date
    /// </summary>
    public async Task<bool> IsExecutedForDateAsync(string workerName, DateTime date)
    {
        var startOfDay = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        var endOfDay = startOfDay.AddDays(1);

        var execution = await context.WorkerExecutions
            .FirstOrDefaultAsync(w => w.WorkerName == workerName && 
                                    w.ExecutionDate >= startOfDay && 
                                    w.ExecutionDate < endOfDay);

        return execution != null;
    }

    /// <summary>
    /// Record worker execution start
    /// </summary>
    public async Task<bool> TryStartExecutionAsync(string workerName, DateTime date)
    {
        var executionDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        var startOfDay = executionDate;
        var endOfDay = startOfDay.AddDays(1);

        // Check if already executed
        var existing = await context.WorkerExecutions
            .FirstOrDefaultAsync(w => w.WorkerName == workerName && 
                                    w.ExecutionDate >= startOfDay && 
                                    w.ExecutionDate < endOfDay);

        if (existing != null)
        {
            logger.LogWarning("Worker {WorkerName} already executed for date {Date}", workerName, executionDate);
            return false;
        }

        // Create execution record
        var execution = new WorkerExecution
        {
            WorkerName = workerName,
            ExecutionDate = executionDate,
            LastExecutedAt = DateTime.UtcNow,
            DocumentsProcessed = 0,
            ErrorsCount = 0,
            IsSuccess = false
        };

        context.WorkerExecutions.Add(execution);
        await context.SaveChangesAsync();

        logger.LogInformation("Started execution for worker {WorkerName} on date {Date}", workerName, executionDate);
        return true;
    }

    /// <summary>
    /// Update worker execution result
    /// </summary>
    public async Task CompleteExecutionAsync(string workerName, DateTime date, ProcessingResult result)
    {
        var startOfDay = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        var endOfDay = startOfDay.AddDays(1);

        var execution = await context.WorkerExecutions
            .FirstOrDefaultAsync(w => w.WorkerName == workerName && 
                                    w.ExecutionDate >= startOfDay && 
                                    w.ExecutionDate < endOfDay);

        if (execution == null)
        {
            logger.LogError("Execution record not found for worker {WorkerName} on date {Date}", workerName, startOfDay);
            return;
        }

        execution.DocumentsProcessed = result.TotalProcessed;
        execution.ErrorsCount = result.TotalErrors;
        execution.IsSuccess = result.Success;
        execution.ErrorMessage = result.ErrorMessage;
        execution.LastExecutedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        logger.LogInformation("Completed execution for worker {WorkerName} on date {Date}: {Result}",
            workerName, startOfDay, result);
    }

    /// <summary>
    /// Get execution history for monitoring
    /// </summary>
    public async Task<List<WorkerExecution>> GetExecutionHistoryAsync(string? workerName = null, int days = 30)
    {
        var query = context.WorkerExecutions.AsQueryable();

        if (!string.IsNullOrEmpty(workerName))
        {
            query = query.Where(w => w.WorkerName == workerName);
        }

        var fromDate = DateTime.UtcNow.AddDays(-days);
        query = query.Where(w => w.ExecutionDate >= fromDate);

        return await query.OrderByDescending(w => w.ExecutionDate).ToListAsync();
    }

    /// <summary>
    /// Force reset execution for a specific date (for manual reprocessing)
    /// </summary>
    public async Task ResetExecutionAsync(string workerName, DateTime date)
    {
        var startOfDay = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        var endOfDay = startOfDay.AddDays(1);

        var execution = await context.WorkerExecutions
            .FirstOrDefaultAsync(w => w.WorkerName == workerName && 
                                    w.ExecutionDate >= startOfDay && 
                                    w.ExecutionDate < endOfDay);

        if (execution != null)
        {
            context.WorkerExecutions.Remove(execution);
            await context.SaveChangesAsync();

            logger.LogWarning("Reset execution for worker {WorkerName} on date {Date}", workerName, startOfDay);
        }
    }
}
