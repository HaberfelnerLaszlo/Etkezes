

using FingerPrintService;

using Microsoft.FluentUI.AspNetCore.Components;

namespace Etkezes_Ellenor.Services
{
    public class BgService : IHostedLifecycleService ,IDisposable
    {
        private readonly IFPService _fpService;
        private readonly SyncService _syncService;
        private readonly LoginUserService _loginUserService;
        private readonly ILogger<BgService> _logger;
        private readonly UserService _userService;
        //private readonly IToastService _toastService;

        public BgService(IFPService fpService,SyncService syncService,LoginUserService loginUserService, UserService userService,  ILogger<BgService> logger)
        {
            _fpService = fpService;
            _syncService = syncService;
            _loginUserService = loginUserService;
            _userService = userService;
           // _toastService = toastService;
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
            if (_fpService.ClearDb())
            {
                _loginUserService.LoginUsersLoad().Wait(cancellationToken);
                _userService.UserLoading().Wait(cancellationToken);
                _fpService.SwitchIdentifyMode(true);
                // _toastService.ShowSuccess("Azonosítás bekapcsolva.");
            }
            //else _toastService.ShowError("Nem sikerült törölni az adatbázist, az azonosítás nem kapcsolható be!");
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
