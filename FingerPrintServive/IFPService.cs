using Etkezes_Models.ViewModels;

using SourceAFIS;

using ZkTecoFingerPrint;

namespace FingerPrintService
{
    public interface IFPService
    {
        event EventHandler<FPRegisteredEventArgs>? FingerPrintRegistered;
        event EventHandler<FPMessageChangedEventArgs>? MessageChanged;
        event EventHandler<FPSuccessIdentificationEventArgs>? SuccessIdentification;

        Task<bool> AddFingerprintAsync(string fingerPrintBase64, int fId);
        int Matching(List<FingerPrintData> fingerPrints);
        bool ClearDb();
        void Close();
        bool DeviceConnected();
        bool DeviceInit();
        void Dispose();
        //bool MatchFingerprint(ZkFingerPrintResult fingerprintResult, List<UserFp> users);
        Task NewFingerprintAsync(int index);
        Task Open();
        void SwitchIdentifyMode(bool identify);
    }
}