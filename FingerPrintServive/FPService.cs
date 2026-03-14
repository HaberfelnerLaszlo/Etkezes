using libzkfpcsharp;
using Microsoft.Extensions.Logging;

using SourceAFIS;

using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

using ZkTecoFingerPrint;
namespace FingerPrintService
{
    public class FPService : IDisposable, IFPService
    {
        IntPtr mDevHandle = IntPtr.Zero;
        IntPtr mDBHandle = IntPtr.Zero;
        //string mDevName = "";
        int devCount = 0;
        public string ErrorInfo = "", SuccessInfo = "";
        bool bIsTimeToDie = false;
        bool IsRegister = false;
        bool bIdentify = true;
        byte[]? FPBuffer;
        int RegisterCount = 0;
        const int REGISTER_FINGER_COUNT = 3;
        const int DEVICE_OPEN = 100;
        const int SUCCESS_DB_ADD = 201;
        private readonly ILogger<FPService> _logger;
        byte[][] RegTmps = new byte[3][];
        byte[] RegTmp = new byte[2048];
        byte[] CapTmp = new byte[2048];
        int cbCapTmp = 2048;
        int cbRegTmp = 0;
        int iFid = 1;
        Thread? captureThread = null;

        private int mfpWidth = 0;
        private int mfpHeight = 0;
        public event EventHandler<FPMessageChangedEventArgs>? MessageChanged;
        public event EventHandler<FPRegisteredEventArgs>? FingerPrintRegistered;
        public event EventHandler<FPSuccessIdentificationEventArgs>? SuccessIdentification;
        public FPService(ILogger<FPService> logger)
        {
            _logger = logger;
            bool flowControl = DeviceInit();
            if (!flowControl)
            {
                return;
            }
        }

