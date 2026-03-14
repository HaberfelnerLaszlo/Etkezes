

using FingerPrintService;

namespace Etkezes_Ellenor.Services
{
    public class BgService : IHostedLifecycleService ,IDisposable
    {
        private readonly IFPService _fpService;
        private readonly SyncService _syncService;
        private readonly ILogger<BgService> _logger;

        public BgService(IFPService fpService,SyncService syncService, ILogger<BgService> logger)
        {
            _fpService = fpService;
            _syncService = syncService;
            _logger = logger;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if(await _syncService.GetSyncDates()) {_logger.LogInformation("Adatbázis szinkronizálás."); }
            else { _logger.LogInformation($"Adatbázis szinkronizálás sikertelen. Hibaüzenet: {_syncService.ErrorMessage}"); }
            await Task.CompletedTask;
        }

        public Task StartedAsync(CancellationToken cancellationToken)
        {
            var message = _fpService.DeviceConnected() ? "Fingerprint device connected." : "Fingerprint device not connected.";
            _logger.LogInformation(message);
            return Task.CompletedTask;
        }

        public Task StartingAsync(CancellationToken cancellationToken)
        {
            _fpService.Open();
            //_fpService.DeviceInit();
            _logger.LogInformation("Fingerprint service started.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _fpService.Dispose();
            _logger.LogInformation("Fingerprint service stopped.");
            return Task.CompletedTask;
        }

        public Task StoppedAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StoppingAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        public void Dispose()
        {
            _fpService.Dispose();
            _logger.LogInformation("Fingerprint service stopped.");
        }
    }
}
