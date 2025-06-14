using ScanEventsWorker.Interfaces;

namespace ScanEventsWorker.Workers
{
    public class ScanEventWorker(IServiceScopeFactory scopeFactory, ILogger<ScanEventWorker> logger) : BackgroundService
    {
        private readonly ILogger<ScanEventWorker> _logger = logger;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var scanEventService = scope.ServiceProvider.GetRequiredService<IScanEventService>();
                    var parcelService = scope.ServiceProvider.GetRequiredService<IParcelService>();

                    var scanEvents = await scanEventService.FetchScanEventsAsync();
                    if (scanEvents != null && scanEvents.Count > 0)
                    {
                        foreach(var scanEvent in scanEvents)
                        {
                            await parcelService.ProcessAndSaveParcelAsync(scanEvent);
                            await scanEventService.SaveScanEventAsync(scanEvent);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to fetch scan events.");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
