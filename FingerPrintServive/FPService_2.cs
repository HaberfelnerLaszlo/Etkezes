using System;
using System.Collections.Generic;
using System.Text;

using ZkTecoFingerPrint;
using System.Reactive.Linq;
using SourceAFIS;
using Microsoft.Extensions.Logging; // <- Added to make Observable.Where/Select/Do available for IObservable<T>

namespace FingerPrintService
{
    public class FPService_2(ILogger<FPService_2> logger) : IFPService
    {
        public event EventHandler<FPRegisteredEventArgs>? FingerPrintRegistered;
        public event EventHandler<FPMessageChangedEventArgs>? MessageChanged;
        public event EventHandler<FPSuccessIdentificationEventArgs>? SuccessIdentification;
        IDisposable deviceObservable;
        bool isConnected = false;

        public bool AddFingerprint(string fingerPrintBase64, int fId)
        {

            return true;
        }

        public bool ClearDb()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            logger.LogInformation("Closing ZkTeco Fingerprint Device...");
            return;
        }

        public bool DeviceConnected()
        {
            logger.LogInformation($"Checking for connected fingerprint devices...{isConnected}");
            return isConnected;
        }

        public bool DeviceInit()
        {
            // Initialize the ZkTeco Library first
            ZkTecoFingerHost.Initialize();

            // Get how many ZkTeco devices are connected
            var deviceCount = ZkTecoFingerHost.GetDeviceCount();
            if (deviceCount > 0) {
               //Open();
               Console.WriteLine("Fingerprint device connected: " + deviceCount);
                isConnected = true;
                return true;}
            else {return false; }
        }

        public void Dispose()
        {
            deviceObservable.Dispose();
            ZkTecoFingerHost.Close();
        }

        public void NewFingerprint(int index)
        {
            throw new NotImplementedException();
        }

        public async Task Open()
        {
            string fPTemplate = string.Empty;
            /*
        Watching a device for fingerprints
        PS: if device count is 1, deviceIndex is 0
        */

            ZkTecoFingerHost.Initialize();
            logger.LogInformation("Initializing ZkTeco Fingerprint Device...");

            var deviceCount = ZkTecoFingerHost.GetDeviceCount();
            logger.LogInformation($"Number of fingerprint devices detected: {deviceCount}");
            if (deviceCount > 0)
            {
                var deviceResult = ZkTecoFingerHost.OpenDevice(0);
                if (deviceResult.IsSuccess)
                {
                    var device = deviceResult.Value;
                    while (device !=null)
                    {
                        var fingerprintResult = await device.AcquireFingerprintAsync();
                        if (fingerprintResult.IsSuccess)
                        {
                            logger.LogInformation("Fingerprint acquired successfully.");
                            var fingerprint = fingerprintResult.Value;
                            /*
                             * ....
                             */
                        }
                    }
                }
                else {logger.LogError($"Failed to open fingerprint device. {deviceResult.Response}"); }
            }




            //     deviceObservable = ZkTecoFingerHost.ObserveDevice(deviceIndex: 0)
            //                     .Where(deviceResult => deviceResult.IsSuccess)
            //                     .Select(deviceResult => deviceResult.Value)
            //                     .Do(fingerPrintResult =>
            //                     {
            //                         var bitmapImage = fingerPrintResult.Bitmap;
            //                         var fingerPrintTemplate = fingerPrintResult.Template; 
            //                         fPTemplate = fingerPrintTemplate.ToString()??string.Empty;
            //                         logger.LogInformation("Open fut-observerDo");
            //          if (!string.IsNullOrEmpty(fPTemplate))
            //         FingerPrintRegistered?.Invoke(this, new FPRegisteredEventArgs(1,fPTemplate,fingerPrintResult));
            //                    /*
            //                          * ...store in a db or whatever
            //                          */
            //                     })
            //                     .Subscribe(onNext: (i) => Console.WriteLine(i.DeviceInfo),
            //onError: (err) =>
            //{
            //    logger.LogError(err, "Error occurred while observing fingerprint device.");
            //});
            logger.LogInformation("ZkTeco Fingerprint Device initialized and observing for fingerprints...");
            isConnected = true;
        }
         public bool MatchFingerprint(ZkFingerPrintResult fingerprintResult, List<UserFp> users)
        {
            logger.LogInformation("Matching fingerprint...");
            var minimumSimilarity = 50;
            var matchedUser = fingerprintResult.Identify<UserFp>(candidates: users,
                                    templateSelector: user => user.FingerprintTemplate,
                                    threshold: minimumSimilarity);
            return true;
        }
        public void SwitchIdentifyMode(bool identify)
        {
            throw new NotImplementedException();
        }

        public bool MatchFingerprint(FingerprintTemplate fingerprintTemplate, List<UserFp> users)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddFingerprintAsync(string fingerPrintBase64, int fId)
        {
            throw new NotImplementedException();
        }

        public Task NewFingerprintAsync(int index)
        {
            throw new NotImplementedException();
        }
    }
}
