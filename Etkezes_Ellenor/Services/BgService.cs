

using FingerPrintService;

namespace Etkezes_Ellenor.Services
{
    public class BgService : IHostedLifecycleService ,IDisposable
    {
        private readonly FPService _fpService;
        private readonly ILogger<BgService> _logger;

        public BgService(FPService fpService, ILogger<BgService> logger)
        {
            _fpService = fpService;
            _logger = logger;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
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
