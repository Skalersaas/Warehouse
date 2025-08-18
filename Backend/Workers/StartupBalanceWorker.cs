using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackgroundServices;

/// <summary>
/// Simple worker to trigger balance processing on application startup
/// Uses DailyBalanceWorker to execute the logic
/// </summary>
public class StartupBalanceWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StartupBalanceWorker> _logger;

    public StartupBalanceWorker(
        IServiceProvider serviceProvider,
        ILogger<StartupBalanceWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("StartupBalanceWorker starting...");

        // Start processing on application startup
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(3000, cancellationToken); // Wait for app initialization
                await ProcessOnStartup();
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Startup processing cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during startup processing");
            }
        }, cancellationToken);

        _logger.LogInformation("StartupBalanceWorker started");
        return;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("StartupBalanceWorker stopped");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes once on application startup
    /// </summary>
    private async Task ProcessOnStartup()
    {
        try
        {
            _logger.LogInformation("Running balance processing on application startup...");

            using var scope = _serviceProvider.CreateScope();
            var dailyWorker = scope.ServiceProvider.GetRequiredService<DailyBalanceWorker>();

            // Simply call the method from DailyBalanceWorker
            var result = await dailyWorker.ProcessManuallyAsync(DateTime.Today);

            if (result.Success)
            {
                _logger.LogInformation("Startup processing completed: {Result}", result);
            }
            else
            {
                _logger.LogError("Startup processing failed: {Error}", result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error during startup processing");
        }
        finally
        {
            _logger.LogInformation("StartupBalanceWorker finished and exiting");
        }
    }
}