        public bool DeviceInit()
        {
            int ret = zkfperrdef.ZKFP_ERR_OK;
//            if ((ret = zkfp2.Init()) == zkfperrdef.ZKFP_ERR_OK)
            if ((ret = ZkfpLinux.ZKFPM_Init()) == zkfperrdef.ZKFP_ERR_OK)
            {
                //devCount = zkfp2.GetDeviceCount();
                devCount = ZkfpLinux.ZKFPM_GetDeviceCount(); 
                
                if (devCount > 0)
                {
                _logger.LogInformation("Number of devices connected: {DevCount}", devCount);
                    if (devCount > 1)
                    {
                        ErrorInfo = devCount + " darab eszköz csatlakoztatva!";
                        MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                        return false;
                    }
                    SuccessInfo = "Eszköz csatlakoztatva, kész a használatra!";  
                    _logger.LogInformation(SuccessInfo);
                    MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false, DEVICE_OPEN));
                }
            }
            else
            {
                ZkfpLinux.ZKFPM_Terminate();
                ErrorInfo = $"Nincs csatlakoztatott eszköz! devCount: {devCount}";
                _logger.LogError(ErrorInfo);
                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Service destructor to ensure that the device handle is properly closed when the service is disposed of. This prevents resource leaks and ensures that the fingerprint device is released correctly.
        /// </summary>
        ~FPService()
        {
            if (mDevHandle != IntPtr.Zero)
            {
                _logger.LogInformation("Destructor called, closing device handle...");
                ZkfpLinux.ZKFPM_CloseDevice(mDevHandle);
                mDevHandle = IntPtr.Zero;
            }
        }
        public async Task Open()
        {
            if (devCount < 1)
            {
                _logger.LogWarning("No devices connected, attempting to reinitialize...");
                bool flowControl = DeviceInit();
                if (!flowControl)
                {
                    return;
                }
            }

            int ret = zkfp.ZKFP_ERR_OK;
            if (IntPtr.Zero == mDevHandle )
 {
            Console.WriteLine("Opening device...");
                if (IntPtr.Zero == (mDevHandle = ZkfpLinux.ZKFPM_OpenDevice(0)))
            {
                ErrorInfo = "Eszköz megnyitása sikertelen!";
                _logger?.LogError(ErrorInfo+mDevHandle.ToString());
                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                return;
            }
            }
            if (IntPtr.Zero == (mDBHandle = ZkfpLinux.ZKFPM_DBInit()))
            {
                ErrorInfo = "DB inicializálása sikertelen!"; 
                _logger?.LogError(ErrorInfo);
                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                ZkfpLinux.ZKFPM_CloseDevice(mDevHandle);
                mDevHandle = IntPtr.Zero;
                return;
            }
            RegisterCount = 0;
            cbRegTmp = 0;
            //iFid = 1;
            for (int i = 0; i < 3; i++)
            {
                RegTmps[i] = new byte[2048];
            }
            byte[] paramValue = new byte[4];
            int size = 4;
            ZkfpLinux.ZKFPM_GetParameters(mDevHandle, 1, paramValue, ref size);
            mfpHeight=ByteIntConverter.ToInt(paramValue, bigEndian: false);
            size = 4;
            ZkfpLinux.ZKFPM_GetParameters(mDevHandle, 2, paramValue, ref size);
            mfpWidth = ByteIntConverter.ToInt(paramValue, bigEndian: false);
            FPBuffer = new byte[mfpWidth * mfpHeight];

            captureThread = new Thread(new ThreadStart(DoCapture));
            captureThread.IsBackground = true;
            captureThread.Start();
            bIsTimeToDie = false;

        }
        private void DoCapture()
        {
            Console.WriteLine("Starting fingerprint capture thread...");
            while (!bIsTimeToDie)
            {
                cbCapTmp = 2048;
                int ret = ZkfpLinux.ZKFPM_AcquireFingerprint(mDevHandle, FPBuffer, (uint)FPBuffer!.Length, CapTmp, ref cbCapTmp);
                if (ret == zkfp.ZKFP_ERR_OK)
                {
                    // Process the captured fingerprint data (FPBuffer) as needed
                    Console.WriteLine("Fingerprint captured successfully, processing...");
                    Register();
                }
                Thread.Sleep(200);
            }
            Console.WriteLine("Exiting fingerprint capture thread...");
        }
        private void Register()
        {
            //MemoryStream ms = new MemoryStream();
            //BitmapFormat.GetBitmap(FPBuffer, mfpWidth, mfpHeight, ref ms);
            //Bitmap bmp = new Bitmap(ms);
            if (IsRegister)
            {
                Console.WriteLine("Registering fingerprint...");
                int ret = zkfp.ZKFP_ERR_OK;
                int fid = 0, score = 0;
                if (cbRegTmp <= 0)
                {
                    ErrorInfo = "Kérem, először regisztrálja az ujjlenyomatát!";
                    MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, -1));
                    cbRegTmp=RegTmp.Length;
                    Console.WriteLine("First registration, skipping identification check..."+$"RegTmp hossz: {cbRegTmp}");
                    //IsRegister = true;
                    return;
                }
                ret = ZkfpLinux.ZKFPM_DBIdentify(mDBHandle, CapTmp, (uint)CapTmp.Length, out uint fidUint, out uint scoreUint);
                fid = (int)fidUint;
                score = (int)scoreUint;
                if (zkfp.ZKFP_ERR_OK == ret)
                {
                    ErrorInfo = "Ujjlenyomat már regisztrálva van, fid= " + fid + "!";
                    MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, ret));
                    return;
                }
                //int APICALL ZKFPM_DBMatch (HANDLE hDBCache, unsigned char* fpTemplate1, unsigned int cbfpTemplate1, unsigned char* fpTemplate2, unsigned int cbfpTemplate2);
                if (RegisterCount > 0 && ZkfpLinux.ZKFPM_DBMatch(mDBHandle, CapTmp, CapTmp.Length, RegTmps[RegisterCount - 1], RegTmps[RegisterCount - 1].Length, ref fid) <= 0)
                {
                    SuccessInfo = "Kérem, érintse meg ugyanazt az ujjlenyomatot 3 alkalommal a regisztrációhoz";
                    MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false));
                    return;
                }

                Array.Copy(CapTmp, RegTmps[RegisterCount], cbCapTmp);
                String strBase64 = Base64Converter.ToBase64(CapTmp, false, false);
                byte[] blob = Base64Converter.ToBytes(strBase64).Bytes;
                RegisterCount++;
                if (RegisterCount >= REGISTER_FINGER_COUNT)
                {
                    RegisterCount = 0;
                    if (zkfp.ZKFP_ERR_OK == (ret = ZkfpLinux.ZKFPM_DBMerge(mDBHandle, RegTmps[0], RegTmps[1], RegTmps[2], RegTmp, ref cbRegTmp)) &&
                           zkfp.ZKFP_ERR_OK == (ret = ZkfpLinux.ZKFPM_DBAdd(mDBHandle, iFid, RegTmp,RegTmp.Length)))
                    {
                        string fingerTmpBase64 = Base64Converter.ToBase64(RegTmp, false, false);
                        FingerPrintRegistered?.Invoke(this, new FPRegisteredEventArgs(iFid, fingerTmpBase64));
                        SuccessInfo = "Sikeres regisztráció";
                        MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false));
                        iFid = (iFid >= 1000) ? iFid - 999 : iFid + 1;
                    }
                    else
                    {
                        ErrorInfo = "Sikertelen regisztráció, hibakód=" + ret;
                        MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, ret));
                    }
                    IsRegister = false;
                    return;
                }
                else
                {
                    SuccessInfo = "Sikeres regisztráció, érintse meg a szennert még " + (REGISTER_FINGER_COUNT - RegisterCount) + " ujjlenyomat szükséges";
                    MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false));
                }
            }
            else
            {
                //if (cbRegTmp <= 0)
                //{
                //    ErrorInfo = "Kérem, először regisztrálja az ujjlenyomatát!";
                //    MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, -1));
                //    //IsRegister = true;
                //    return;
                //}
                Console.WriteLine("Identifying fingerprint...");
                int fid = 0, score = 0;
                if (bIdentify)
                {
                    int ret = zkfp.ZKFP_ERR_OK;
                     
                    ret = ZkfpLinux.ZKFPM_DBIdentify(mDBHandle, CapTmp, (uint)CapTmp.Length, out uint fidUint, out uint scoreUint);
                     fid = (int)fidUint;
                     score = (int)scoreUint;
                    if (zkfp.ZKFP_ERR_OK == ret)
                    {
                        SuccessInfo = "Sikeres azonosítás, fid= " + fid + ",score=" + score + "!";
                        SuccessIdentification?.Invoke(this, new FPSuccessIdentificationEventArgs(fid, score));
                        //MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false));
                        return;
                    }
                    else
                    {
                        ErrorInfo = "Sikertelen azonosítás, hibakód= " + ret;
                        MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, ret));
                        return;
                    }
                }
                else
                {
                    int ret = ZkfpLinux.ZKFPM_DBMatch(mDBHandle, CapTmp, CapTmp.Length, RegTmp, RegTmp.Length,ref fid);
                    if (0 < ret)
                    {
                        SuccessInfo = "Sikeres ujjlenyomat egyeztetés, score=" + ret + "!";
                        MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false));
                        return;
                    }
                    else
                    {
                        ErrorInfo = "Sikertelen ujjlenyomat egyeztetés, hibakód= " + ret;
                        MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, ret));
                        return;
                    }
                }
            }
        }
        public async Task NewFingerprintAsync(int index)
        {
            if (index < 1 || index > 3)
            {
                ErrorInfo = "Érvénytelen index, csak 1-3 között lehet!";
                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, -1));
                return;
            }
            else if (index == 2)
            {
                iFid += 1000;
            }
            IsRegister = true;
            cbRegTmp = 2048;
            if (mDevHandle == IntPtr.Zero)
            {
                await Open();
            }
        }
        public bool ClearDb()
        {
            try
            {
                if (mDBHandle != IntPtr.Zero)
                {
                    ZkfpLinux.ZKFPM_DBFree(mDBHandle);
                    mDBHandle = IntPtr.Zero;
                }
                mDBHandle = ZkfpLinux.ZKFPM_DBInit();
                SuccessInfo = "Sikeres adatbázis törlés";
                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false));
                return true;
            }
            catch (Exception ex)
            {
                ErrorInfo = $"Hiba történt az adatbázis törlése során: {ex.Message}";
                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                return false;
            }
        }
        public async Task<bool> AddFingerprintAsync(string fingerPrintBase64, int fId)
        {
            try
            {
                if (mDevHandle == IntPtr.Zero)
                {
                    await Open();
                }
                byte[] blob = Base64Converter.ToBytes(fingerPrintBase64).Bytes;
                int fid = 0, score = 0;
                int ret = ZkfpLinux.ZKFPM_DBIdentify(mDBHandle, blob, (uint)blob.Length, out uint fidUint, out uint scoreUint);
                fid = (int)fidUint;
                score = (int)scoreUint;
                if (zkfp.ZKFP_ERR_OK == ret)
                {
                    ErrorInfo = "Ujjlenyomat már regisztrálva van, fid= " + fid + "!";
                    _logger.LogWarning(ErrorInfo, fid);
                    MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, ret));
                    return true;
                }
                ret = ZkfpLinux.ZKFPM_DBAdd(mDBHandle, fId, blob, blob.Length);
                if (ret == zkfp.ZKFP_ERR_OK)
                {
                    FingerPrintRegistered?.Invoke(this, new FPRegisteredEventArgs(fId, fingerPrintBase64));
                    SuccessInfo = "Sikeres adatbázis regisztráció";
                    MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(SuccessInfo, false, SUCCESS_DB_ADD));
                    //cbRegTmp = blob.Length;
                    if (fId < 1000)
                    {
                        iFid = fId + 1;
                    }
                    return true;
                }
                else
                {
                    ErrorInfo = "Sikertelen adatbázis regisztráció, hibakód=" + ret;
                    MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true, ret));
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorInfo = $"Hiba történt az adatbázis regisztráció során: {ex.Message}";
                MessageChanged?.Invoke(this, new FPMessageChangedEventArgs(ErrorInfo, true));
                return false;
            }
        }
        public bool DeviceConnected()
        {
            //_logger.LogInformation($"Checking device connection status... Device handle: {mDevHandle}");
            return mDevHandle != IntPtr.Zero;

        }
        public void SwitchIdentifyMode(bool identify)
        {
            bIdentify = identify;
        }
        public void Close()
        {
            bIsTimeToDie = true;
            if (captureThread != null && captureThread.IsAlive)
            {
                captureThread.Join();
            }
            if (mDBHandle != IntPtr.Zero)
            {
                ZkfpLinux.ZKFPM_DBFree(mDBHandle);
                mDBHandle = IntPtr.Zero;
            }
            if (mDevHandle != IntPtr.Zero)
            {
                ZkfpLinux.ZKFPM_CloseDevice(mDevHandle);
                mDevHandle = IntPtr.Zero;
            }
        }
        public void Dispose() => Close();

        public bool MatchFingerprint(ZkFingerPrintResult fingerprintResult, List<UserFp> users)
        {
            throw new NotImplementedException();
        }
    }
    public class FPMessageChangedEventArgs : EventArgs
    {
        public string Message { get; private set; }
        public int Code { get; private set; } = 0;
        public bool IsError { get; private set; }
        public FPMessageChangedEventArgs(string message, bool isError, int code = 0)
        {
            Message = message;
            IsError = isError;
            Code = code;
        }
    }
    public class FPRegisteredEventArgs : EventArgs 
    {
        public int Fid { get; private set; } 
        public string FingerPrint { get; private set; }
        public ZkFingerPrintResult Result { get; private set; }
        public FPRegisteredEventArgs(int fid, string fingerPrint)
        {
            Fid = fid;
            FingerPrint = fingerPrint;
        }
    }
    public class FPSuccessIdentificationEventArgs : EventArgs
    {
        public int Fid { get; private set; }
        public int Score { get; private set; }
        public FPSuccessIdentificationEventArgs(int fid, int score)
        {
            Fid = fid;
            Score = score;
        }
    }
}